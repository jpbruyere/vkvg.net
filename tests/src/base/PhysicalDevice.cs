﻿//
// PhysicalDevice.cs
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
using static Vulkan.Utils;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace tests {
    public class PhysicalDeviceCollection : IEnumerable<PhysicalDevice> {
        VkInstance inst;
        PhysicalDevice[] phys;

        public PhysicalDeviceCollection (VkInstance instance) {
            inst = instance;
            init ();
        }

        public PhysicalDevice this[int i] {
            get {
                return phys[i];
            }
        }

        public IEnumerator<PhysicalDevice> GetEnumerator () {
            return ((IEnumerable<PhysicalDevice>)phys).GetEnumerator ();
        }

        IEnumerator IEnumerable.GetEnumerator () {
            return ((IEnumerable<PhysicalDevice>)phys).GetEnumerator ();
        }

        unsafe void init () {
            uint gpuCount = 0;
            CheckResult (vkEnumeratePhysicalDevices (inst, &gpuCount, null));
            if (gpuCount <= 0)
                throw new Exception ("No GPU found");
            IntPtr[] gpus = new IntPtr [gpuCount];

            fixed (IntPtr* physicalDevices = gpus) {
                CheckResult (vkEnumeratePhysicalDevices (inst, &gpuCount, (VkPhysicalDevice*)physicalDevices),
                    "Could not enumerate physical devices.");
            }
            phys = new PhysicalDevice[gpuCount];

            for (int i = 0; i < gpuCount; i++) {
                phys[i] = new PhysicalDevice (gpus[i]);
            }
        }
    }
    public class PhysicalDevice {
        IntPtr phy;

        public VkPhysicalDeviceProperties deviceProperties { get; private set; }
        public VkPhysicalDeviceFeatures deviceFeatures { get; private set; }
        public VkPhysicalDeviceMemoryProperties memoryProperties { get; private set; }
        public VkQueueFamilyProperties[] QueueFamilies { get; private set; }

        public bool HasSwapChainSupport { get; private set; }
        public IntPtr Handle => phy;

        public PhysicalDevice (IntPtr vkPhy) {
            phy = vkPhy;
            init ();
        }

        unsafe void init () {
            VkPhysicalDeviceProperties pdp;
            vkGetPhysicalDeviceProperties (phy, &pdp);
            deviceProperties = pdp;

            VkPhysicalDeviceFeatures df;
            vkGetPhysicalDeviceFeatures (phy, &df);
            deviceFeatures = df;

            // Gather physical Device memory properties
            VkPhysicalDeviceMemoryProperties dmp;
            vkGetPhysicalDeviceMemoryProperties (phy, &dmp);
            memoryProperties = dmp;

            uint queueFamilyCount = 0;
            vkGetPhysicalDeviceQueueFamilyProperties (phy, ref queueFamilyCount, null);
            QueueFamilies = new VkQueueFamilyProperties[queueFamilyCount];

            if (queueFamilyCount <= 0)
                throw new Exception ("No queues found for physical device");

            fixed (VkQueueFamilyProperties* ptr = QueueFamilies) {
                vkGetPhysicalDeviceQueueFamilyProperties (phy, &queueFamilyCount, ptr);
            }

            uint propCount = 0;

            vkEnumerateDeviceExtensionProperties (phy, (byte*)null, ref propCount, IntPtr.Zero);

            VkExtensionProperties[] extProps = new VkExtensionProperties[propCount];
            fixed (VkExtensionProperties* ptr = extProps) {
                vkEnumerateDeviceExtensionProperties (phy, (byte*)null, ref propCount, ptr);
            }

            foreach (VkExtensionProperties p in extProps) {
                IntPtr n = (IntPtr)p.extensionName;
                switch (Marshal.PtrToStringUTF8 (n)) {
                    case "VK_KHR_swapchain":
                        HasSwapChainSupport = true;
                        break;
                }
            }
        }

        public bool GetPresentIsSupported (uint qFamilyIndex, VkSurfaceKHR surf) {
            VkBool32 isSupported = false;
            VulkanNative.vkGetPhysicalDeviceSurfaceSupportKHR (phy, qFamilyIndex, surf, out isSupported);
            return isSupported;
        }

        public VkSurfaceCapabilitiesKHR GetSurfaceCapabilities (VkSurfaceKHR surf) {
            VkSurfaceCapabilitiesKHR caps;
            vkGetPhysicalDeviceSurfaceCapabilitiesKHR (phy, surf, out caps);
            return caps;
        }

        unsafe public VkSurfaceFormatKHR[] GetSurfaceFormats (VkSurfaceKHR surf) {
            uint count = 0;
            vkGetPhysicalDeviceSurfaceFormatsKHR (phy, surf, ref count, null);
            VkSurfaceFormatKHR[] formats = new VkSurfaceFormatKHR[count];
            fixed (VkSurfaceFormatKHR* ptr = formats) {
                vkGetPhysicalDeviceSurfaceFormatsKHR (phy, surf, ref count, ptr);
            }
            return formats;
        }
        unsafe public VkPresentModeKHR[] GetSurfacePresentModes (VkSurfaceKHR surf) {
            uint count = 0;
            vkGetPhysicalDeviceSurfacePresentModesKHR (phy, surf, ref count, null);
            VkPresentModeKHR[] modes = new VkPresentModeKHR[count];
            fixed (VkPresentModeKHR* ptr = modes) {
                vkGetPhysicalDeviceSurfacePresentModesKHR (phy, surf, ref count, ptr);
            }
            return modes;
        }
    }
}
