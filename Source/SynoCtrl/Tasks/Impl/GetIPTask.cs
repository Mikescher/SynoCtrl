using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using MSHC.Util.Helper;
using SynoCtrl.Util;

namespace SynoCtrl.Tasks.Impl
{
	public class GetIPTask : SCTask
	{
		protected override int Execute()
		{
			var config = FindConfig();

			if (config.Selected.MACAddressRaw == null) return WriteError("No MAC address specified");
			
			var ip = ExecuteDirect(config.Selected.MACAddressRaw);
			return WriteInfoOutput(ip.ToString(), $"The IP address of {config.Selected.MACAddress} is {ip}");
		}

		public IPAddress ExecuteDirect(byte[] macaddr)
		{
			WriteDebug($"Executing arp -a");
			var output = ProcessHelper.ProcExecute("arp", "-a");
			WriteDebug(output.StdCombined);
			WriteDebug();

			if (output.ExitCode != 0) throw new TaskException($"Call to [arp -a] failed - Exitcode {output.ExitCode}");

			var arpdata = ParseARPOutput(output.StdOut);

			var mac = arpdata.FirstOrDefault(p => p.Item2.SequenceEqual(macaddr));
			if (mac != null) return mac.Item1;
			
			throw new TaskException($"MAC Address not in local ARP table - Cannot determine IP Adress");
		}

		private static IEnumerable<Tuple<IPAddress, byte[]>> ParseARPOutput(string stdout)
		{
			var result = new List<Tuple<IPAddress, byte[]>>();

			foreach (var line in stdout.Split('\n').Select(p => p.Trim('\r')))
			{
				if (string.IsNullOrWhiteSpace(line)) continue;

				var cols = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
				if (cols.Length != 3) continue;

				if (!IPAddress.TryParse(cols[0], out var ipaddr)) continue;

				var macaddr = SCUtil.ParseMacAddress(cols[1], false);
				if (macaddr == null) continue;

				result.Add(Tuple.Create(ipaddr, macaddr));
			}

			return result;
		}
	}
}
