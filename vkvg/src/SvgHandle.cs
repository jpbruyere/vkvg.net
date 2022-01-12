// Copyright (c) 2021  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using Drawing2D;

namespace vkvg
{
	public sealed class SvgHandle : ISvgHandle {

		internal Surface surf;

		#region CTOR
		public  SvgHandle (Device dev, Span<byte> svgFragment)
		{
			surf = new Surface(dev, NativeMethods.vkvg_surface_create_from_svg_fragment (dev.Handle, 512, 512, svgFragment.ToArray()));
		}
		public SvgHandle (Device dev, string file_name)
		{
			surf = new Surface(dev, NativeMethods.vkvg_surface_create_from_svg (dev.Handle, 512, 512, file_name));
		}
		#endregion

		public void Render(IContext cr) {
			cr.SetSource (surf, 0, 0);
			cr.Paint();
		}
		public void Render (IContext cr, string id) {
			cr.SetSource (surf, 0, 0);
			cr.Paint();
		}
		public Size Dimensions {
			get {
				return new Size (surf.Width, surf.Height);
			}
		}
		public void Dispose() {
			surf?.Dispose();
		}

	}
}
