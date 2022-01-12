// Copyright (c) 2018-2020  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;

namespace vkvg
{
	public class Recording: IDisposable
	{

		IntPtr handle = IntPtr.Zero;

		#region CTORS & DTOR
		internal Recording (IntPtr handle)
		{
			this.handle = handle;
		}
		~Recording ()
		{
			Dispose (false);
		}
		#endregion

		public IntPtr Handle => handle;
		public IntPtr Data => NativeMethods.vkvg_recording_get_data (handle);
		public uint Count => NativeMethods.vkvg_recording_get_count (handle);

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

			NativeMethods.vkvg_recording_destroy (handle);
			handle = IntPtr.Zero;
		}
		#endregion
	}
}

