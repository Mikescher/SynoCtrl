using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SynoCtrl.Tasks;

namespace SynoCtrl.Util
{
	public static class SCUtil
	{
		private static readonly Regex REX_MAC_1 = new Regex(@"^[0-9A-Fa-f]{2}(:[0-9A-Fa-f]{2}){5}$", RegexOptions.Compiled);
		private static readonly Regex REX_MAC_2 = new Regex(@"^[0-9A-Fa-f]{2}(-[0-9A-Fa-f]{2}){5}$", RegexOptions.Compiled);
		private static readonly Regex REX_MAC_3 = new Regex(@"^([0-9A-Fa-f]{2}){6}$", RegexOptions.Compiled);

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

		public static byte[] ParseMacAddress(string addr, bool throwExceptionOnError)
		{
			if (string.IsNullOrWhiteSpace(addr)) return null;

			if (REX_MAC_1.IsMatch(addr)) return addr.Split(':').Select(p => Convert.ToByte(p, 16)).ToArray();
			if (REX_MAC_2.IsMatch(addr)) return addr.Split('-').Select(p => Convert.ToByte(p, 16)).ToArray();
			if (REX_MAC_3.IsMatch(addr)) return Enumerable.Range(0,6).Select(i => Convert.ToByte(addr.Substring(2*i, 2),16)).ToArray();

			if (throwExceptionOnError) throw new TaskException($"Not a valid MAC address: '{addr}'");
			return null;
		}
	}
}
