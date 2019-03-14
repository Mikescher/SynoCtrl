using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security;

namespace SynoCtrl.Tasks.Impl
{
	public class GetMacTask : SCTask
	{
		[DllImport("iphlpapi.dll", ExactSpelling = true)]
		[SecurityCritical]
		static extern int SendARP(int destinationIp, int sourceIp, byte[] macAddress, ref int physicalAddrLength); 

		protected override int Execute()
		{
			var config = FindConfig();

			if (config.Selected.IPAddress == null) return WriteError("No IP address specified");

			WriteDebug($"Target IP address is {config.Selected.IPAddress}");
			WriteDebug();

			try
			{
				var mac = SendARP(config.Selected.IPAddressRaw);
				var strmac = SCUtil.FormatByteArrayToHex(mac, ":", -1, string.Empty, true);
				return WriteInfoOutput(strmac, $"The MAC address of {config.Selected.IPAddress} is {strmac}");
			}
			catch (Exception e)
			{
				return WriteError($"An error occured while sending WOL package: {e.Message}", e);
			}
		}

		private byte[] SendARP(IPAddress ipaddr)
		{
			var destIp = BitConverter.ToInt32(ipaddr.GetAddressBytes(), 0);

			var addr = new byte[6];
			var len = addr.Length;
			
			WriteDebug($"Sending ARP package to {(uint)destIp}");
			WriteDebug();

			var res = SendARP(destIp, 0, addr, ref len);

			if (res != 0) throw new TaskException($"ARP Request failed (Errorcode {res} -- {new Win32Exception(res).Message})");

			return addr; 
		}
	}
}
