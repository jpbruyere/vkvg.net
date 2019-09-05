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
		static void DrawRoundedRectangle(vkvg.Context gr, double x, double y, double width, double height, double radius)
		{
			if ((radius > height / 2) || (radius > width / 2))
				radius = Math.Min(height / 2, width / 2);

			gr.MoveTo(x, y + radius);
			gr.Arc(x + radius, y + radius, radius, Math.PI, -Math.PI / 2);
			gr.LineTo(x + width - radius, y);
			gr.Arc(x + width - radius, y + radius, radius, -Math.PI / 2, 0);
			gr.LineTo(x + width, y + height - radius);
			gr.Arc(x + width - radius, y + height - radius, radius, 0, Math.PI / 2);
			gr.LineTo(x + radius, y + height);
			gr.Arc(x + radius, y + height - radius, radius, Math.PI / 2, Math.PI);
			gr.ClosePath();
		}

		public override void Update()
		{
			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.Clear();

				for (int i = 0; i < iterations; i++)
				{
					float x = 0.7f * (float)(rnd.NextDouble() * Width);
					float y = 0.7f * (float)(rnd.NextDouble() * Height);
					float w = 0.3f * (float)(rnd.NextDouble() * Width);
					float h = 0.3f * (float)(rnd.NextDouble() * Height);
					float r = 10.0f * (float)(rnd.NextDouble());
					randomize_color(ctx);
					DrawRoundedRectangle(ctx, x, y, w, h, 5);
					ctx.Fill();
				}
			}
		}
	}
}
