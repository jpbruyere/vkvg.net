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

namespace vke {
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

		public Image uiImage;
		protected Interface iFace;
		public bool MouseIsInInterface =>
			iFace.HoverWidget != null;
		public Device Dev => dev;
		public CommandPool CmdPool => cmdPool;
		public Queue GraphicQueue => presentQueue;

		protected DescriptorSetWrites uiImageUpdate;

		protected override void initVulkan () {
			base.initVulkan ();

			iFace = new Crow.Interface ((int)Width, (int)Height, WindowHandle);
			iFace.Init ();

			initUISurface ();
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
			if (iFace.OnKeyUp (key))
				return;
			base.onKeyUp (key, scanCode, modifiers);
		}
		protected override void onKeyDown (Key key, int scanCode, Modifier modifiers) {
			if (iFace.OnKeyDown (key))
				return;
			base.onKeyDown (key, scanCode, modifiers);
		}

		protected override void OnResize ()
		{
			base.OnResize ();

			iFace.ProcessResize (new Crow.Rectangle (0,0,(int)Width, (int)Height));
			initUISurface ();
		}

		protected override void Dispose (bool disposing)
		{
			dev.WaitIdle ();
			uiImage?.Dispose ();
			iFace.Dispose ();
			base.Dispose (disposing);
		}


		void initUISurface ()
		{
			iFace.surf?.Dispose ();
			uiImage?.Dispose ();

			uiImage = new Image (dev, VkFormat.B8g8r8a8Unorm, VkImageUsageFlags.Sampled,
				VkMemoryPropertyFlags.HostVisible | VkMemoryPropertyFlags.HostCoherent, Width, Height, VkImageType.Image2D,
				VkSampleCountFlags.SampleCount1, VkImageTiling.Linear);
			uiImage.CreateView (VkImageViewType.ImageView2D, VkImageAspectFlags.Color);
			uiImage.CreateSampler (VkFilter.Nearest, VkFilter.Nearest, VkSamplerMipmapMode.Nearest, VkSamplerAddressMode.ClampToBorder);
			uiImage.Descriptor.imageLayout = VkImageLayout.ShaderReadOnlyOptimal;
			uiImage.Map ();

			PrimaryCommandBuffer cmd = cmdPool.AllocateAndStart (VkCommandBufferUsageFlags.OneTimeSubmit);
			uiImage.SetLayout (cmd, VkImageAspectFlags.Color,
				VkImageLayout.Undefined, VkImageLayout.ShaderReadOnlyOptimal);
			presentQueue.EndSubmitAndWait (cmd, true);

			NotifyValueChanged ("uiImage", uiImage);

			uiImageUpdate?.Write (dev, uiImage.Descriptor);

			iFace.surf = new Crow.Cairo.ImageSurface (uiImage.MappedData, Crow.Cairo.Format.ARGB32,
				(int)Width, (int)Height, (int)uiImage.GetSubresourceLayout ().rowPitch);
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
