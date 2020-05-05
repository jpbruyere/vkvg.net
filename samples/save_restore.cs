using System;
using vke;

namespace VK
{
	partial class Program : VkWindow
	{
		void recurseDraw(vkvg.Context ctx, int step)
		{
			step++;

			ctx.SetSource(1.0, 0.0, 0.0);
			ctx.FontFace = "droid";
			ctx.FontSize = 10;

			ctx.Save();

			ctx.Translate(20, 20);
			ctx.Rectangle(step, step, 600, 600);
			ctx.ClipPreserve();
			ctx.SetSource(1f / step, 1f / step, 1f / step);
			ctx.FillPreserve();
			ctx.SetSource(0, 0, 0);
			ctx.Stroke();

			if (step < 5)
				recurseDraw(ctx, step);

			ctx.Restore();

			ctx.Rectangle(step + 50, step + 1, 100, 12);
			ctx.Operator = vkvg.Operator.Clear;
			ctx.Fill();
			ctx.Operator = vkvg.Operator.Over;
			ctx.SetSource(1.0, 0.0, 0.0);
			ctx.MoveTo(step + 30, step + 12);
			ctx.ShowText($"fps: {fps}");


			ctx.Operator = vkvg.Operator.Clear;
			ctx.Rectangle(step, step, 20, 20);
			ctx.Fill();
			ctx.Operator = vkvg.Operator.Over;

			ctx.SetSource(1f / step, 0f, 0f);
			ctx.Rectangle(step + 2, step + 2, 16, 16);
			ctx.Fill();

		}

		public override void Update()
		{
			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				recurseDraw(ctx, 0);
			}
		}

	}
}
