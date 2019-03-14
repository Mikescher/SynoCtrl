using System;
using System.Collections.Generic;
using System.Linq;

namespace SynoCtrl
{
	public static class SCUtil
	{
		public static string FormatByteArrayToHex(IList<byte> b, string bytesep = "", int linelen = -1, string nullValue = null, bool upper = false)
		{
			if (b == null) return nullValue;

			string ToHex(byte p) => (upper ? (Convert.ToString(p, 16).PadLeft(2, '0').ToUpper()) : (Convert.ToString(p, 16).PadLeft(2, '0')));

			if (linelen <= 0) return string.Join(bytesep, b.Select(ToHex));

			return string.Join(Environment.NewLine, Enumerable
				.Range(0, (int)Math.Ceiling(b.Count/(linelen*1d)))
				.Select(i1 => Enumerable.Range(i1*linelen, Math.Min(b.Count, i1*linelen+linelen)-i1*linelen))
				.Select(rr => string.Join(bytesep, rr.Select(idx => ToHex(b[idx])))));
		}
	}
}
