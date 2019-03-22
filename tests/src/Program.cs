using System;
using Vulkan;
using static Vulkan.VulkanNative;

namespace tests {
    class Program : VkWindow {
        vkvg.Device vkvgDev;
        vkvg.Surface vkvgSurf;

        Program () : base ("test") {
            vkvgDev = new vkvg.Device (instance.Handle, phy.Handle, dev.VkDev.Handle, 0);
            vkvgSurf = new vkvg.Surface (vkvgDev, 1024, 768);
        }

        int a = 0;
        void vkvgDraw () {
            using (vkvg.Context ctx = new vkvg.Context (vkvgSurf)) {
                ctx.SetSource (0.4, 0.4, 0.4);
                ctx.Paint ();
                ctx.Rotate (0.05 * a);
                /*ctx.LineWidth = 4;
                ctx.MoveTo (100, a);
                ctx.LineTo (200, a);
                ctx.SetSource (0, 0, 1);
                ctx.Stroke ();*/
                float xc = 128.0f;
                float yc = 128.0f;
                float radius = 100.0f;
                float angle1 = 45.0f * (MathF.PI / 180.0f);  /* angles are specified */
                float angle2 = 180.0f * (MathF.PI / 180.0f);  /* in radians           */

                ctx.SetSource (0, 0, 0);
                ctx.LineWidth = 10;
                ctx.Arc (xc, yc, radius, angle1, angle2);
                ctx.Stroke ();

                /* draw helping lines */
                ctx.SetSource (1, 0.2, 0.2, 0.6);
                ctx.LineWidth = 6;
                ctx.Arc (xc, yc, 10, 0, 2f * Math.PI);
                ctx.Fill ();

                ctx.Arc (xc, yc, radius, angle1, angle1);
                ctx.LineTo (xc, yc);
                ctx.Arc (xc, yc, radius, angle2, angle2);
                ctx.Stroke ();

                ctx.SetSource (1, 1, 1);
                ctx.MoveTo (200, 200);
                ctx.FontSize = 20;
                ctx.FontFace = "droid";
                //ctx.ShowText (this.frameTimer.ToString());
                ctx.ShowText ("This is a test string");
            }
        }

        protected override void HandleWindowSizeDelegate (IntPtr window, int width, int height) {
            dev.WaitIdle ();
            vkvgSurf.Dispose ();
            Console.WriteLine ("surf resized");
            vkvgSurf = new vkvg.Surface (vkvgDev, width, height);

            base.HandleWindowSizeDelegate (window, width, height);
        }
        unsafe protected override void BuildCommandBuffers () {
            Console.WriteLine ("rebuild cmd buff");
            VkImage srcImg = new Vulkan.VkImage ((ulong)vkvgSurf.VkImage.ToInt64 ());

            for (int i = 0; i < swapChain.ImageCount; ++i) {

                swapChain.cmds[i].Start ();

                Utils.setImageLayout (swapChain.cmds[i].Handle, swapChain.images[i], VkImageAspectFlags.Color,
                    VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal,
                    VkPipelineStageFlags.BottomOfPipe, VkPipelineStageFlags.Transfer);
                Utils.setImageLayout (swapChain.cmds[i].Handle, srcImg, VkImageAspectFlags.Color,
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
                    srcOffset = default (VkOffset3D),
                    dstSubresource = imgSubResLayer,
                    dstOffset = default (VkOffset3D),
                    extent = new VkExtent3D { width = (uint)vkvgSurf.Width, height = (uint)vkvgSurf.Height }
                };
                vkCmdCopyImage (swapChain.cmds[i].Handle, srcImg, VkImageLayout.TransferSrcOptimal,
                    swapChain.images[i], VkImageLayout.TransferDstOptimal, 1, &cregion);

                Utils.setImageLayout (swapChain.cmds[i].Handle, swapChain.images[i], VkImageAspectFlags.Color,
                    VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR,
                    VkPipelineStageFlags.Transfer, VkPipelineStageFlags.BottomOfPipe);
                Utils.setImageLayout (swapChain.cmds[i].Handle, srcImg, VkImageAspectFlags.Color,
                    VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal,
                    VkPipelineStageFlags.Transfer, VkPipelineStageFlags.ColorAttachmentOutput);

                swapChain.cmds[i].End ();
            }
        }
        public override void Update () {
            vkvgDraw ();
        }
        protected override void Destroy () {
            vkvgSurf.Dispose ();
            vkvgDev.Dispose ();

            base.Destroy ();
        }
        static void Main (string[] args) {
            Program vke = new Program ();
            vke.Run ();
        }
    }
}
