using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security;
using MSHC.Math.Encryption;

namespace SynoCtrl.Tasks.Impl
{
	public class GetMACTask : SCTask
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
				var mac = ExecuteDirect(config.Selected.IPAddressRaw);
				var strmac = EncodingConverter.ByteArrayToHexDump(mac, ":", -1, string.Empty, true);
				return WriteInfoOutput(strmac, $"The MAC address of {config.Selected.IPAddress} is {strmac}");
			}
			catch (TaskException)
			{
				throw;
			}
			catch (Exception e)
			{
				return WriteError($"An error occured while sending ARP package: {e.Message}", e);
			}
		}

		public byte[] ExecuteDirect(IPAddress ipaddr)
		{
			return SendARP(ipaddr);
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

			if (addr.All(b => b == 0)) throw new TaskException($"ARP Request failed");

			return addr; 
		}
	}
}
