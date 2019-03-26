﻿//
// Device.cs
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
using System.Collections.Generic;
using System.Linq;
using Vulkan;
using static Vulkan.VulkanNative;

namespace tests {
    public class Device : IDisposable {
        public readonly PhysicalDevice phy;
        VkDevice dev;
        public VkDevice VkDev => dev;

        internal List<Queue> queues = new List<Queue> ();

        public Device (PhysicalDevice _phy) {
            phy = _phy;
        }

        unsafe public void Activate (VkPhysicalDeviceFeatures enabledFeatures, string[] extensions) {

            NativeList<VkDeviceQueueCreateInfo> qInfos = new NativeList<VkDeviceQueueCreateInfo> ();
            NativeList<float> priorities = null;

            foreach (IGrouping<uint, Queue> qfams in queues.GroupBy (q => q.qFamIndex)) {
                int qTot = qfams.Count ();
                uint qIndex = 0;
                priorities = new NativeList<float> ();
                bool qCountReached = false;//true when queue count of that family is reached

                foreach (Queue q in qfams) {
                    q.index = qIndex++;
                    if (qIndex == phy.QueueFamilies[qfams.Key].queueCount) {
                        qIndex = 0;
                        qCountReached = true;
                    }
                    if (qCountReached)
                        continue;
                    priorities.Add (q.priority);
                }

                qInfos.Add (new VkDeviceQueueCreateInfo {
                    sType = VkStructureType.DeviceQueueCreateInfo,
                    queueCount = qCountReached ? phy.QueueFamilies[qfams.Key].queueCount : qIndex,
                    queueFamilyIndex = qfams.Key,
                    pQueuePriorities = (float*)priorities.Data.ToPointer ()
                });
            }


            NativeList<IntPtr> deviceExtensions = new NativeList<IntPtr> ();
            for (int i = 0; i < extensions.Length; i++) {
                deviceExtensions.Add (new FixedUtf8String(extensions[i]));
            }

            VkPhysicalDeviceFeatures features = enabledFeatures;
            VkDeviceCreateInfo deviceCreateInfo = VkDeviceCreateInfo.New ();

            deviceCreateInfo.queueCreateInfoCount = qInfos.Count;
            deviceCreateInfo.pQueueCreateInfos = (VkDeviceQueueCreateInfo*)qInfos.Data.ToPointer();
            deviceCreateInfo.pEnabledFeatures = &features;

            if (deviceExtensions.Count > 0) {
                deviceCreateInfo.enabledExtensionCount = deviceExtensions.Count;
                deviceCreateInfo.ppEnabledExtensionNames = (byte**)deviceExtensions.Data.ToPointer ();
            }

            Utils.CheckResult (vkCreateDevice (phy.Handle, &deviceCreateInfo, null, out dev));

            qInfos.Dispose ();
            priorities.Dispose ();
            deviceExtensions.Dispose ();

            foreach (Queue q in queues)
                q.updateHandle ();
        }

        unsafe public VkSemaphore CreateSemaphore () {
            VkSemaphore tmp;
            VkSemaphoreCreateInfo info = VkSemaphoreCreateInfo.New ();
            Utils.CheckResult (vkCreateSemaphore (dev, &info, null, out tmp));
            return tmp;
        }
        unsafe public VkFence CreateFence (bool signaled = false) {
            VkFence tmp;
            VkFenceCreateInfo info = VkFenceCreateInfo.New ();
            info.flags = signaled ? VkFenceCreateFlags.Signaled : VkFenceCreateFlags.None;
            Utils.CheckResult (vkCreateFence (dev, &info, null, out tmp));
            return tmp;
        }
        public void DestroySemaphore (VkSemaphore semaphore) {
            vkDestroySemaphore (dev, semaphore, IntPtr.Zero);
        }
        public void DestroyFence (VkFence fence) {
            vkDestroyFence (dev, fence, IntPtr.Zero);
        }
        public void WaitIdle () {
            Utils.CheckResult(vkDeviceWaitIdle (dev));
        }
        unsafe public VkSwapchainKHR CreateSwapChain (VkSwapchainCreateInfoKHR infos) {
            VkSwapchainKHR newSwapChain;
            Utils.CheckResult (vkCreateSwapchainKHR (dev,&infos,NullHandle,out newSwapChain));
            return newSwapChain;
        }
        public void DestroySwapChain (VkSwapchainKHR swapChain) {
            vkDestroySwapchainKHR (dev, swapChain, IntPtr.Zero);
        }
        unsafe public VkImage[] GetSwapChainImages (VkSwapchainKHR swapchain) {
            uint imageCount = 0;
            Utils.CheckResult (vkGetSwapchainImagesKHR (dev, swapchain, ref imageCount, null));
            if (imageCount == 0)
                throw new Exception ("Swapchain image count is 0.");
            VkImage[] imgs = new VkImage[imageCount];
            fixed (VkImage* ptr = imgs) {
                Utils.CheckResult (vkGetSwapchainImagesKHR (dev, swapchain, ref imageCount, ptr));
            }
            return imgs;
        }
        unsafe public VkImageView CreateImageView (VkImage image, VkFormat format, VkImageViewType viewType = VkImageViewType.Image2D, VkImageAspectFlags aspectFlags = VkImageAspectFlags.Color) {
            VkImageView view;
            VkImageViewCreateInfo infos = VkImageViewCreateInfo.New ();
            infos.image = image;
            infos.viewType = viewType;
            infos.format = format;
            infos.components = new VkComponentMapping { r = VkComponentSwizzle.R, g = VkComponentSwizzle.G, b = VkComponentSwizzle.B, a = VkComponentSwizzle.A };
            infos.subresourceRange = new VkImageSubresourceRange (aspectFlags);
                    
            Utils.CheckResult (vkCreateImageView (dev, &infos, IntPtr.Zero, out view));
            return view;
        }
        public void DestroyImageView (VkImageView view) {
            vkDestroyImageView (dev, view, IntPtr.Zero);
        }
        unsafe public CommandPool CreateCommandPool (uint qFamIdx) {
            VkCommandPool pool;
            VkCommandPoolCreateInfo infos = VkCommandPoolCreateInfo.New ();
            infos.queueFamilyIndex = qFamIdx;
            Utils.CheckResult (vkCreateCommandPool (dev, &infos, null, out pool));
            return new CommandPool (dev, qFamIdx, pool);
        }

        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose (bool disposing) {
            if (!disposedValue) {
                if (disposing) {
                    // TODO: supprimer l'état managé (objets managés).
                }

                vkDestroyDevice (dev, IntPtr.Zero);

                disposedValue = true;
            }
        }

        // TODO: remplacer un finaliseur seulement si la fonction Dispose(bool disposing) ci-dessus a du code pour libérer les ressources non managées.
        ~Device() {
           // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
           Dispose(false);
        }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose () {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose (true);
            // TODO: supprimer les marques de commentaire pour la ligne suivante si le finaliseur est remplacé ci-dessus.
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
