// Copyright (c) 2013-2020  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
namespace Crow
{
	public static class Extensions
	{
		public static void SetAsSource (this Fill f, vkvg.Context ctx, Rectangle bounds = default (Rectangle)) {
			if (f is SolidColor sc)
				sc.SetAsSource (ctx, bounds);
			else
				throw new NotImplementedException ();
		}
		public static void SetAsSource (this SolidColor sc, vkvg.Context ctx, Rectangle bounds = default (Rectangle)) =>
			ctx.SetSource (sc.color.R, sc.color.G, sc.color.B, sc.color.A);
		
	}
}
