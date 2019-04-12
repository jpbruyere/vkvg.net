using System;
using VKE;
using static VK.Vk;

namespace VK {
    class Program : VkWindow {
        vkvg.Device vkvgDev;
        vkvg.Surface vkvgSurf;

        Program () : base ("test", 1024,768) {
            UpdateFreq = 2;
            vkvgDev = new vkvg.Device (instance.Handle, phy.Handle, dev.VkDev.Handle, presentQueue.qFamIndex);
        }
            
        int a = 0;
        void vkvgDraw () {
            a ++;
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
                float angle1 = 45.0f * ((float)Math.PI / 180.0f);  /* angles are specified */
                float angle2 = 180.0f * ((float)Math.PI / 180.0f);  /* in radians           */

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

        public override void Update () {         
            vkvgDraw ();
        }

        protected override void OnResize () {

            if (vkvgSurf != null)
                vkvgSurf.Dispose ();
            vkvgSurf = new vkvg.Surface (vkvgDev, (int)swapChain.Width, (int)swapChain.Height);

            VkImage srcImg = new VkImage ((ulong)vkvgSurf.VkImage.ToInt64 ());

            for (int i = 0; i < swapChain.ImageCount; ++i) {

                cmds[i] = cmdPool.AllocateCommandBuffer ();
                cmds[i].Start ();

                Utils.setImageLayout (cmds[i].Handle, swapChain.images[i].Handle, VkImageAspectFlags.Color,
                    VkImageLayout.Undefined, VkImageLayout.TransferDstOptimal,
                    VkPipelineStageFlags.BottomOfPipe, VkPipelineStageFlags.Transfer);
                Utils.setImageLayout (cmds[i].Handle, srcImg, VkImageAspectFlags.Color,
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
                vkCmdCopyImage (cmds[i].Handle, srcImg, VkImageLayout.TransferSrcOptimal,
                    swapChain.images[i].Handle, VkImageLayout.TransferDstOptimal, 1, ref cregion);

                Utils.setImageLayout (cmds[i].Handle, swapChain.images[i].Handle, VkImageAspectFlags.Color,
                    VkImageLayout.TransferDstOptimal, VkImageLayout.PresentSrcKHR,
                    VkPipelineStageFlags.Transfer, VkPipelineStageFlags.BottomOfPipe);
                Utils.setImageLayout (cmds[i].Handle, srcImg, VkImageAspectFlags.Color,
                    VkImageLayout.TransferSrcOptimal, VkImageLayout.ColorAttachmentOptimal,
                    VkPipelineStageFlags.Transfer, VkPipelineStageFlags.ColorAttachmentOutput);

                cmds[i].End ();
            }
            dev.WaitIdle ();
        }

        protected override void Dispose (bool disposing) {

            vkvgSurf.Dispose ();
            vkvgDev.Dispose ();

            base.Dispose (disposing);
        }

        static void Main (string[] args) {
            using (Program vke = new Program ()) {
                vke.Run ();
            }			
        }
    }
}
