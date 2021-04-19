using System.Security.AccessControl;
// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Reflection;
using NativeLibrary = System.Runtime.InteropServices.NativeLibrary;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Glfw;
using vke;
using Vulkan;
using Image = vke.Image;

namespace VK
{
	public enum DrawMode
	{
		Select,
		Lines,
		Rect,
		Arc,
		Star,
		Image,
	}


	public class Polytest : VkCrowWindow
	{
#if NETCOREAPP		
		static IntPtr resolveUnmanaged (Assembly assembly, String libraryName) {
			
			switch (libraryName)
			{
				case "glfw3":
					return  System.Runtime.InteropServices.NativeLibrary.Load("glfw", assembly, null);
				case "rsvg-2.40":
					return  System.Runtime.InteropServices.NativeLibrary.Load("rsvg-2", assembly, null);
			}
			Console.WriteLine ($"[UNRESOLVE] {assembly} {libraryName}");			
			return IntPtr.Zero;
		}

		static Polytest () {
			System.Runtime.Loader.AssemblyLoadContext.Default.ResolvingUnmanagedDll+=resolveUnmanaged;
		}
#endif		
		static void Main (string [] args)
		{

			SwapChain.PREFERED_FORMAT = VkFormat.B8g8r8a8Unorm;
			Instance.VALIDATION = true;
			using (Polytest vke = new Polytest ()) {
				vke.Run ();
			}
		}

		protected vkvg.Device vkvgDev;
		protected vkvg.Surface vkvgSurf;

		Image vkvgImage;
		FrameBuffers frameBuffers;
		GraphicPipeline plMain;
		DescriptorPool dsPool;
		DescriptorSetLayout dslMain;
		DescriptorSet dsUIimage, dsVKVGimg;

		Stopwatch lastClickElapsed = new Stopwatch ();

		void init_main_pipeline () {
			cmds = cmdPool.AllocateCommandBuffer (swapChain.ImageCount);

			dsPool = new DescriptorPool (dev, 2, new VkDescriptorPoolSize (VkDescriptorType.CombinedImageSampler, 2));
			dslMain = new DescriptorSetLayout (dev,
				new VkDescriptorSetLayoutBinding (0, VkShaderStageFlags.Fragment, VkDescriptorType.CombinedImageSampler));

			GraphicPipelineConfig cfg = GraphicPipelineConfig.CreateDefault (VkPrimitiveTopology.TriangleList, VkSampleCountFlags.SampleCount1);
			cfg.RenderPass = new RenderPass (dev, swapChain.ColorFormat, VkSampleCountFlags.SampleCount1);
			cfg.Layout = new PipelineLayout (dev, dslMain);
			cfg.blendAttachments [0] = new VkPipelineColorBlendAttachmentState (true);
			cfg.AddShader (dev, VkShaderStageFlags.Vertex, "#vke.FullScreenQuad.vert.spv");
			cfg.AddShader (dev, VkShaderStageFlags.Fragment, "#polytest.simpletexture.frag.spv");
			plMain = new GraphicPipeline (cfg);
			cfg.DisposeShaders ();

			dsUIimage = dsPool.Allocate (dslMain);
			dsVKVGimg = dsPool.Allocate (dslMain);

			uiImageUpdate = new DescriptorSetWrites (dsUIimage, dslMain);
		}
		public override string [] EnabledInstanceExtensions => new string [] {
#if DEBUG
			Ext.I.VK_EXT_debug_utils,
#endif
		};

		protected override void initVulkan () {
			base.initVulkan ();

			init_main_pipeline ();

			vkvgDev = new vkvg.Device (instance.Handle, phy.Handle, dev.VkDev.Handle, presentQueue.qFamIndex, vkvg.SampleCount.Sample_8);
			UpdateFrequency = 10;//update on each frame to have effective drawing perfs

			loadWindow ("#polytest.ui.main.crow", this);

			//this.Dashes.Add (new ValueContainer<float>(20));
			//this.Dashes.Add (new ValueContainer<float> (10));
		}




		DrawMode currentDrawMode = DrawMode.Select;

		public DrawMode CurrentDrawMode {
			get => currentDrawMode;
			set {
				if (value == currentDrawMode)
					return;
				currentDrawMode = value;
				if (currentDrawMode == DrawMode.Select)
					SetCursor (CursorShape.Arrow);
				else
					SetCursor (CursorShape.Crosshair);
				NotifyValueChanged ("CurrentDrawMode", currentDrawMode);
			}
		}

		public vkvg.Shape.Shape CurrentShape {
			get => currentShape;
			set {
				if (currentShape == value)
					return;
				currentShape = value;
				NotifyValueChanged ("CurrentShape", currentShape);
			}
		}


		public Crow.ObservableList<vkvg.Shape.Shape> Shapes = new Crow.ObservableList<vkvg.Shape.Shape> ();
		vkvg.Shape.Shape currentShape;
		vkvg.PathCommand curPathCmd;
		int curPathCmdPoint;
		vkvg.PointD? lastMousePos;


		bool locked = false;
		bool redraw;


		void draw_current_path () {
			using (vkvg.Context ctx = new vkvg.Context (vkvgSurf)) {
				ctx.SetSource (1, 1, 1);
				ctx.Paint ();

				foreach (vkvg.Shape.Shape shape in Shapes) {
					shape.Draw (ctx, (currentDrawMode != DrawMode.Select && lastMousePos != null) ? lastMousePos : null);
					if (shape == currentShape)
						shape.DrawPoints (ctx, curPathCmd, curPathCmdPoint);
				}
			}
		}

		protected override void onMouseMove(double xPos, double yPos)
		{
			base.onMouseMove(xPos, yPos);

			if (MouseIsInInterface) {
				lastMousePos = null;
				return;
			}

			vkvg.PointD m = new vkvg.Point ((int)xPos, (int)yPos);

			if (locked) {
				if (currentShape != null) {
					if (curPathCmd != null) {
						curPathCmd [curPathCmdPoint] = m - currentShape.Translation;
					} else if (lastMousePos != null)
						currentShape.Translation += m - (vkvg.PointD)lastMousePos;
				}

			} else {
				if (currentShape == null) {
					curPathCmd = null;
					curPathCmdPoint = -1;
				} else {
					currentShape.IsOver (m, out curPathCmd, out curPathCmdPoint);
				}
				/*					foreach (vkvg.PathCommand cmd in currentPath.OfType<vkvg.PathCommand> ()) {
										for (int i = 0; i < cmd.Length; i++) {
											if (!isOver (cmd [i], m))
												continue;
											curPathCmd = cmd;
											curPathCmdPoint = i;
											return;
										}
									}*/
			}
			lastMousePos = m;
		}

		protected override void onMouseButtonDown (MouseButton button) {
			base.onMouseButtonDown (button);

			if (MouseIsInInterface || lastMousePos == null)
				return;

			if (button != Glfw.MouseButton.Left)
				return;

			if (lastClickElapsed.IsRunning && lastClickElapsed.ElapsedMilliseconds < 200 && currentDrawMode == DrawMode.Lines) {
				//double click
				CurrentDrawMode = DrawMode.Select;
				return;
			}
			if (currentDrawMode == DrawMode.Select) {

			} else if (currentDrawMode == DrawMode.Lines) {
				if (currentShape == null)
					Shapes.Add (CurrentShape = new vkvg.Shape.Path ((vkvg.PointD)lastMousePos));
				else
					CurrentShape.PathCommands.Add (new vkvg.Line () { A = (vkvg.PointD)lastMousePos });
			} else if (currentShape == null) {
				switch (currentDrawMode) {
				case DrawMode.Rect:

					break;
				case DrawMode.Arc:
					break;
				case DrawMode.Star:
					break;
				case DrawMode.Image:
					break;
				}
			}
			locked = true;
		}
		protected override void onMouseButtonUp(MouseButton button)
		{
			base.onMouseButtonUp(button);
			if (!(button == Glfw.MouseButton.Left && locked))
				return;
				
			if (currentDrawMode != DrawMode.Lines)
				CurrentDrawMode = DrawMode.Select;

			locked = false;
			lastClickElapsed.Restart ();
		}
		protected override void onKeyDown (Key key, int scanCode, Modifier modifiers)
		{
			base.onKeyDown (key, scanCode, modifiers);

		}
		protected override void OnResize ()
		{
			base.OnResize ();

			vkvgSurf?.Dispose ();
			vkvgSurf = new vkvg.Surface (vkvgDev, (int)Width, (int)Height);
			vkvgSurf.Clear ();

			vkvgImage?.Dispose ();
			vkvgImage = new Image (dev, new VkImage ((ulong)vkvgSurf.VkImage.ToInt64 ()), VkFormat.B8g8r8a8Unorm,
				VkImageUsageFlags.Sampled, (uint)vkvgSurf.Width, (uint)vkvgSurf.Height);
			vkvgImage.CreateView ();
			vkvgImage.CreateSampler (VkFilter.Nearest, VkFilter.Nearest, VkSamplerMipmapMode.Nearest, VkSamplerAddressMode.ClampToBorder);
			vkvgImage.Descriptor.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;

			DescriptorSetWrites dsUpdate = new DescriptorSetWrites (dsVKVGimg, dslMain);
			dsUpdate.Write (dev, vkvgImage.Descriptor);

			frameBuffers?.Dispose ();
			frameBuffers = plMain.RenderPass.CreateFrameBuffers (swapChain);

			buildCommandBuffers ();
		}

		void buildCommandBuffers ()
		{
			cmdPool.Reset (VkCommandPoolResetFlags.ReleaseResources);

			for (int i = 0; i < swapChain.ImageCount; ++i) {
				FrameBuffer fb = frameBuffers [i];
				cmds [i].Start ();

				vkvgImage.SetLayout (cmds [i], VkImageAspectFlags.Color,
					VkImageLayout.ColorAttachmentOptimal, VkImageLayout.ShaderReadOnlyOptimal);

				plMain.RenderPass.Begin (cmds [i], fb);

				cmds [i].SetViewport (swapChain.Width, swapChain.Height);
				cmds [i].SetScissor (swapChain.Width, swapChain.Height);


				cmds [i].BindPipeline (plMain);

				cmds [i].BindDescriptorSet (plMain.Layout, dsVKVGimg);
				cmds [i].Draw (3, 1, 0, 0);

				cmds [i].BindDescriptorSet (plMain.Layout, dsUIimage);
				cmds [i].Draw (3, 1, 0, 0);

				plMain.RenderPass.End (cmds [i]);

				vkvgImage.SetLayout (cmds [i], VkImageAspectFlags.Color,
					VkImageLayout.ShaderReadOnlyOptimal, VkImageLayout.ColorAttachmentOptimal);

				cmds [i].End ();
			}
		}

		public override void Update()
		{
			base.Update ();

			draw_current_path ();
			//if (!redraw)
			//	return;

			/*using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.SetSource(1, 1, 1);
				ctx.Paint();

				

				//try {
				//	vkvg.PathParser pp = new vkvg.PathParser (currentPath);
				//	pp.Draw (ctx);
				//} catch (Exception ex) {
				//	Console.WriteLine (ex);
				//}


				if (points.Count > 0) {
					ctx.MoveTo (points [0]);
					for (int i = 1; i < points.Count; i++) {
						ctx.LineTo (points [i]);
					}
					if (points.Count > 2 && closedPath) {
						ctx.ClosePath ();
						ctx.SetSource (fillColor.R, fillColor.G, fillColor.B, fillColor.A);
						ctx.FillPreserve ();
					}
					ctx.SetSource (strokeColor.R, strokeColor.G, strokeColor.B, strokeColor.A);
					ctx.LineWidth = lineWidth;
					ctx.LineJoin = lineJoin;
					ctx.LineCap = lineCap;
					ctx.StrokePreserve ();
					ctx.LineWidth = 1;
					ctx.SetSource (0.5, 0.5, 0.5, 1);
					ctx.Stroke ();
				}

				ctx.Dashes = null;

				switch (CurrentDrawMode) {
				case DrawMode.Select:
					iFace.MouseCursor = Crow.MouseCursor.Arrow;
					if (curPoint < 0)
						break;
					ctx.LineWidth = 1;
					ctx.SetSource (0.6, 0.6, 1, 0.6);
					ctx.Rectangle ((double)points [curPoint].X - selRadius, (double)points [curPoint].Y - selRadius, selRadius * 2, selRadius * 2);
					ctx.FillPreserve ();
					ctx.SetSource (0.3, 0.3, 1, 0.8);
					ctx.Stroke ();
					break;
				case DrawMode.Lines:
					iFace.MouseCursor = Crow.MouseCursor.Crosshair;
					if (points.Count == 0 || MouseIsInInterface)
						break;
					ctx.LineWidth = 1;
					ctx.SetSource (0.3, 0.2, 0.2, 0.7);
					ctx.MoveTo (points [points.Count - 1]);
					ctx.LineTo ((int)lastMouseX, (int)lastMouseY);
					ctx.Stroke ();
					break;
				case DrawMode.Rect:
					break;
				case DrawMode.Arc:
					break;
				case DrawMode.Star:
					break;
				case DrawMode.Image:
					break;
				}
			}*/
		}

		protected override void Dispose (bool disposing)
		{
			vkvgImage.Dispose ();
			dsPool.Dispose ();
			frameBuffers.Dispose ();
			plMain.Dispose ();

			vkvgSurf.Dispose ();
			vkvgDev.Dispose ();

			base.Dispose (disposing);
		}


	}
}
