//
// CommandBuffer.cs
//
// Author:
//       Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// Copyright (c) 2019 jp
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using System;
using Vulkan;

namespace tests {
    public class CommandBuffer : DeviceObject {
        VkCommandPool pool;
        VkCommandBuffer buff;

        public VkCommandBuffer Handle => buff;

        internal CommandBuffer (VkDevice _dev, VkCommandPool _pool, VkCommandBuffer _buff)
            :base (_dev)
        {
            pool = _pool;
            buff = _buff;
        }

        unsafe public void Submit (VkQueue queue, VkSemaphore wait, VkSemaphore signal) {
            VkSubmitInfo submit_info = VkSubmitInfo.New ();
            VkPipelineStageFlags dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            submit_info.commandBufferCount = 1;
            submit_info.signalSemaphoreCount = 1;
            submit_info.pWaitDstStageMask = &dstStageMask;
            submit_info.waitSemaphoreCount = 1;
            submit_info.pSignalSemaphores = &signal;
            submit_info.pWaitSemaphores = &wait;
            VkCommandBuffer cmd = buff;
            submit_info.pCommandBuffers = &cmd;
            Utils.CheckResult (VulkanNative.vkQueueSubmit (queue, 1, &submit_info, VkFence.Null));
        }
        public void Start () {
            VkCommandBufferBeginInfo cmdBufInfo = VkCommandBufferBeginInfo.New ();
            Utils.CheckResult (VulkanNative.vkBeginCommandBuffer (buff, ref cmdBufInfo));
        }
        public void End () {
            Utils.CheckResult (VulkanNative.vkEndCommandBuffer (buff));
        }
        public override void Destroy () {
            VkCommandBuffer tmp = buff;
            VulkanNative.vkFreeCommandBuffers (dev, pool, 1, ref tmp);
            buff = IntPtr.Zero;
        }
    }
}
