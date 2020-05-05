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

				for (int i = 0; i < iterations; i++)
				{
					float x = (float)(rnd.NextDouble() * Width);
					float y = (float)(rnd.NextDouble() * Height);
					float r = 0.2f * (float)(rnd.NextDouble() * Width) + 1.0f;

					randomize_color(ctx);
					ctx.Arc (x, y, r, 0, 2.0f * Math.PI);
					ctx.Fill();

					if (i % 50 == 0)
						ctx.Flush();
				}
			}
		}

	}
}
