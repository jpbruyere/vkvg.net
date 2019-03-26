﻿//
// VkEngine.cs
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
using System.Linq;
using Glfw;
using Vulkan;
using static Vulkan.VulkanNative;

namespace tests {
    public class VkWindow {
        IntPtr hWin;

        protected VkSurfaceKHR hSurf;
        protected Instance instance;
        protected PhysicalDevice phy;
        protected Device dev;
        protected Queue presentQueue;
        protected SwapChain swapChain;

        public VkWindow (string name, int width = 1024, int height=768)
        {
            Glfw3.Init ();

            Glfw3.WindowHint (WindowAttribute.ClientApi, 0);
            Glfw3.WindowHint (WindowAttribute.Resizable, 1);

            hWin = Glfw3.CreateWindow (width, height, name, MonitorHandle.Zero, IntPtr.Zero);

            Glfw3.SetKeyCallback (hWin, HandleKeyDelegate);
            Glfw3.SetMouseButtonPosCallback (hWin, HandleMouseButtonDelegate);
            Glfw3.SetCursorPosCallback (hWin, HandleCursorPosDelegate);
            Glfw3.SetWindowSizeCallback (hWin, HandleWindowSizeDelegate);

            initVulkan ();
        }

        void initVulkan () {
            instance = new Instance ();

            hSurf = instance.CreateSurface (hWin);

            phy = instance.GetAvailablePhysicalDevice ().Where (p => p.HasSwapChainSupport).FirstOrDefault ();

            VkPhysicalDeviceFeatures enabledFeatures = default(VkPhysicalDeviceFeatures);
            configureEnabledFeatures (ref enabledFeatures);

            //First create the c# device class
            dev = new Device (phy);
            //create queue class
            createQueues ();
            //activate the device to have effective queues created accordingly to what's available
            dev.Activate (enabledFeatures, new string[] { "VK_KHR_swapchain" });

            swapChain = new SwapChain (presentQueue as PresentQueue);
        }

        protected virtual void configureEnabledFeatures (ref VkPhysicalDeviceFeatures features) {
        }

        protected virtual void createQueues () {
            presentQueue = new PresentQueue (dev, VkQueueFlags.Graphics, hSurf);
        }




        protected virtual void HandleWindowSizeDelegate (IntPtr window, int width, int height) {
            BuildCommandBuffers ();
        }


        protected virtual void HandleCursorPosDelegate (IntPtr window, double xPosition, double yPosition) {
        }


        protected virtual void HandleMouseButtonDelegate (IntPtr window, Glfw.MouseButton button, InputAction action, Modifier mods) {
        }


        protected virtual void HandleKeyDelegate (IntPtr window, Key key, int scanCode, InputAction action, Modifier modifiers) {
            if (key == Key.F4 && modifiers == Modifier.Alt || key == Key.Escape)
                Glfw3.SetWindowShouldClose (hWin, 1);
        }

        public virtual void Run () {
            BuildCommandBuffers ();
            while (!Glfw3.WindowShouldClose (hWin)) {
                if (!swapChain.Swap ())
                    BuildCommandBuffers ();
                Update ();
                Glfw3.PollEvents ();
            }
            Destroy ();
        }
        public virtual void Update () {
        }


        protected virtual void BuildCommandBuffers () {
        }
        protected virtual void Destroy () {
            swapChain.Destroy ();

            vkDestroySurfaceKHR (instance.Handle, hSurf, IntPtr.Zero);

            dev.Dispose ();
            instance.Dispose ();

            Glfw3.DestroyWindow (hWin);
            Glfw3.Terminate ();
        }
    }
}
