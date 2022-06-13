﻿// Copyright (c) 2018-2022  Jean-Philippe Bruyère <jp_bruyere@hotmail.com>
//
// This code is licensed under the MIT license (MIT) (http://opensource.org/licenses/MIT)

using System;
using System.Linq;
using System.Text;
using Drawing2D;

namespace vkvg
{
	public class Context : IContext
	{

		IntPtr handle = IntPtr.Zero;

		#region CTORS & DTOR
		public Context(Surface surf)
		{
			handle = NativeMethods.vkvg_create(surf.Handle);
		}
		~Context()
		{
			Dispose(false);
		}
		#endregion

		public IntPtr Handle => handle;
		public Status Status => NativeMethods.vkvg_status (handle);
		public void AddReference() => NativeMethods.vkvg_reference(handle);
		public uint References() => NativeMethods.vkvg_get_reference_count(handle);

		public double LineWidth
		{
			get => NativeMethods.vkvg_get_line_width (handle);
			set { NativeMethods.vkvg_set_line_width(handle, (float)value); }
		}
		public LineJoin LineJoin {
			get => NativeMethods.vkvg_get_line_join (handle);
			set { NativeMethods.vkvg_set_line_join (handle, value); }
		}
		public LineCap LineCap {
			get => NativeMethods.vkvg_get_line_cap (handle);
			set { NativeMethods.vkvg_set_line_cap (handle, value); }
		}
		public Operator Operator
		{
			set { NativeMethods.vkvg_set_operator(handle, value); }
			get { return NativeMethods.vkvg_get_operator(handle); }
		}
		public FillRule FillRule
		{
			set { NativeMethods.vkvg_set_fill_rule(handle, value); }
			get { return NativeMethods.vkvg_get_fill_rule(handle); }
		}
		public Antialias Antialias {
			set;
			get;
		}
		public uint FontSize
		{
			set { NativeMethods.vkvg_set_font_size(handle, value); }
		}
		public string FontFace
		{
			set { NativeMethods.vkvg_select_font_face(handle, value); }
		}
		public Drawing2D.FontExtents FontExtents
		{
			get
			{
				NativeMethods.vkvg_font_extents(handle, out vkvg.FontExtents e);
				return new Drawing2D.FontExtents (e.Ascent, e.Descent, e.Height, e.MaxXAdvance, e.MaxYAdvance);
			}
		}
		public void SetFontSize (double size) => NativeMethods.vkvg_set_font_size(handle, (uint)size);
		public void ShowText(string txt) => ShowText(txt.AsSpan());
		public void ShowText(TextRun textRun) => NativeMethods.vkvg_show_text_run(handle, textRun.Handle);
		public Matrix Matrix
		{
			get
			{
				Matrix m;
				NativeMethods.vkvg_get_matrix(handle, out m);
				return m;
			}
			set
			{
				NativeMethods.vkvg_set_matrix(handle, ref value);
			}
		}
		public void Save() => NativeMethods.vkvg_save(handle);
		public void Restore() => NativeMethods.vkvg_restore(handle);
		public void Flush() => NativeMethods.vkvg_flush(handle);
		public void Clear() => NativeMethods.vkvg_clear(handle);
		public void Paint() => NativeMethods.vkvg_paint(handle);
		public void ClosePath() => NativeMethods.vkvg_close_path (handle);

		public void NewPath() => NativeMethods.vkvg_new_path(handle);
		public void NewSubPath() => NativeMethods.vkvg_new_sub_path(handle);

		public void Arc(double xc, double yc, double radius, double a1, double a2)
			=> NativeMethods.vkvg_arc(handle, (float)xc, (float)yc, (float)radius, (float)a1, (float)a2);
		public void Arc (PointD center, double radius, double angle1, double angle2)
			=> NativeMethods.vkvg_arc (handle, (float)center.X, (float)center.Y, (float)radius, (float)angle1, (float)angle2);
		public void ArcNegative (PointD center, double radius, double angle1, double angle2)
			=> NativeMethods.vkvg_arc_negative (handle, (float)center.X, (float)center.Y, (float)radius, (float)angle1, (float)angle2);
		public void ArcNegative(double xc, double yc, double radius, double a1, double a2)
			=> NativeMethods.vkvg_arc_negative(handle, (float)xc, (float)yc, (float)radius, (float)a1, (float)a2);
		public void MoveTo(double x, double y) => NativeMethods.vkvg_move_to(handle, (float)x, (float)y);
		public void MoveTo(Point p) => NativeMethods.vkvg_move_to(handle, p.X, p.Y);
		public void MoveTo(PointD p) => NativeMethods.vkvg_move_to(handle, (float)p.X, (float)p.Y);
		public void LineTo(Point p) => NativeMethods.vkvg_line_to(handle, p.X, p.Y);
		public void LineTo(PointD p) => NativeMethods.vkvg_line_to(handle, (float)p.X, (float)p.Y);
		public void LineTo(double x, double y) => NativeMethods.vkvg_line_to(handle, (float)x, (float)y);
		public void CurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
			=> NativeMethods.vkvg_curve_to (handle, (float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3);
		public void RelMoveTo(double x, double y) => NativeMethods.vkvg_rel_move_to(handle, (float)x, (float)y);
		public void RelLineTo(double x, double y) => NativeMethods.vkvg_rel_line_to(handle, (float)x, (float)y);
		public void RelCurveTo(double x1, double y1, double x2, double y2, double x3, double y3)
			=> NativeMethods.vkvg_rel_curve_to(handle, (float)x1, (float)y1, (float)x2, (float)y2, (float)x3, (float)y3);
		public void Rectangle(double x, double y, double width, double height)
			=> NativeMethods.vkvg_rectangle (handle, (float)x, (float)y, (float)width, (float)height);
		public void Rectangle(Rectangle r)
			=> NativeMethods.vkvg_rectangle (handle, (float)r.X, (float)r.Y, (float)r.Width, (float)r.Height);
		public void MoveTo(float x, float y) => NativeMethods.vkvg_move_to(handle, x, y);
		public void RelMoveTo(float x, float y) => NativeMethods.vkvg_rel_move_to(handle, x, y);
		public void LineTo(float x, float y) => NativeMethods.vkvg_line_to(handle, x, y);
		public void RelLineTo(float x, float y) => NativeMethods.vkvg_rel_line_to(handle, x, y);
		public void CurveTo(float x1, float y1, float x2, float y2, float x3, float y3) => NativeMethods.vkvg_curve_to(handle, x1, y1, x2, y2, x3, y3);
		public void RelCurveTo(float x1, float y1, float x2, float y2, float x3, float y3) => NativeMethods.vkvg_rel_curve_to(handle, x1, y1, x2, y2, x3, y3);


		public void SetSource(IPattern pat) {
			if (pat is Pattern p)
				NativeMethods.vkvg_set_source (handle, p.Handle);
		}
		public void SetSource (Color color)
		{
			NativeMethods.vkvg_set_source_rgba (handle,
				(float)(color.R / 255.0), (float)(color.G / 255.0), (float)(color.B / 255.0), (float)(color.A / 255.0));
		}
		public void SetSource(ISurface surf, double x = 0, double y = 0)
			=> NativeMethods.vkvg_set_source_surface(handle, (surf as Surface).Handle, (float)x, (float)y);
		public void SetSource(double r, double g, double b, double a = 1.0)
			=> NativeMethods.vkvg_set_source_rgba(handle, (float)r, (float)g, (float)b, (float)a);
		public void RenderSvg(ISvgHandle svg, string subId = null) {
			if (svg is SvgHandle sh)
				sh.Render (this, subId);
		}
		Matrix savedMat = Matrix.Identity;
		public void SaveTransformations() => NativeMethods.vkvg_get_matrix (handle, out savedMat);
		public void RestoreTransformations() => NativeMethods.vkvg_set_matrix (handle, ref savedMat);


		public void Scale(double sx, double sy) => NativeMethods.vkvg_scale(handle, (float)sx, (float)sy);
		public void Translate(double dx, double dy) => NativeMethods.vkvg_translate(handle, (float)dx, (float)dy);
		public void Translate(PointD p) => NativeMethods.vkvg_translate(handle, (float)p.X, (float)p.Y);
		public void Rotate(double alpha) => NativeMethods.vkvg_rotate(handle, (float)alpha);


		public void Rectangle(float x, float y, float width, float height) => NativeMethods.vkvg_rectangle(handle, x, y, width, height);
		public void Scale(float sx, float sy) => NativeMethods.vkvg_scale(handle, sx, sy);
		public void Translate(float dx, float dy) => NativeMethods.vkvg_translate(handle, dx, dy);
		public void Rotate(float alpha) => NativeMethods.vkvg_rotate(handle, alpha);

		public void Fill() => NativeMethods.vkvg_fill(handle);
		public void FillPreserve() => NativeMethods.vkvg_fill_preserve(handle);
		public void Stroke() => NativeMethods.vkvg_stroke(handle);
		public void StrokePreserve() => NativeMethods.vkvg_stroke_preserve(handle);
		public void Clip() => NativeMethods.vkvg_clip(handle);
		public void ClipPreserve() => NativeMethods.vkvg_clip_preserve(handle);
		public void ResetClip() => NativeMethods.vkvg_reset_clip(handle);

		public void PopGroupToSource()
		{
			throw new NotImplementedException();
		}
		public void PushGroup()
		{
			throw new NotImplementedException();
		}

		public void PathExtents (out float x1, out float y1, out float x2, out float y2) {
			NativeMethods.vkvg_path_extents (handle, out x1, out y1, out x2, out y2);
		}

		/*public void SetSource(Pattern pat)
		{
			NativeMethods.vkvg_set_source(handle, pat.Handle);
		}*/
		public void SetSource(float r, float g, float b, float a = 1f) => NativeMethods.vkvg_set_source_rgba(handle, r, g, b, a);
		public void SetSource(Surface surf, float x = 0f, float y = 0f) => NativeMethods.vkvg_set_source_surface(handle, surf.Handle, x, y);
		public void SetSourceSurface(Surface surf, float x = 0f, float y = 0f) => NativeMethods.vkvg_set_source_surface(handle, surf.Handle, x, y);
		public void StartRecording () => NativeMethods.vkvg_start_recording(handle);
		public Recording StopRecording () => new Recording (NativeMethods.vkvg_stop_recording(handle));
		public void Replay (Recording rec) => NativeMethods.vkvg_replay (handle, rec.Handle);
		public void Replay (Recording rec, UInt32 commandIndex) => NativeMethods.vkvg_replay_command (handle, rec.Handle, commandIndex);

		public float[] Dashes {
			set {
				if (value == null)
					NativeMethods.vkvg_set_dash (handle, null, 0, 0);
				else
					NativeMethods.vkvg_set_dash (handle, value, (uint)value.Length, 0);
			}
		}
		public void SelectFontFace(string family, FontSlant slant, FontWeight weight)
		{
			NativeMethods.vkvg_select_font_face (handle, family);
		}
		internal static byte[] TerminateUtf8(string s)
		{
			// compute the byte count including the trailing \0
			var byteCount = Encoding.UTF8.GetMaxByteCount(s.Length + 1);
			var bytes = new byte[byteCount];
			Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
			return bytes;
		}
		public void SetDash (double [] dashes, double offset = 0) {
			if (dashes == null)
				NativeMethods.vkvg_set_dash(handle, null, 0, 0);
			else {
				float[] floats = dashes.Cast<float> ().ToArray ();
				NativeMethods.vkvg_set_dash(handle, floats, (uint)dashes.Length, (float)offset);
			}
		}

		public Drawing2D.TextExtents TextExtents (ReadOnlySpan<char> s, int tabSize = 4) {
			TextExtents (s, tabSize, out Drawing2D.TextExtents e);
			return e;
		}
		public void TextExtents (ReadOnlySpan<char> s, int tabSize, out Drawing2D.TextExtents extents) {
			if (s.Length == 0) {
				extents = default;
				return;
			}
			int size = s.Length * 4 + 1;
			Span<byte> bytes = size > 512 ? new byte[size] : stackalloc byte[size];
			int encodedBytes = s.ToUtf8 (bytes, tabSize);
			bytes[encodedBytes] = 0;
			TextExtents (bytes.Slice (0, encodedBytes + 1), out extents);
		}
		public void TextExtents (Span<byte> bytes, out Drawing2D.TextExtents extents) {
			NativeMethods.vkvg_text_extents (handle, ref bytes.GetPinnableReference (), out TextExtents e);
			extents = new Drawing2D.TextExtents (e.XBearing, e.YBearing, e.Width, e.Height, e.XAdvance, e.YAdvance);
		}
		public void ShowText (ReadOnlySpan<char> s, int tabSize = 4) {
			int size = s.Length * 4 + 1;
			Span<byte> bytes = size > 512 ? new byte[size] : stackalloc byte[size];
			int encodedBytes = s.ToUtf8 (bytes, tabSize);
			bytes[encodedBytes] = 0;
			ShowText (bytes.Slice (0, encodedBytes + 1));
		}
		public void ShowText (Span<byte> bytes) {
			NativeMethods.vkvg_show_text (handle, ref bytes.GetPinnableReference());
		}


		public void PaintWithAlpha(double alpha) => Paint();
		public Rectangle StrokeExtents()
		{
			throw new NotImplementedException();
		}


		#region IDisposable implementation
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing || handle == IntPtr.Zero)
				return;

			NativeMethods.vkvg_destroy(handle);
			handle = IntPtr.Zero;
		}
		#endregion
	}
}
