﻿//
// Queue.cs
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
using System.Collections;
using Vulkan;

namespace tests {
    public class QueueFamily {
        public VkQueueFlags Flags;
        public NativeList<float> Priorities;
    }
    public class PresentQueue : Queue {

        public readonly VkSurfaceKHR Surface;

        public PresentQueue (Device _dev, VkQueueFlags requestedFlags, VkSurfaceKHR _surface, float _priority = 0.0f) 
        : base(_dev, requestedFlags, _priority) {
            dev = _dev;
            priority = _priority;
            Surface = _surface;

            qFamIndex = searchQFamily (requestedFlags);
            dev.queues.Add (this);
        }
        uint searchQFamily (VkQueueFlags requestedFlags) {
            //search for dedicated Q
            for (uint i = 0; i < dev.phy.QueueFamilies.Length; i++) {
                if (dev.phy.QueueFamilies[i].queueFlags == requestedFlags && dev.phy.GetPresentIsSupported (i, Surface)) 
                    return i;
            }
            //search Q having flags
            for (uint i = 0; i < dev.phy.QueueFamilies.Length; i++) {
                if ((dev.phy.QueueFamilies[i].queueFlags & requestedFlags) == requestedFlags && dev.phy.GetPresentIsSupported (i, Surface)) 
                    return i;
            }

            throw new Exception (string.Format ("No Queue with flags {0} found", requestedFlags));
        }
    }

    public class Queue {

        internal VkQueue handle;
        internal Device dev;

        VkQueueFlags flags => dev.phy.QueueFamilies[qFamIndex].queueFlags;
        public uint qFamIndex;
        public uint index;//index in queue family
        public float priority;

        public Queue (Device _dev, VkQueueFlags requestedFlags, float _priority = 0.0f) {
            dev = _dev;
            priority = _priority;

            qFamIndex = searchQFamily (requestedFlags);
            dev.queues.Add (this);
        }

        public void Submit () { 
        }

        uint searchQFamily (VkQueueFlags requestedFlags) {
            //search for dedicated Q
            for (uint i = 0; i < dev.phy.QueueFamilies.Length; i++) {
                if (dev.phy.QueueFamilies[i].queueFlags == requestedFlags)
                    return i;
            }
            //search Q having flags
            for (uint i = 0; i < dev.phy.QueueFamilies.Length; i++) {
                if ((dev.phy.QueueFamilies[i].queueFlags & requestedFlags) == requestedFlags)
                    return i;
            }

            throw new Exception (string.Format ("No Queue with flags {0} found", requestedFlags));
        }

        internal void updateHandle () {
            VulkanNative.vkGetDeviceQueue (dev.VkDev, qFamIndex, index, out handle);
        }
    }
}
