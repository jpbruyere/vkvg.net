using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace vkvg
{
	public abstract class Parameter {
		protected Command cmd;
		protected int offset;
		internal Parameter(Command cmd, int _offset) {
			this.cmd = cmd;
			offset = cmd.dataOffset + _offset;
		}
	}
	public class Parameter<T> : Parameter
									where T : struct
	{
		protected int size;
		public T Value {
			get {
				byte[] bytes = new byte[size];
				Marshal.Copy (cmd.rec.Data, bytes, offset, size);
				return MemoryMarshal.Cast<byte, T> (bytes.AsSpan())[0];
			}
			set {
				if (Value.Equals (value))
					return;
				byte[] bytes = null;
				if (value is float f)
					bytes = BitConverter.GetBytes (f);
				else if (value is Int32 i)
					bytes = BitConverter.GetBytes (i);
				else if (value is UInt32 u)
					bytes = BitConverter.GetBytes (u);
				Marshal.Copy (bytes, 0, IntPtr.Add(cmd.rec.Data, offset), size);
			}
		}
		internal Parameter (Command cmd, int _offset = 0)
			: base (cmd, _offset) {
			size = Marshal.SizeOf<T> ();
		}

	}
	public class Command {
		internal Recording rec;
		UInt32 index;
		Commands type;
		internal int dataOffset;
		public Commands Type => type;
		public IEnumerable<Parameter> Parameters => parameters;
		internal List<Parameter> parameters;
		internal void AddParameter<T> (int offset = 0) where T: struct{
			parameters.Add (new Parameter<T> (this, dataOffset + offset));
		}
		internal void AddParameters<T> (int count, int startOffset = 0) where T: struct{
			int size = Marshal.SizeOf<T> ();
			for (int i = 0; i < count; i++)
				parameters.Add (new Parameter<T> (this, dataOffset + startOffset + i * size));
		}

		internal Command (Recording rec, UInt32 index, Commands type, int dataOffset) {
			this.rec = rec;
			this.index = index;
			this.type = type;
			this.dataOffset = dataOffset;
		}
		internal static Command create (Recording rec, UInt32 index) {
			NativeMethods.vkvg_recording_get_command (rec.Handle, index, out UInt32 cmdType, out IntPtr dataOffset);
			Command cmd = new Command (rec, index, (Commands)cmdType, (int)dataOffset);
			switch (cmd.Type) {
				case Commands.VKVG_CMD_SET_FONT_SIZE:
				case Commands.VKVG_CMD_ROTATE:
				case Commands.VKVG_CMD_SET_LINE_WIDTH:
					cmd.AddParameter<float> ();
					break;
				case Commands.VKVG_CMD_REL_MOVE_TO:
				case Commands.VKVG_CMD_REL_LINE_TO:
				case Commands.VKVG_CMD_MOVE_TO:
				case Commands.VKVG_CMD_LINE_TO:
				case Commands.VKVG_CMD_TRANSLATE:
				case Commands.VKVG_CMD_SCALE:
					cmd.AddParameters<float> (2);
					break;
				case Commands.VKVG_CMD_SET_SOURCE_RGB:
					cmd.AddParameters<float> (3);
					break;
				case Commands.VKVG_CMD_RECTANGLE:
				case Commands.VKVG_CMD_QUADRATIC_TO:
				case Commands.VKVG_CMD_REL_QUADRATIC_TO:
				case Commands.VKVG_CMD_SET_SOURCE_RGBA:
					cmd.AddParameters<float> (4);
					break;
				case Commands.VKVG_CMD_ARC:
				case Commands.VKVG_CMD_ARC_NEG:
					cmd.AddParameters<float> (5);
					break;
				case Commands.VKVG_CMD_CURVE_TO:
				case Commands.VKVG_CMD_REL_CURVE_TO:
					cmd.AddParameters<float> (6);
					break;
				case Commands.VKVG_CMD_ELLIPTICAL_ARC_TO:
				case Commands.VKVG_CMD_REL_ELLIPTICAL_ARC_TO:
					cmd.AddParameters<float> (5);
					cmd.AddParameters<bool> (2);
					break;
				case Commands.VKVG_CMD_TRANSFORM:
				case Commands.VKVG_CMD_SET_MATRIX:
					cmd.AddParameter<Matrix> ();
					break;
				case Commands.VKVG_CMD_SET_LINE_JOIN:
				case Commands.VKVG_CMD_SET_LINE_CAP:
				case Commands.VKVG_CMD_SET_OPERATOR:
				case Commands.VKVG_CMD_SET_FILL_RULE:
					cmd.AddParameter<UInt32> ();
					break;
				case Commands.VKVG_CMD_SET_SOURCE_COLOR:
					cmd.AddParameter<Drawing2D.Color> ();
					break;

				case Commands.VKVG_CMD_SET_DASH:
				case Commands.VKVG_CMD_SET_FONT_FACE:
				case Commands.VKVG_CMD_SET_FONT_PATH:
				case Commands.VKVG_CMD_SHOW_TEXT:
				case Commands.VKVG_CMD_SET_SOURCE:
				case Commands.VKVG_CMD_SET_SOURCE_SURFACE:
					break;
			}
			return cmd;
		}

	}
	public enum Commands {
		VKVG_CMD_SAVE				= 0x0001,
		VKVG_CMD_RESTORE			= 0x0002,

		VKVG_CMD_PATH_COMMANDS		= 0x0100,
		VKVG_CMD_DRAW_COMMANDS		= 0x0200,
		VKVG_CMD_RELATIVE_COMMANDS	= (0x0400|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_PATHPROPS_COMMANDS	= (0x1000|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_PRESERVE_COMMANDS	= (0x0400|VKVG_CMD_DRAW_COMMANDS),
		VKVG_CMD_PATTERN_COMMANDS	= 0x0800,
		VKVG_CMD_TRANSFORM_COMMANDS	= 0x2000,
		VKVG_CMD_TEXT_COMMANDS		= 0x4000,

		VKVG_CMD_NEW_PATH			= (0x0001|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_NEW_SUB_PATH		= (0x0002|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_CLOSE_PATH			= (0x0003|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_MOVE_TO			= (0x0004|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_LINE_TO			= (0x0005|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_RECTANGLE			= (0x0006|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_ARC				= (0x0007|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_ARC_NEG			= (0x0008|VKVG_CMD_PATH_COMMANDS),
		//VKVG_CMD_ELLIPSE			= (0x0009|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_CURVE_TO			= (0x000A|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_QUADRATIC_TO		= (0x000B|VKVG_CMD_PATH_COMMANDS),
		VKVG_CMD_ELLIPTICAL_ARC_TO	= (0x000C|VKVG_CMD_PATH_COMMANDS),

		VKVG_CMD_SET_LINE_WIDTH		= (0x0001|VKVG_CMD_PATHPROPS_COMMANDS),
		VKVG_CMD_SET_LINE_JOIN		= (0x0002|VKVG_CMD_PATHPROPS_COMMANDS),
		VKVG_CMD_SET_LINE_CAP		= (0x0003|VKVG_CMD_PATHPROPS_COMMANDS),
		VKVG_CMD_SET_OPERATOR		= (0x0004|VKVG_CMD_PATHPROPS_COMMANDS),
		VKVG_CMD_SET_FILL_RULE		= (0x0005|VKVG_CMD_PATHPROPS_COMMANDS),
		VKVG_CMD_SET_DASH			= (0x0006|VKVG_CMD_PATHPROPS_COMMANDS),

		VKVG_CMD_TRANSLATE			= (0x0001|VKVG_CMD_TRANSFORM_COMMANDS),
		VKVG_CMD_ROTATE				= (0x0002|VKVG_CMD_TRANSFORM_COMMANDS),
		VKVG_CMD_SCALE				= (0x0003|VKVG_CMD_TRANSFORM_COMMANDS),
		VKVG_CMD_TRANSFORM			= (0x0004|VKVG_CMD_TRANSFORM_COMMANDS),
		VKVG_CMD_IDENTITY_MATRIX	= (0x0005|VKVG_CMD_TRANSFORM_COMMANDS),

		VKVG_CMD_SET_MATRIX			= (0x0006|VKVG_CMD_TRANSFORM_COMMANDS),

		VKVG_CMD_SET_FONT_SIZE		= (0x0001|VKVG_CMD_TEXT_COMMANDS),
		VKVG_CMD_SET_FONT_FACE		= (0x0002|VKVG_CMD_TEXT_COMMANDS),
		VKVG_CMD_SET_FONT_PATH		= (0x0003|VKVG_CMD_TEXT_COMMANDS),
		VKVG_CMD_SHOW_TEXT			= (0x0004|VKVG_CMD_TEXT_COMMANDS),

		VKVG_CMD_REL_MOVE_TO			= (VKVG_CMD_MOVE_TO			|VKVG_CMD_RELATIVE_COMMANDS),
		VKVG_CMD_REL_LINE_TO			= (VKVG_CMD_LINE_TO			|VKVG_CMD_RELATIVE_COMMANDS),
		VKVG_CMD_REL_CURVE_TO			= (VKVG_CMD_CURVE_TO			|VKVG_CMD_RELATIVE_COMMANDS),
		VKVG_CMD_REL_QUADRATIC_TO		= (VKVG_CMD_QUADRATIC_TO		|VKVG_CMD_RELATIVE_COMMANDS),
		VKVG_CMD_REL_ELLIPTICAL_ARC_TO	= (VKVG_CMD_ELLIPTICAL_ARC_TO	|VKVG_CMD_RELATIVE_COMMANDS),

		VKVG_CMD_PAINT				= (0x0001|VKVG_CMD_DRAW_COMMANDS),
		VKVG_CMD_FILL				= (0x0002|VKVG_CMD_DRAW_COMMANDS),
		VKVG_CMD_STROKE				= (0x0003|VKVG_CMD_DRAW_COMMANDS),
		VKVG_CMD_CLIP				= (0x0004|VKVG_CMD_DRAW_COMMANDS),
		VKVG_CMD_RESET_CLIP			= (0x0005|VKVG_CMD_DRAW_COMMANDS),
		VKVG_CMD_CLEAR				= (0x0006|VKVG_CMD_DRAW_COMMANDS),

		VKVG_CMD_FILL_PRESERVE		= (VKVG_CMD_FILL	|VKVG_CMD_PRESERVE_COMMANDS),
		VKVG_CMD_STROKE_PRESERVE	= (VKVG_CMD_STROKE	|VKVG_CMD_PRESERVE_COMMANDS),
		VKVG_CMD_CLIP_PRESERVE		= (VKVG_CMD_CLIP	|VKVG_CMD_PRESERVE_COMMANDS),

		VKVG_CMD_SET_SOURCE_RGB		= (0x0001|VKVG_CMD_PATTERN_COMMANDS),
		VKVG_CMD_SET_SOURCE_RGBA	= (0x0002|VKVG_CMD_PATTERN_COMMANDS),
		VKVG_CMD_SET_SOURCE_COLOR	= (0x0003|VKVG_CMD_PATTERN_COMMANDS),
		VKVG_CMD_SET_SOURCE			= (0x0004|VKVG_CMD_PATTERN_COMMANDS),
		VKVG_CMD_SET_SOURCE_SURFACE	= (0x0005|VKVG_CMD_PATTERN_COMMANDS)
	}
}