﻿//
// CommandPool.cs
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
using static Vulkan.VulkanNative;

namespace tests {
    public class CommandPool : DeviceObject {
        public readonly uint QFamIndex;
        VkCommandPool pool;

        internal CommandPool (VkDevice _dev, uint qFamIdx, VkCommandPool _pool)
            : base (_dev)
        {
            dev = _dev;
            pool = _pool;
            QFamIndex = qFamIdx;
        }
        public override void Destroy () {
            vkDestroyCommandPool (dev, pool, IntPtr.Zero);
        }
        unsafe public CommandBuffer AllocateCommandBuffer (VkCommandBufferLevel level = VkCommandBufferLevel.Primary) {
            VkCommandBuffer buff;
            VkCommandBufferAllocateInfo infos = VkCommandBufferAllocateInfo.New ();
            infos.commandPool = pool;
            infos.level = level;
            infos.commandBufferCount = 1;

            Utils.CheckResult (vkAllocateCommandBuffers (dev, &infos, &buff));

            return new CommandBuffer (dev, pool, buff);
        }
    }
}
