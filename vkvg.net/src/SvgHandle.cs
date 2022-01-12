// Copyright (c) 2021  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using Drawing2D;

namespace vkvg
{
	public sealed class SvgHandle : ISvgHandle {

		internal IntPtr handle;

		#region CTOR
		public SvgHandle (Device dev, string file_name)
		{
			handle = NativeMethods.vkvg_svg_load (file_name);
		}
		public  SvgHandle (Device dev, Span<byte> svgFragment)
		{
			handle = NativeMethods.vkvg_svg_load_fragment (ref svgFragment.GetPinnableReference ());
		}
		#endregion
		~SvgHandle ()
		{
			Dispose (false);
		}

		public void Render(IContext cr) {
			if (cr is Context ctx)
				NativeMethods.vkvg_svg_render (handle, ctx.Handle, null);
		}
		public void Render (IContext cr, string id) {
			if (cr is Context ctx)
				NativeMethods.vkvg_svg_render (handle, ctx.Handle, id);
		}
		public Size Dimensions {
			get {
				NativeMethods.vkvg_svg_get_dimensions (handle, out uint width, out uint height);
				return new Size ((int)width, (int)height);
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}
		void Dispose (bool disposing)
		{
			if (!disposing || handle == IntPtr.Zero)
				return;

			NativeMethods.vkvg_svg_destroy (handle);
			handle = IntPtr.Zero;
		}
		#endregion
	}
}
