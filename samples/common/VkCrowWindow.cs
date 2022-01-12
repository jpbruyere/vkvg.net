// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Glfw;
using vke;
using Vulkan;
using Crow;
using Drawing2D;

namespace vke {
	public class VkTextureBackend : Crow.CairoBackend.ImageBackend {
		public IntPtr textureDataHandle;
		public int stride;
		public VkTextureBackend (IntPtr mappedData, int width, int height, int stride)
		: base (mappedData, width, height, stride) {
			this.stride = stride;
		}
		public override void ResizeMainSurface(int width, int height)
		{
			//surf?.Dispose ();
			surf = new Crow.CairoBackend.ImageSurface(textureDataHandle, Format.ARGB32, width, height, stride);
		}
	}
	public class VkCrowInterface : Interface {
		public Image uiImage;
		public VkCrowInterface (IntPtr glfwWinHandle, Image uiImage) : base ((int)uiImage.Width, (int)uiImage.Height, glfwWinHandle) {
			this.uiImage = uiImage;
			backend = new VkTextureBackend (uiImage.MappedData, (int)uiImage.Width,
				(int)uiImage.Height, (int)uiImage.GetSubresourceLayout ().rowPitch);
			clipping = Backend.CreateRegion ();
		}
		public override void ProcessResize(Rectangle bounds)
		{
			VkTextureBackend vkb = backend as VkTextureBackend;
			vkb.textureDataHandle = uiImage.MappedData;
			vkb.stride = (int)uiImage.GetSubresourceLayout ().rowPitch;
			base.ProcessResize(bounds);
		}
	}
	/// <summary>
	/// Vulkan context with Crow enabled window.
	/// Crow vector drawing is handled with Cairo Image on an Host mapped vulkan image.
	/// This is an easy way to have GUI in my samples with low GPU cost. Most of the ui
	/// is cached on cpu memory images.
	/// </summary>
	public class VkCrowWindow : VkWindow, IValueChange {
		#region IValueChange implementation
		public event EventHandler<Crow.ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged (string MemberName, object _value)
		{
			ValueChanged?.Invoke (this, new Crow.ValueChangeEventArgs (MemberName, _value));
		}
		#endregion

		public VkCrowWindow () : base ("VkWindow", 800, 600, false) { }

		protected VkCrowInterface iFace;
		public bool MouseIsInInterface =>
			iFace.HoverWidget != null;
		public Device Dev => dev;
		public CommandPool CmdPool => cmdPool;
		public Queue GraphicQueue => presentQueue;

		protected DescriptorSetWrites uiImageUpdate;

		protected override void initVulkan () {
			base.initVulkan ();

			iFace = new VkCrowInterface (WindowHandle, createUITexture ());
			iFace.Init ();
		}
		public override void Update ()
		{
			NotifyValueChanged ("fps", fps);
			iFace.Update ();
		}

		protected override void onMouseMove (double xPos, double yPos)
		{
			if (iFace.OnMouseMove ((int)xPos, (int)yPos))
				return;
			base.onMouseMove (xPos, yPos);
		}
		protected override void onMouseButtonDown (MouseButton button) {
			if (iFace.OnMouseButtonDown (button))
				return;
			base.onMouseButtonDown (button);
		}
		protected override void onMouseButtonUp (MouseButton button)
		{
			if (iFace.OnMouseButtonUp (button))
				return;
			base.onMouseButtonUp (button);
		}
		protected override void onChar (CodePoint cp) {
			if (iFace.OnKeyPress (cp.ToChar()))
				return;
			base.onChar (cp);
		}
		protected override void onKeyUp (Key key, int scanCode, Modifier modifiers) {
			if (iFace.OnKeyUp (new KeyEventArgs (key, scanCode, modifiers)))
				return;
			base.onKeyUp (key, scanCode, modifiers);
		}
		protected override void onKeyDown (Key key, int scanCode, Modifier modifiers) {
			if (iFace.OnKeyDown (new KeyEventArgs (key, scanCode, modifiers)))
				return;
			base.onKeyDown (key, scanCode, modifiers);
		}

		protected override void OnResize ()
		{
			base.OnResize ();

			dev.WaitIdle ();
			iFace.MainSurface.Dispose();
			iFace.uiImage.Dispose();
			iFace.uiImage = createUITexture();
			dev.WaitIdle ();

			iFace.ProcessResize (new Rectangle (0,0,(int)Width, (int)Height));
		}

		protected override void Dispose (bool disposing)
		{
			dev.WaitIdle ();
			iFace.uiImage?.Dispose ();
			iFace.Dispose ();
			base.Dispose (disposing);
		}


		Image createUITexture ()
		{
			Image uiImage = new Image (dev, VkFormat.B8g8r8a8Unorm, VkImageUsageFlags.Sampled,
				VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent, Width, Height, VkImageType.Image2D,
				VkSampleCountFlags.SampleCount1, VkImageTiling.Linear);
			uiImage.CreateView (VkImageViewType.ImageView2D, VkImageAspectFlags.Color);
			uiImage.CreateSampler (VkFilter.Nearest, VkFilter.Nearest, VkSamplerMipmapMode.Nearest, VkSamplerAddressMode.ClampToBorder);
			uiImage.Descriptor.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;
			uiImage.SetName ("uiImage");
			uiImage.Map ();

			PrimaryCommandBuffer cmd = cmdPool.AllocateAndStart (VkCommandBufferUsageFlags.OneTimeSubmit);
			uiImage.SetLayout (cmd, VkImageAspectFlags.Color,
				VkImageLayout.Undefined, VkImageLayout.ShaderReadOnlyOptimal);
			presentQueue.EndSubmitAndWait (cmd, true);

			NotifyValueChanged ("uiImage", uiImage);

			uiImageUpdate?.Write (dev, uiImage.Descriptor);
			return uiImage;
		}

		protected void loadWindow (string path, object dataSource = null) {
			try {
				Widget w = iFace.FindByName (path);
				if (w != null) {
					iFace.PutOnTop (w);
					return;
				}
				w = iFace.Load (path);
				w.Name = path;
				w.DataSource = dataSource;

			} catch (Exception ex) {
				System.Diagnostics.Debug.WriteLine (ex.ToString ());
			}
		}
		protected void closeWindow (string path) {
			Widget g = iFace.FindByName (path);
			if (g != null)
				iFace.DeleteWidget (g);
		}

	}
}
