// Copyright (c) 2018-2020  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

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
	}
}
