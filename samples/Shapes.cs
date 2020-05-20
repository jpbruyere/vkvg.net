// Copyright (c) 2013-2020  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)
using System;
using System.Collections.Generic;
using System.Linq;
using Crow;

namespace vkvg.Shape
{
	public class Path : Shape {
		bool closed;

		public bool Closed {
			get => closed;
			set {
				if (value == closed)
					return;
				closed = value;
				NotifyValueChanged ("Closed", closed);
			}
		}
		public override PointD Center { 
			get {
				return new PointD ();
			}
		}
		public Path (PointD firstPoint) {
			pathCommands = new CommandCollection (new vkvg.Move () { A = firstPoint });
		}
	}
	public abstract class Shape : IValueChange {
		#region IValueChange implementation
		public event EventHandler<ValueChangeEventArgs> ValueChanged;
		public virtual void NotifyValueChanged (string MemberName, object _value) {
			ValueChanged?.Invoke (this, new ValueChangeEventArgs (MemberName, _value));
		}
		#endregion

		uint lineWidth = 20;
		LineJoin lineJoin = LineJoin.Miter;
		LineCap lineCap = LineCap.Butt;
		Fill fillColor;
		Fill strokeColor = Color.GreenYellow.AdjustAlpha (0.6f);
		bool enableDash;
		ObservableList<ValueContainer<float>> dashes = new ObservableList<ValueContainer<float>> ();
		protected vkvg.CommandCollection pathCommands;

		public uint LineWidth {
			get => lineWidth;
			set {
				if (value == lineWidth)
					return;
				lineWidth = value;
				NotifyValueChanged ("LineWidth", lineWidth);
			}
		}
		public LineJoin LineJoin {
			get => lineJoin;
			set {
				if (value == lineJoin)
					return;
				lineJoin = value;
				NotifyValueChanged ("LineJoin", lineJoin);
			}
		}
		public LineCap LineCap {
			get => lineCap;
			set {
				if (value == lineCap)
					return;
				lineCap = value;
				NotifyValueChanged ("LineCap", lineCap);
			}
		}
		public Color FillColor {
			get => fillColor;
			set {
				if (value == fillColor)
					return;
				fillColor = value;
				NotifyValueChanged ("FillColor", fillColor);
			}
		}
		public Color StrokeColor {
			get => strokeColor;
			set {
				if (value == strokeColor)
					return;
				strokeColor = value;
				NotifyValueChanged ("StrokeColor", strokeColor);
			}
		}
		public bool EnableDash {
			get => enableDash;
			set {
				if (value == enableDash)
					return;
				enableDash = value;
				NotifyValueChanged ("EnableDash", enableDash);
			}
		}
		public ObservableList<ValueContainer<float>> Dashes {
			set {
				if (dashes == value)
					return;
				dashes = value;
				NotifyValueChanged ("Dashes", dashes);
			}
			get => dashes;
		}

		public PointD Translation;
		public double Rotation;

		public CommandCollection PathCommands {
			get => pathCommands;
			set {
				if (pathCommands == value)
					return;
				pathCommands = value;
				NotifyValueChanged ("PathCommands", pathCommands);
			}
		}
		public virtual void Draw (Context ctx, PointD? mousePos = null) {
			ctx.Save ();
			ctx.Translate (Translation);
			if (enableDash && dashes.Count > 0)
				ctx.Dashes = dashes.Select (d => d.Value).ToArray ();
			ctx.LineWidth = lineWidth;
			ctx.LineJoin = lineJoin;
			ctx.LineCap = lineCap;

			foreach (Command cmd in PathCommands.ToArray ().Reverse ())
				cmd.Execute (ctx);
			if (mousePos != null)
				ctx.LineTo ((vkvg.PointD)mousePos);


			if (fillColor == null) {
				if (strokeColor == null)
					return;
				strokeColor.SetAsSource (ctx);
				ctx.Stroke ();
				return;
			}
			fillColor.SetAsSource (ctx);
			if (strokeColor == null)
				ctx.Fill ();
			else {
				ctx.FillPreserve ();
				strokeColor.SetAsSource (ctx);
				ctx.Stroke ();
			}
			ctx.Restore ();
		}
		public void DrawPoints (Context ctx, PathCommand selectedCmd = null, int selectedPoint = -1) {
			foreach (PathCommand cmd in pathCommands.OfType<PathCommand> ()) {
				ctx.LineWidth = 1;
				cmd.DrawPoints (ctx, cmd == selectedCmd ? selectedPoint : -1);
			}
		}
		public virtual bool IsOver (PointD m, out PathCommand cmd, out int pointIndex) {
			m -= Translation; 
			foreach (PathCommand c in pathCommands.OfType<PathCommand> ()) {
				if (c.IsOver (m, out pointIndex)) {
					cmd = c;
					return true;
				}
			}
			cmd = null;
			pointIndex = -1;
			return false;
		}

		public abstract PointD Center { get; }

		public Shape () {
		}
		public Shape (PointD firstPoint) {
			pathCommands = new CommandCollection(new vkvg.Move () { A = firstPoint });
		}
	}
}
