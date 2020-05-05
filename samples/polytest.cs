// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Linq;
using Glfw;
using vke;
using Vulkan;

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

	public class ValueContainer<T> : Crow.IValueChange
	{
		public event EventHandler<Crow.ValueChangeEventArgs> ValueChanged;
		T val;
		public T Value {
			get => val;
			set {
				if (EqualityComparer<T>.Default.Equals (value, val))
					return;
				val = value;
				ValueChanged?.Invoke (this, new Crow.ValueChangeEventArgs ("Value", val));
			}
		}
		public ValueContainer (T _val) { val = _val; }
	}

	public class polytest : VkCrowWindow
	{
		static void Main (string [] args)
		{
			SwapChain.PREFERED_FORMAT = VkFormat.B8g8r8a8Unorm;
			Instance.VALIDATION = true;
			using (polytest vke = new polytest ()) {
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

		void init_main_pipeline () {
			cmds = cmdPool.AllocateCommandBuffer (swapChain.ImageCount);

			dsPool = new DescriptorPool (dev, 2, new VkDescriptorPoolSize (VkDescriptorType.CombinedImageSampler, 2));
			dslMain = new DescriptorSetLayout (dev,
				new VkDescriptorSetLayoutBinding (0, VkShaderStageFlags.Fragment, VkDescriptorType.CombinedImageSampler));

			GraphicPipelineConfig cfg = GraphicPipelineConfig.CreateDefault (VkPrimitiveTopology.TriangleList, VkSampleCountFlags.SampleCount1);
			cfg.RenderPass = new RenderPass (dev, swapChain.ColorFormat, VkSampleCountFlags.SampleCount1);
			cfg.Layout = new PipelineLayout (dev, dslMain);
			cfg.blendAttachments [0] = new VkPipelineColorBlendAttachmentState (true);
			cfg.AddShader (VkShaderStageFlags.Vertex, "#vke.FullScreenQuad.vert.spv");
			cfg.AddShader (VkShaderStageFlags.Fragment, "#polytest.simpletexture.frag.spv");
			plMain = new GraphicPipeline (cfg);

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

			this.Dashes.Add (new ValueContainer<float>(20));
			this.Dashes.Add (new ValueContainer<float> (10));
		}


		bool closedPath;
		uint lineWidth = 20;
		vkvg.LineJoin lineJoin = vkvg.LineJoin.Miter;
		vkvg.LineCap lineCap = vkvg.LineCap.Butt;
		Crow.Color fillColor = Crow.Color.RoyalBlue.AdjustAlpha (0.7f);
		Crow.Color strokeColor = Crow.Color.GreenYellow.AdjustAlpha (0.6f);
		bool enableDash;
		Crow.ObservableList<ValueContainer<float>> dashes = new Crow.ObservableList<ValueContainer<float>> ();
		DrawMode currentDrawMode = DrawMode.Select;

		vkvg.Command lastCmd = null;

		public bool ClosedPath {
			get => closedPath;
			set {
				if (value == closedPath)
					return;
				closedPath = value;
				NotifyValueChanged ("ClosedPath", closedPath);
			}
		}
		public uint LineWidth {
			get => lineWidth;
			set {
				if (value == lineWidth)
					return;
				lineWidth = value;
				NotifyValueChanged ("LineWidth", lineWidth);
			}
		}
		public vkvg.LineJoin LineJoin {
			get => lineJoin;
			set {
				if (value == lineJoin)
					return;
				lineJoin = value;
				NotifyValueChanged ("LineJoin", lineJoin);
			}
		}
		public vkvg.LineCap LineCap {
			get => lineCap;
			set {
				if (value == lineCap)
					return;
				lineCap = value;
				NotifyValueChanged ("LineCap", lineCap);
			}
		}
		public Crow.Color FillColor {
			get => fillColor;
			set {
				if (value == fillColor)
					return;
				fillColor = value;
				NotifyValueChanged ("FillColor", fillColor);
			}
		}
		public Crow.Color StrokeColor {
			get => strokeColor;
			set {
				if (value == strokeColor)
					return;
				strokeColor = value;
				NotifyValueChanged ("StrokeColor", strokeColor);
			}
		}
		public bool EnableDash {
			get => enableDash;
			set {
				if (value == enableDash)
					return;
				enableDash = value;
				NotifyValueChanged ("EnableDash", enableDash);
			}
		}
		public Crow.ObservableList<ValueContainer<float>> Dashes {
			set {
				dashes = value;
				NotifyValueChanged ("Dashes", dashes);
			}
			get => dashes;
		}
		public DrawMode CurrentDrawMode {
			get => currentDrawMode;
			set {
				if (value == currentDrawMode)
					return;
				currentDrawMode = value;
				NotifyValueChanged ("CurrentDrawMode", currentDrawMode);
			}
		}

		string currentPath;
		List<string> pathes = new List<string> ();

		public string CurrentPath {
			get => currentPath;
			set {
				if (currentPath == value)
					return;
				currentPath = value;
				redraw = true;
				NotifyValueChanged ("CurrentPath", currentPath);
			}
		}

		List<vkvg.Point> points = new List<vkvg.Point> ();

		vkvg.CommandCollection commands = new vkvg.CommandCollection ();

		int curPoint = -1;
		int cpRadius = 10;
		double selRadius = 3.5;
		bool locked = false;
		bool redraw;

		bool isOver(vkvg.Point p, int x, int y) =>
			p.X - cpRadius < x && p.X + cpRadius > x && p.Y - cpRadius < y && p.Y + cpRadius > y;

		protected override void onMouseMove(double xPos, double yPos)
		{
			base.onMouseMove(xPos, yPos);

			if (MouseIsInInterface)
				return;

			if (locked)
			{
				if (curPoint < 0)
					return;
				points[curPoint] = new vkvg.Point((int)xPos, (int)yPos);
			}
			else {
				for (int i = 0; i < points.Count; i++)
				{
					if (!isOver(points[i], (int)lastMouseX, (int)lastMouseY))
						continue;
					curPoint = i;
					break;
				}
			}
		}

		protected override void onMouseButtonDown(MouseButton button)
		{
			base.onMouseButtonDown(button);

			if (MouseIsInInterface)
				return;

			if (button != Glfw.MouseButton.Left)
				return;

			if (CurrentDrawMode == DrawMode.Lines) {

				curPoint = points.Count;
				points.Add (new vkvg.Point ((int)lastMouseX, (int)lastMouseY));
			}

			locked = true;
		}
		protected override void onMouseButtonUp(MouseButton button)
		{
			base.onMouseButtonUp(button);
			locked &= button != Glfw.MouseButton.Left;
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


			//if (!redraw)
			//	return;

			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.SetSource(1, 1, 1);
				ctx.Paint();

				if (enableDash && dashes.Count > 0)
					ctx.Dashes = dashes.Select (d => d.Value).ToArray();
				ctx.LineWidth = lineWidth;
				ctx.LineJoin = lineJoin;
				ctx.LineCap = lineCap;

				ctx.SetSource (strokeColor.R, strokeColor.G, strokeColor.B, strokeColor.A);

				/*try {
					vkvg.PathParser pp = new vkvg.PathParser (currentPath);
					pp.Draw (ctx);
				} catch (Exception ex) {
					Console.WriteLine (ex);
				}*/


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
			}
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
