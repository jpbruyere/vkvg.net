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
			if (paused)
				return;
			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.Clear();

				for (int i = 0; i < iterations; i++)
				{
					float x = 0.8f * (float)(rnd.NextDouble() * Width);
					float y = 0.8f * (float)(rnd.NextDouble() * Height);
					float w = 0.2f * (float)(rnd.NextDouble() * Width);
					float h = 0.2f * (float)(rnd.NextDouble() * Height);
					randomize_color(ctx);
					ctx.Rectangle(x, y, w, h);
					ctx.Fill();
				}
			}
		}

	}
}
