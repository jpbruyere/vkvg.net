// Copyright (c) 2018-2020  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Drawing2D;

namespace vkvg
{
	public class Device : IDevice
	{

		IntPtr handle = IntPtr.Zero;

		#region CTORS & DTOR
		public Device (IntPtr instance, IntPtr phy, IntPtr dev, uint qFamIdx, SampleCount samples = SampleCount.Sample_1, uint qIndex = 0)
		{
			handle = NativeMethods.vkvg_device_create_from_vk_multisample (instance, phy, dev, qFamIdx, qIndex, samples, false);
		}
		~Device ()
		{
			Dispose (false);
		}
		#endregion

		public IntPtr Handle => handle;
		public void AddReference () => NativeMethods.vkvg_device_reference (handle);
		public uint References () => NativeMethods.vkvg_device_get_reference_count (handle);

		#region IDevice implementation
		public void GetDpy (out int hdpy, out int vdpy) => NativeMethods.vkvg_device_get_dpy (handle, out hdpy, out vdpy);
		public void SetDpy (int hdpy, int vdpy) => NativeMethods.vkvg_device_set_dpy (handle, hdpy, vdpy);
		#endregion

		public static IEnumerable<string> GetRequiredInstanceExtensions () {
			NativeMethods.vkvg_get_required_instance_extensions(IntPtr.Zero, out uint count);
			IntPtr ptrSupExts = Marshal.AllocHGlobal (IntPtr.Size * (int)count);
			NativeMethods.vkvg_get_required_instance_extensions(ptrSupExts, out count);
			
			IntPtr tmp = ptrSupExts;
			for (int i = 0; i < count; i++) {
				IntPtr strPtr = Marshal.ReadIntPtr (tmp);
				yield return Marshal.PtrToStringAnsi (strPtr);
				tmp += IntPtr.Size;
			}
			Marshal.FreeHGlobal(ptrSupExts);
		}
		public static IEnumerable<string> GetRequiredDeviceExtensions (IntPtr phy) {
			NativeMethods.vkvg_get_required_device_extensions(phy, IntPtr.Zero, out uint count);
			IntPtr ptrSupExts = Marshal.AllocHGlobal (IntPtr.Size * (int)count);
			NativeMethods.vkvg_get_required_device_extensions(phy, ptrSupExts, out count);
			
			IntPtr tmp = ptrSupExts;
			for (int i = 0; i < count; i++) {
				IntPtr strPtr = Marshal.ReadIntPtr (tmp);
				yield return Marshal.PtrToStringAnsi (strPtr);
				tmp += IntPtr.Size;
			}
			Marshal.FreeHGlobal(ptrSupExts);
		}
		public static IntPtr GetDeviceRequirements (IntPtr pEnabledFeatures) {
			return NativeMethods.vkvg_get_device_requirements(pEnabledFeatures);
		}

		
		#region IDisposable implementation
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (!disposing || handle == IntPtr.Zero)
				return;

			NativeMethods.vkvg_device_destroy (handle);
			handle = IntPtr.Zero;
		}
		#endregion
	}
}

