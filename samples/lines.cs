// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using CVKL;

namespace VK
{
	partial class Program : VkWindow
	{
		static void Main(string[] args)
		{
			using (Program vke = new Program())
			{
				vke.Run();
			}
		}


		public override void Update()
		{
			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.Clear();

				for (int i = 0; i < iterations; i++)
				{
					float x1 = (float)(rnd.NextDouble() * Width);
					float y1 = (float)(rnd.NextDouble() * Height);
					float x2 = (float)(rnd.NextDouble() * Width) + 1.0f;
					float y2 = (float)(rnd.NextDouble() * Height) + 1.0f;
					randomize_color(ctx);
					ctx.MoveTo(x1, y1);
					ctx.LineTo(x2, y2);
					ctx.Stroke();
				}
			}
		}

	}
}
