// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using vke;

namespace VK
{
	partial class Program : VkWindow
	{
		public override void Update()
		{
			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.Clear();

				ctx.FontFace = "mono";

				for (int i = 0; i < iterations; i++)
				{
					float x = 0.8f * (float)(rnd.NextDouble() * Width) + 10f;
					float y = 0.9f * (float)(rnd.NextDouble() * Height) + 10f;
					uint s = (uint)(rnd.NextDouble() * 80) + 1;
					randomize_color(ctx);
					ctx.FontSize = s;
					ctx.MoveTo(x, y);
					ctx.ShowText("This is a test string!");
				}
			}
		}

	}
}
