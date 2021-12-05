// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using vke;
using Vulkan;

using OpenTK.Mathematics;
using static System.MathF;
using Drawing2D;

namespace VK
{
	partial class Program : VkWindow
	{
		/*void midptellipse(vkvg.Context ctx, int rx, int ry,
						int xc, int yc)
		{
			float pointSize = 0.2f;
			int pointCount = 0;
			float dx, dy, d1, d2, x, y;
			x = 0;
			y = ry;
			// Initial decision parameter of region 1
			d1 = (ry * ry) - (rx * rx * ry) + (0.25f * rx * rx);
			dx = 2 * ry * ry * x;
			dy = 2 * rx * rx * y;

			// For region 1
			while (dx < dy)
			{
				Console.WriteLine ($"d1:{d1} dx:{dx} dy:{dy} dy-dx:{dy-dx}");
				float alpha = (float)pointCount / 68.0f;
				// Print points based on 4-way symmetry
				ctx.SetSource (1,0,0,alpha);
				ctx.Arc (x + xc, y + yc, pointSize * 2, 0, MathF.PI*2);
				ctx.Fill ();
				ctx.SetSource (0,1,0,alpha);
				ctx.Arc (-x + xc, y + yc, pointSize, 0, MathF.PI*2);
				ctx.Fill ();
				ctx.SetSource (0,0,1,alpha);
				ctx.Arc (x + xc, -y + yc, pointSize, 0, MathF.PI*2);
				ctx.Fill ();
				ctx.SetSource (1,1,1,alpha);
				ctx.Arc (-x + xc, -y + yc, pointSize, 0, MathF.PI*2);
				ctx.Fill ();
				pointCount += 4;

				// Checking and updating value of
				// decision parameter based on algorithm
				if (d1 < 0)
				{
					x++;
					dx = dx + (2 * ry * ry);
					d1 = d1 + dx + (ry * ry);
				}
				else
				{
					x++;
					y--;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d1 = d1 + dx - dy + (ry * ry);
				}
			}

			// Decision parameter of region 2
			d2 = ((ry * ry) * ((x + 0.5f) * (x + 0.5f))) +
				((rx * rx) * ((y - 1) * (y - 1))) -
				(rx * rx * ry * ry);

			Console.WriteLine ($"point count={pointCount}");
			pointCount = 0;

			// Plotting points of region 2
			while (y >= 0)
			{
				float alpha = (float)pointCount / 26.0f;
				ctx.SetSource (1,0,1,alpha);
				ctx.Arc (x + xc, y + yc, pointSize * 2, 0, MathF.PI*2);
				ctx.Fill ();
				ctx.SetSource (0,1,1,alpha);
				ctx.Arc (-x + xc, y + yc, pointSize, 0, MathF.PI*2);
				ctx.Fill ();
				ctx.SetSource (1,1,0,alpha);
				ctx.Arc (x + xc, -y + yc, pointSize, 0, MathF.PI*2);
				ctx.Fill ();
				ctx.SetSource (0.5,0.1,0.8,alpha);
				ctx.Arc (-x + xc, -y + yc, pointSize, 0, MathF.PI*2);
				ctx.Fill ();
				pointCount += 4;
				// Checking and updating parameter
				// value based on algorithm
				if (d2 > 0)
				{
					y--;
					dy = dy - (2 * rx * rx);
					d2 = d2 + (rx * rx) - dy;
				}
				else
				{
					y--;
					x++;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d2 = d2 + dx - dy + (rx * rx);
				}
			}
			Console.WriteLine ($"point count={pointCount}");
		}*/
		void midptellipse(vkvg.Context ctx, int rx, int ry,
						int xc, int yc)
		{
			List<PointD>[] pts = new List<PointD>[4] {
				new List<PointD>(100),
				new List<PointD>(100),
				new List<PointD>(100),
				new List<PointD>(100)
			};
			float dx, dy, d1, d2, x, y;
			x = 0;
			y = ry;
			// Initial decision parameter of region 1
			d1 = (ry * ry) - (rx * rx * ry) + (0.25f * rx * rx);
			dx = 2 * ry * ry * x;
			dy = 2 * rx * rx * y;

			// For region 1
			while (dx < dy)
			{
				pts[0].Add (new PointD (x + xc, y + yc));
				pts[1].Add (new PointD (-x + xc, y + yc));
				pts[2].Add (new PointD (x + xc, -y + yc));
				pts[3].Add (new PointD (-x + xc, -y + yc));

				// Checking and updating value of
				// decision parameter based on algorithm
				if (d1 < 0)
				{
					x++;
					dx = dx + (2 * ry * ry);
					d1 = d1 + dx + (ry * ry);
				}
				else
				{
					x++;
					y--;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d1 = d1 + dx - dy + (ry * ry);
				}
			}

			// Decision parameter of region 2
			d2 = ((ry * ry) * ((x + 0.5f) * (x + 0.5f))) +
				((rx * rx) * ((y - 1) * (y - 1))) -
				(rx * rx * ry * ry);

			// Plotting points of region 2
			while (y >= 0)
			{
				pts[0].Add (new PointD (x + xc, y + yc));
				pts[1].Add (new PointD (-x + xc, y + yc));
				pts[2].Add (new PointD (x + xc, -y + yc));
				pts[3].Add (new PointD (-x + xc, -y + yc));
				// Checking and updating parameter
				// value based on algorithm
				if (d2 > 0)
				{
					y--;
					dy = dy - (2 * rx * rx);
					d2 = d2 + (rx * rx) - dy;
				}
				else
				{
					y--;
					x++;
					dx = dx + (2 * ry * ry);
					dy = dy - (2 * rx * rx);
					d2 = d2 + dx - dy + (rx * rx);
				}
			}
			float minLenght = 10;
			PointD lp = pts[0][pts[0].Count -1];
			ctx.MoveTo (lp);
			for (int i = pts[0].Count - 2; i >= 0; i--)
			{
				PointD p = pts[0][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			for (int i = 0; i < pts[1].Count; i++)
			{
				PointD p = pts[1][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			for (int i = pts[3].Count - 2; i >= 0; i--)
			{
				PointD p = pts[3][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			for (int i = 0; i < pts[2].Count; i++)
			{
				PointD p = pts[2][i];
				if ((p - lp).Length < minLenght)
					continue;
				ctx.LineTo (p);
				lp = p;
			}
			ctx.SetSource (1,1,1);
			ctx.LineWidth = 1;
			ctx.ClosePath ();
			ctx.Stroke ();
		}
		void printColorCodes (vkvg.Context ctx, float x, float y, float r, float g, float b) {
			ctx.Rectangle (x,y,10,6);
			ctx.SetSource (r,g,b);
			ctx.Fill();
			ctx.SetSource (1,1,1);
			ctx.MoveTo (x+12, y+6);
			ctx.ShowText ($"{r} {g} {b}");
		}

		void elliptic_arc (vkvg.Context ctx, float x1, float y1, float x2, float y2, bool largeArc, bool sweepAnglePositive, float rx, float ry, float phi) {
			Matrix2 m = new Matrix2 (
				 Cos (phi), Sin (phi),
				-Sin (phi), Cos (phi)
			);
			Vector2 p = new Vector2 ((x1 - x2)/2, (y1 - y2)/2);
			Vector2 p1 = m * p;

			p = new Vector2 (rx * p1.Y / ry, -ry * p1.X / rx);
			Vector2 cp = Sqrt (Abs(
				(Pow (rx,2) * Pow (ry,2) - Pow (rx,2) * Pow (p1.Y, 2) - Pow (ry,2) * Pow (p1.X, 2)) /
				(Pow (rx,2) * Pow (p1.Y, 2) + Pow (ry,2) * Pow (p1.X, 2))
			)) * p;
			if (largeArc == sweepAnglePositive)
				cp = -cp;

			m = new Matrix2 (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);
			p = new Vector2 ((x1 + x2)/2, (y1 + y2)/2);
			Vector2 c = m * cp + p;

			Vector2 u = Vector2.UnitX;
			Vector2 v = new Vector2 ((p1.X-cp.X)/rx, (p1.Y-cp.Y)/ry);
			float theta1 = Acos (Vector2.Dot (u, v) / (v.Length));
			if (u.X*v.Y-u.Y*v.X < 0)
				theta1 = -theta1;

			u = v;
			v = new Vector2 ((-p1.X-cp.X)/rx, (-p1.Y-cp.Y)/ry);
			float delta_theta = Acos (Vector2.Dot (u, v) / (v.Length)) % (PI * 2);
			if (u.X*v.Y-u.Y*v.X < 0)
				delta_theta = -delta_theta;

			m = new Matrix2 (
				Cos (phi),-Sin (phi),
				Sin (phi), Cos (phi)
			);

			float a = theta1;
			float theta2 = theta1 + delta_theta;
			float step = 0.1f;

			List<PointD> pts = new List<PointD> (1000);
			Vector2 pT = default;
			if (delta_theta < 0) {
				while (a > theta2) {
					pT = new Vector2 (rx*Cos(a),ry*Sin(a));
					p = m * pT + c;
					pts.Add (new PointD (p.X, p.Y));
					a-=step;
				}
			} else {
				while (a < theta2) {
					pT = new Vector2 (rx*Cos(a),ry*Sin(a));
					p = m * pT + c;
					pts.Add (new PointD (p.X, p.Y));
					a+=step;
				}
			}
			pT = new Vector2 (rx*Cos(theta2),ry*Sin(theta2));
			Vector2 lp = m * pT + c;
			if ((lp - p).Length > float.Epsilon)
				pts.Add (new PointD (lp.X, lp.Y));

			ctx.LineWidth = 1;
			ctx.MoveTo (pts[0]);
			for (int i = 1; i < pts.Count; i++)
				ctx.LineTo (pts[i]);
			//ctx.ClosePath ();
			ctx.SetSource (1,1,1);
			ctx.Stroke ();
		}
		public override void Update()
		{


			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.Clear();

				//ctx.Scale (2,2);
				//midptellipse (ctx, 200,120,220,150);
				elliptic_arc (ctx,10,10,305,100,false,false,20,4,60.0f*PI/180.0f);

				/*for (int i = 0; i < iterations; i++)
				{
					float x1 = (float)(rnd.NextDouble() * Width);
					float y1 = (float)(rnd.NextDouble() * Height);
					float x2 = (float)(rnd.NextDouble() * Width) + 1.0f;
					float y2 = (float)(rnd.NextDouble() * Height) + 1.0f;
					randomize_color(ctx);
					ctx.MoveTo(x1, y1);
					ctx.LineTo(x2, y2);
					ctx.Stroke();
				}*/
			}
		}

	}
}
