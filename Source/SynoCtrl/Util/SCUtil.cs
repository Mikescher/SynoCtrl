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
