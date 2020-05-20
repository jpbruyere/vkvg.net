// Copyright (c) 2018-2020  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;

namespace vkvg {

	public struct PointD
	{
		public PointD (double x, double y)
		{
			this.x = x;
			this.y = y;
		}

		double x, y;
		public double X {
			get { return x; }
			set { x = value; }
		}

		public double Y {
			get { return y; }
			set { y = value; }
		}
		public double Length {
			get { return Math.Sqrt (Math.Pow (x, 2) + Math.Pow (y, 2)); }
		}
		public static PointD operator / (PointD p, double d) {
			return new PointD (p.X / d, p.Y / d);
		}
		public static PointD operator * (PointD p, double d) {
			return new PointD (p.X * d, p.Y * d);
		}
		public static PointD operator + (PointD p1, PointD p2) {
			return new PointD (p1.X + p2.X, p1.Y + p2.Y);
		}
		public static PointD operator + (PointD p, double i) {
			return new PointD (p.X + i, p.Y + i);
		}
		public static PointD operator - (PointD p1, PointD p2) {
			return new PointD (p1.X - p2.X, p1.Y - p2.Y);
		}

	}
}
