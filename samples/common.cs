// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using CVKL;

namespace VK
{
	partial class Program : VkWindow
	{
		static Random rnd = new Random();
		static uint iterations = 250;

		vkvg.Device vkvgDev;
		vkvg.Surface vkvgSurf;

		Program() : base()
		{
			vkvgDev = new vkvg.Device(instance.Handle, phy.Handle, dev.VkDev.Handle, presentQueue.qFamIndex, vkvg.SampleCount.Sample_4);
			UpdateFrequency = 0;//update on each frame to have effective drawing perfs
		}

		void randomize_color(vkvg.Context ctx)
		{
			ctx.SetSource(rnd.NextDouble(), rnd.NextDouble(), rnd.NextDouble(), 0.5);
		}

		protected override void OnResize()
		{
			vkvgSurf?.Dispose();
			vkvgSurf = new vkvg.Surface(vkvgDev, (int)swapChain.Width, (int)swapChain.Height);

			VkImage srcImg = new VkImage((ulong)vkvgSurf.VkImage.ToInt64());

			for (int i = 0; i < swapChain.ImageCount; ++i)
			{

				cmds[i] = cmdPool.AllocateCommandBuffer();
				cmds[i].Start();

				Utils.setImageLayout(cmds[i].Handle, swapChain.images[i].Handle, VkImageAspectFlags.Color,
					VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal,
					VkPipelineStageFlags.BottomOfPipe, VkPipelineStageFlags.Transfer);
				Utils.setImageLayout(cmds[i].Handle, srcImg, VkImageAspectFlags.Color,
					VkImageLayout.ColorAttachmentOptimal, VkImageLayout.TransferSrcOptimal,
					VkPipelineStageFlags.ColorAttachmentOutput, VkPipelineStageFlags.Transfer);

				VkImageSubresourceLayers imgSubResLayer = new VkImageSubresourceLayers
				{
					aspectMask = VkImageAspectFlags.Color,
					mipLevel = 0,
					baseArrayLayer = 0,
					layerCount = 1
				};
				VkImageCopy cregion = new VkImageCopy
				{
					srcSubresource = imgSubResLayer,
					srcOffset = default,
					dstSubresource = imgSubResLayer,
					dstOffset = default,
					extent = new VkExtent3D { width = (uint)vkvgSurf.Width, height = (uint)vkvgSurf.Height }
				};
				Vk.vkCmdCopyImage(cmds[i].Handle, srcImg, VkImageLayout.TransferSrcOptimal,
					swapChain.images[i].Handle, VkImageLayout.TransferDstOptimal, 1, ref cregion);

				Utils.setImageLayout(cmds[i].Handle, swapChain.images[i].Handle, VkImageAspectFlags.Color,
					VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR,
					VkPipelineStageFlags.Transfer, VkPipelineStageFlags.BottomOfPipe);
				Utils.setImageLayout(cmds[i].Handle, srcImg, VkImageAspectFlags.Color,
					VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal,
					VkPipelineStageFlags.Transfer, VkPipelineStageFlags.ColorAttachmentOutput);

				cmds[i].End();
			}
			dev.WaitIdle();
		}
		protected override void Dispose(bool disposing)
		{

			vkvgSurf.Dispose();
			vkvgDev.Dispose();

			base.Dispose(disposing);
		}

	}
}
