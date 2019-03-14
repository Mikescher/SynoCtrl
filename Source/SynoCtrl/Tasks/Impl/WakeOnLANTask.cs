using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace SynoCtrl.Tasks.Impl
{
	public class WakeOnLANTask : SCTask
	{
		protected override int Execute()
		{
			var config = FindConfig();

			if (config.Default.MacAddressRaw == null) return WriteError("No MAC address specified");

			WriteDebug($"Target MAC address is {config.Default.MacAddress}");
			WriteDebug();

			try
			{
				SendWOL(config.Default.MacAddressRaw);
			}
			catch (Exception e)
			{
				return WriteError($"An error occured while sending WOL package: {e.Message}", e);
			}

			return WriteOutput($"WOL Package sent to {config.Default.MacAddress}");
		}

		private void SendWOL(byte[] macaddr)
		{
			var packet = new List<byte>(); 
			for (var i = 0; i < 6; i++) packet.Add(0xFF);
			for (var i = 0; i < 16; i++) packet.AddRange(macaddr);
			
			WriteDebug($"Sending UDP package to [{IPAddress.Broadcast}]:{42} with data\n{SCUtil.FormatByteArrayToHex(packet, ":", 6, "", true)}");
			WriteDebug();

			using(var client = new UdpClient())
			{
				client.Connect(IPAddress.Broadcast, 42);
				client.Send(packet.ToArray(), packet.Count); 
				client.Close();
			}
		}
	}
}
