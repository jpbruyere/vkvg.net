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
			last = current = cmds.Last;
		}

		public Command Current => current;
		object IEnumerator.Current => current;

		public void Dispose () {

		}

		public bool MoveNext () {
			current = current?.Previous;
			return current != null;
		}

		public void Reset () {
			current = last;
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
	public class Command {
		public bool relative;
		public Command Previous;
	
	}
	public class PathCommand
	{
		public PointD LastPoint;
	}
	public class MoveTo : Command
	{

	}
	public class LineTo : Command
	{
		public PointD p;
		public override string ToString () => relative ? $"l ${p.X},${p.Y}" : $"L ${p.X},${p.Y}";
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

