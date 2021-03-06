﻿// Copyright (c) 2019  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using vke;
using Glfw;
using Vulkan;

namespace VK
{
	partial class Program : VkWindow
	{
		vkvg.Point[] points =  {
			new vkvg.Point(200,300),//pt1
			new vkvg.Point(200,50),	//cp1
			new vkvg.Point(500,300),//cp2
			new vkvg.Point(500,50),//pt2
		};

		int curPoint = -1;
		int cpRadius = 4, selRadius = 4;
		bool locked = false;

		bool isOver(vkvg.Point p, int x, int y) =>
			p.X - cpRadius < x && p.X + cpRadius > x && p.Y - cpRadius < y && p.Y + cpRadius > y;

		protected override void onMouseMove(double xPos, double yPos)
		{
			base.onMouseMove(xPos, yPos);

			if (locked)
			{
				if (curPoint < 0)
					return;
				points[curPoint] = new vkvg.Point((int)xPos, (int)yPos);
			}
			else
			{
				curPoint = -1;
				for (int i = 0; i < points.Length; i++)
				{
					if (!isOver(points[i], (int)lastMouseX, (int)lastMouseY))
						continue;
					curPoint = i;
					break;
				}
			}
		}
		protected override void onMouseButtonDown(MouseButton button)
		{
			base.onMouseButtonDown(button);
			locked |= button == Glfw.MouseButton.Left;
		}
		protected override void onMouseButtonUp(MouseButton button)
		{
			base.onMouseButtonUp(button);
			locked &= button != Glfw.MouseButton.Left;
		}

		public override void Update()
		{
			using (vkvg.Context ctx = new vkvg.Context(vkvgSurf))
			{
				ctx.SetSource(1, 1, 1);
				ctx.Paint();


				ctx.LineWidth = 6;
				ctx.SetSource (0.1, 0.1, 0.1, 0.8);

				ctx.MoveTo (points [0]);
				ctx.CurveTo (points [1].X, points [1].Y, points [2].X, points [2].Y, points [3].X, points [3].Y);
				ctx.Stroke ();

				ctx.LineWidth = 1;
				ctx.SetSource(0.2, 0.2, 0.2, 1);

				ctx.MoveTo(points[0]);
				ctx.LineTo(points[1]);
				ctx.MoveTo(points[2]);
				ctx.LineTo(points[3]);
				ctx.Stroke();

				ctx.SetSource (0.5, 0.5, 1, 0.9);
				for (int i = 0; i < points.Length; i++) {
					if (i == curPoint)
						continue;
					ctx.Arc (points[i].X, points [i].Y, cpRadius, 0, Math.PI * 2.0);
				}
				ctx.FillPreserve ();
				ctx.Stroke ();

				if (curPoint < 0)
					return;

				ctx.SetSource (1, 0.4, 0.4, 0.9);
				ctx.Arc (points [curPoint].X, points [curPoint].Y, selRadius, 0, Math.PI * 2.0);
				ctx.FillPreserve ();
				ctx.Stroke ();
			}
		}

	}
}
