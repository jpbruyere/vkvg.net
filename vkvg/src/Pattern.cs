// Copyright (c) 2018-2022  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using Drawing2D;

namespace vkvg
{
	public class Pattern : IPattern
	{

		protected IntPtr handle = IntPtr.Zero;

		#region CTORS & DTOR
		protected Pattern(IntPtr handle)
		{
			this.handle = handle;
		}
		public Pattern()
		{
			handle = NativeMethods.vkvg_pattern_create();
		}
		public Pattern(float r, float g, float b)
		{
			handle = NativeMethods.vkvg_pattern_create_rgb(r, g, b);
		}
		public Pattern(float r, float g, float b, float a)
		{
			handle = NativeMethods.vkvg_pattern_create_rgba(r, g, b, a);
		}
		public Pattern(Surface surf)
		{
			handle = NativeMethods.vkvg_pattern_create_for_surface(surf.Handle);
		}

		~Pattern()
		{
			Dispose(false);
		}
		#endregion

		public static Pattern CreateLinearGradient(float x0, float y0, float x1, float y1)
		{
			return new Pattern(NativeMethods.vkvg_pattern_create_linear(x0, y0, x1, y1));
		}
		public static Pattern CreateRadialGradient(float cx0, float cy0, float radius0,
													 float cx1, float cy1, float radius1)
		{
			return new Pattern(NativeMethods.vkvg_pattern_create_radial(cx0, cy0, radius0, cx1, cy1, radius1));
		}

		public IntPtr Handle => handle;
		public void AddReference() => NativeMethods.vkvg_pattern_reference(handle);
		public uint References() => NativeMethods.vkvg_pattern_get_reference_count(handle);

		public Extend Extend
		{
			get => NativeMethods.vkvg_pattern_get_extend(handle);
			set { NativeMethods.vkvg_pattern_set_extend(handle, value); }
		}
		public Filter Filter
		{
			get => NativeMethods.vkvg_pattern_get_filter(handle);
			set { NativeMethods.vkvg_pattern_set_filter(handle, value); }
		}

		#region IDisposable implementation
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || handle == IntPtr.Zero)
				return;

			NativeMethods.vkvg_pattern_destroy(handle);
			handle = IntPtr.Zero;
		}
		#endregion
	}
}