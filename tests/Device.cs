//
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
using Vulkan;
using static Vulkan.VulkanNative;

namespace tests {
    public class Device : IDisposable {
        public readonly PhysicalDevice phy;
        VkDevice dev;
        VkSurfaceKHR hSurf;
        public VkDevice VkDev => dev;


        public Device (PhysicalDevice _phy, VkSurfaceKHR _hSurf) {
            phy = _phy;
            hSurf = _hSurf;

            ConfigureQueues ();
        }

        protected virtual void ConfigureEnabledFeatures (ref VkPhysicalDeviceFeatures enabledFeatures) {
            enabledFeatures.sampleRateShading = true;
        }

        unsafe protected void ConfigureQueues () {
            uint selectQFamIdx = uint.MaxValue;

            for (int i = 0; i < phy.QueueFamilies.Length; i++) {
                if (phy.QueueFamilies[i].queueFlags.HasFlag (VkQueueFlags.Graphics) &&
                    phy.GetPresentIsSupported (i, hSurf)) {
                    selectQFamIdx = (uint)i;
                    break;
                }
            }

            float defaultQueuePriority = 0.0f;

            VkDeviceQueueCreateInfo[] qInfos = {
                new VkDeviceQueueCreateInfo {
                    sType = VkStructureType.DeviceQueueCreateInfo,
                    queueCount = 1,
                    queueFamilyIndex = selectQFamIdx,
                    pQueuePriorities = &defaultQueuePriority
                }
            };

            VkPhysicalDeviceFeatures enabledFeatures = default (VkPhysicalDeviceFeatures);
            ConfigureEnabledFeatures (ref enabledFeatures);

            FixedUtf8String VK_KHR_SWAPCHAIN_EXTENSION_NAME = "VK_KHR_swapchain";
            NativeList<IntPtr> deviceExtensions = new NativeList<IntPtr> ();
            deviceExtensions.Add (VK_KHR_SWAPCHAIN_EXTENSION_NAME);

            VkDeviceCreateInfo deviceCreateInfo = VkDeviceCreateInfo.New ();
            fixed (VkDeviceQueueCreateInfo* pQInfos = qInfos) {
                deviceCreateInfo.queueCreateInfoCount = (uint)qInfos.Length;
                deviceCreateInfo.pQueueCreateInfos = pQInfos;
                deviceCreateInfo.pEnabledFeatures = &enabledFeatures;

                if (deviceExtensions.Count > 0) {
                    deviceCreateInfo.enabledExtensionCount = deviceExtensions.Count;
                    deviceCreateInfo.ppEnabledExtensionNames = (byte**)deviceExtensions.Data.ToPointer ();
                }

                Utils.CheckResult (vkCreateDevice (phy.Handle, &deviceCreateInfo, null, out dev));
            }

            deviceExtensions.Dispose ();
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
