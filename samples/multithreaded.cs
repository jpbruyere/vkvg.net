// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Threading;
using Glfw;
using vke;
using Vulkan;

namespace VK
{
	public class Program : VkWindow
	{
		static void Main (string [] args) {
			SwapChain.PREFERED_FORMAT = VkFormat.B8g8r8a8Unorm;
			SwapChain.IMAGES_USAGE = VkImageUsageFlags.TransferSrc | VkImageUsageFlags.TransferDst;
			Instance.VALIDATION = true;

			using (Program vke = new Program ()) {
				vke.Run ();
			}
		}

		public override string [] EnabledInstanceExtensions => new string [] {
			Ext.I.VK_EXT_debug_utils,
		};

		protected Random rnd = new Random ();
		protected uint iterations = 100;
		protected bool paused;

		protected vkvg.Device vkvgDev;
		protected vkvg.Surface vkvgSurf;

		volatile int activeThreads = 0;
		const int threadCount = 10;
		CountdownEvent countdown = new CountdownEvent (threadCount);

		void drawingThread () {
			int threadId = activeThreads;
			activeThreads++;
			//Console.WriteLine ($"Drawing thread strart {Thread.CurrentThread.ManagedThreadId}");
			lock (vkvgSurf) {
				using (vkvg.Context ctx = new vkvg.Context (vkvgSurf)) {
					//Console.WriteLine ($"{threadId}:draw");

					for (int i = 0; i < iterations; i++) {
						float x1 = (float)(rnd.NextDouble () * Width);
						float y1 = (float)(rnd.NextDouble () * Height);
						float x2 = (float)(rnd.NextDouble () * Width) + 1.0f;
						float y2 = (float)(rnd.NextDouble () * Height) + 1.0f;
						randomize_color (ctx);
						ctx.MoveTo (x1, y1);
						ctx.LineTo (x2, y2);
						ctx.Stroke ();
					}
				}
			}

			//Console.WriteLine ($"Drawing thread end {Thread.CurrentThread.ManagedThreadId}");
			activeThreads--;

			//Console.WriteLine ($"{threadId}:signal");
			countdown.Signal ();
		}

		protected override void initVulkan () {
			base.initVulkan ();
			vkvgDev = new vkvg.Device (instance.Handle, phy.Handle, dev.VkDev.Handle, presentQueue.qFamIndex, vkvg.SampleCount.Sample_1);
			UpdateFrequency = 0;//update on each frame to have effective drawing perfs
		}

		protected void randomize_color (vkvg.Context ctx) {
			ctx.SetSource (rnd.NextDouble (), rnd.NextDouble (), rnd.NextDouble (), 0.5);
		}

		protected override void onKeyDown (Key key, int scanCode, Modifier modifiers) {
			if (key == Key.Space)
				paused = !paused;
			base.onKeyDown (key, scanCode, modifiers);

		}
		public override void Update () {
			createThreads ();
			countdown.Wait ();
		}
		protected override void render () {
			int idx = swapChain.GetNextImage ();
			if (idx < 0) {
				OnResize ();
				return;
			}

			if (cmds [idx] == null)
				return;
				
			//Console.WriteLine ($"render wait threads");

			drawFence.Wait ();
			drawFence.Reset ();

			presentQueue.Submit (cmds [idx], swapChain.presentComplete, drawComplete [idx], drawFence);
			presentQueue.Present (swapChain, drawComplete [idx]);

			presentQueue.WaitIdle ();
						//createThreads ();
			
		}

		void createThreads () {
			//Console.WriteLine ($"count down reset");
			countdown.Reset ();
			for (int i = 0; i < threadCount; i++) {
				Thread t = new Thread (drawingThread);
				t.IsBackground = true;
				t.Start ();
			}
		}


		protected override void OnResize () {
			base.OnResize ();

			dev.WaitIdle ();

			vkvgSurf?.Dispose ();
			vkvgSurf = new vkvg.Surface (vkvgDev, (int)Width, (int)Height);
			vkvgSurf.Clear ();

			VkImage srcImg = new VkImage ((ulong)vkvgSurf.VkImage.ToInt64 ());

			for (int i = 0; i < swapChain.ImageCount; ++i) {

				cmds [i] = cmdPool.AllocateCommandBuffer ();
				cmds [i].Start ();

				Utils.setImageLayout (cmds [i].Handle, swapChain.images [i].Handle, VkImageAspectFlags.Color,
					VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal,
					VkPipelineStageFlags.BottomOfPipe, VkPipelineStageFlags.Transfer);
				Utils.setImageLayout (cmds [i].Handle, srcImg, VkImageAspectFlags.Color,
					VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal,
					VkPipelineStageFlags.ColorAttachmentOutput, VkPipelineStageFlags.Transfer);

				VkImageSubresourceLayers imgSubResLayer = new VkImageSubresourceLayers {
					aspectMask = VkImageAspectFlags.Color,
					mipLevel = 0,
					baseArrayLayer = 0,
					layerCount = 1
				};
				VkImageCopy cregion = new VkImageCopy {
					srcSubresource = imgSubResLayer,
					srcOffset = default,
					dstSubresource = imgSubResLayer,
					dstOffset = default,
					extent = new VkExtent3D { width = (uint)vkvgSurf.Width, height = (uint)vkvgSurf.Height }
				};
				Vk.vkCmdCopyImage (cmds [i].Handle, srcImg, VkImageLayout.TransferSrcOptimal,
					swapChain.images [i].Handle, VkImageLayout.TransferDstOptimal, 1, ref cregion);

				Utils.setImageLayout (cmds [i].Handle, swapChain.images [i].Handle, VkImageAspectFlags.Color,
					VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR,
					VkPipelineStageFlags.Transfer, VkPipelineStageFlags.BottomOfPipe);
				Utils.setImageLayout (cmds [i].Handle, srcImg, VkImageAspectFlags.Color,
					VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal,
					VkPipelineStageFlags.Transfer, VkPipelineStageFlags.ColorAttachmentOutput);

				cmds [i].End ();
			}
			dev.WaitIdle ();

		}
		protected override void Dispose (bool disposing) {
			countdown.Wait ();

			vkvgSurf.Dispose ();
			vkvgDev.Dispose ();

			base.Dispose (disposing);
		}

	}
}
