// Copyright (c) 2013-2019  Bruyère Jean-Philippe jp_bruyere@hotmail.com
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;

namespace vkvg
{
	public class CommandEnumerator : IEnumerator<Command>
	{
		Command last;
		Command current;

		internal CommandEnumerator (CommandCollection cmds) {
			last = cmds.Last;
			current = null;
		}

		public Command Current => current;
		object IEnumerator.Current => current;

		public void Dispose () {

		}

		public bool MoveNext () {
			current = (current == null) ?
				last : current.Previous;
			return current != null;
		}

		public void Reset () {
			current = null;
		}
	}
	/// <summary>
	/// inversed linked list starting by last element holding previous command.
	/// </summary>
	public class CommandCollection : IEnumerable<Command>
	{
		Command last;
		public Command Last => last;
		public IEnumerator<Command> GetEnumerator () => new CommandEnumerator (this);
		IEnumerator IEnumerable.GetEnumerator () => new CommandEnumerator (this);
		public bool IsEmpty => last == null;

		public CommandCollection () { }
		public CommandCollection (params Command[] cmds) {
			foreach (var cmd in cmds) 
				Add (cmd);
		}

		public void Add (Command cmd) {
			cmd.Previous = last;
			last = cmd;
		}
		public void Remove (Command cmd) {
			if (last == null)
				return;
			if (cmd == last) {
				last = cmd.Previous;
				return;
			}

			using (IEnumerator<Command> e = GetEnumerator ()) {
				Command previous = last;
				while (e.MoveNext ()) {
					previous = e.Current;
					if (e.Current == cmd)
						break;
				}
				previous.Previous = cmd.Previous;
			}
		}
	}
	public abstract class Command {
		public Command Previous;
		public abstract void Execute (Context ctx);
	}
	public enum DrawCommandType
	{
		Stroke,
		Fill,
		Clip,
		Paint
	}
	public class DrawCommand : Command
	{
		public DrawCommandType DrawType;
		public bool preserve;
		public override void Execute (Context ctx) {
			switch (DrawType) {
			case DrawCommandType.Stroke:
				if (preserve)
					ctx.StrokePreserve ();
				else
					ctx.Stroke ();
				break;
			case DrawCommandType.Fill:
				if (preserve)
					ctx.FillPreserve ();
				else
					ctx.Fill ();
				break;
			case DrawCommandType.Clip:
				if (preserve)
					ctx.ClipPreserve ();
				else
					ctx.Clip ();
				break;
			case DrawCommandType.Paint:
				ctx.Paint ();
				break;			
			}
		}
	}
	public abstract class PathCommand : Command
	{
		static double cpRadius = 10, selRadius = 3;
		public bool relative;
		public PointD A;
		public virtual PointD this [int i] {
			get => A;
			set => A = value;
		}
		public virtual int Length => 1;
		public virtual void DrawPoints (Context ctx, int selectedPoint = -1) {
			for (int i = 0; i < Length; i++) {
				PointD p = this [i];

				if (i == selectedPoint)
					ctx.SetSource (1, 0.6, 0.6, 0.6);
				else
					ctx.SetSource (0.6, 0.6, 1, 0.6);

				ctx.Rectangle (p.X - selRadius, p.Y - selRadius, selRadius * 2, selRadius * 2);
				ctx.FillPreserve ();

				if (i == selectedPoint)
					ctx.SetSource (1, 0.3, 0.3, 0.8);
				else
					ctx.SetSource (0.3, 0.3, 1, 0.8);

				ctx.Stroke ();
			}

		}
		public virtual bool IsOver (PointD m, out int pointIndex) {
			for (int i = 0; i < Length; i++) {
				PointD p = this [i];
				if (p.X - cpRadius < m.X && p.X + cpRadius > m.X && p.Y - cpRadius < m.Y && p.Y + cpRadius > m.Y) {
					pointIndex = i;
					return true;
				}
			}
			pointIndex = -1;
			return false;
		}

	}
	public class Move : PathCommand
	{
		public override void Execute (Context ctx) {
			if (relative)
				ctx.RelMoveTo (A.X, A.Y); 
			else
				ctx.MoveTo(A.X,A.Y);
		}
	}
	public class Line : PathCommand
	{
		public override void Execute (Context ctx) {
			if (relative)
				ctx.RelLineTo (A.X, A.Y);
			else
				ctx.LineTo (A.X, A.Y);
		}
		//public override string ToString () => relative ? $"l ${p.X},${p.Y}" : $"L ${p.X},${p.Y}";
	}
	public class Arc : PathCommand
	{
		public bool Negative;
		public double radius, startAngle, endAngle;

		public override void Execute (Context ctx) {
			if (Negative)
				ctx.ArcNegative (A.X, A.Y, radius, startAngle, endAngle);
			else
				ctx.Arc (A.X, A.Y, radius, startAngle, endAngle);
		}
	}
	public class Curve : PathCommand
	{
		public PointD controlPoint1, controlPoint2;
		public override void Execute (Context ctx) {
			if (relative)
				ctx.RelCurveTo (controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y, A.X, A.Y);
			else
				ctx.CurveTo (controlPoint1.X, controlPoint1.Y, controlPoint2.X, controlPoint2.Y, A.X, A.Y);
		}
	}

	static class ExtensionMethods
	{
		public static bool IsWhiteSpaceOrNewLine (this char c) {
			return c == '\t' || c == '\r' || c == '\n' || char.IsWhiteSpace (c);
		}
	}
	public class PathParser : StringReader
	{
		public PathParser (string str) : base (str) { }

		double readDouble () {
			StringBuilder tmp = new StringBuilder ();

			while (Peek () >= 0) {
				char c = (char)Read ();
				if (c.IsWhiteSpaceOrNewLine ()) {
					if (tmp.Length == 0)
						continue;
					else
						break;
				} 
				if (c == ',')
					break;
				tmp.Append (c);
			}
			return double.Parse (tmp.ToString ());
		}
		public void Draw (Context gr) {
			while (Peek () >= 0) {
				char c = (char)Read ();
				if (c.IsWhiteSpaceOrNewLine ())
					continue;
				switch (c) {
				case 'M':
					gr.MoveTo (readDouble (), readDouble ());
					break;
				case 'm':
					gr.RelMoveTo (readDouble (), readDouble ());
					break;
				case 'L':
					gr.LineTo (readDouble (), readDouble ());
					break;
				case 'l':
					gr.RelLineTo (readDouble (), readDouble ());
					break;
				case 'C':
					gr.CurveTo (readDouble (), readDouble (), readDouble (), readDouble (), readDouble (), readDouble ());
					break;
				case 'c':
					gr.RelCurveTo (readDouble (), readDouble (), readDouble (), readDouble (), readDouble (), readDouble ());
					break;
				case 'Z':
					gr.ClosePath ();
					break;
				case 'F':
					gr.Fill ();
					break;
				case 'G':
					gr.Stroke ();
					break;
				default:
					throw new Exception ("Invalid character in path string of Shape control");
				}
			}
		}
	}
}

