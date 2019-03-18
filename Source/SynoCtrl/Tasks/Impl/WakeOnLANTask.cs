using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using MSHC.Math.Encryption;

namespace SynoCtrl.Tasks.Impl
{
	public class WakeOnLANTask : SCTask
	{
		protected override int Execute()
		{
			var config = FindConfig();

			if (config.Selected.MACAddressRaw == null)
			{
				if (config.Selected.IPAddressRaw == null) return WriteError("No MAC address or IP address specified");
				
				WriteInfo($"No MAC address configured - trying to send ARP request to IP address {config.Selected.IPAddress}");
				WriteInfo();

				config.Selected.SetMACAddress(new GetMACTask().ExecuteDirect(config.Selected.IPAddressRaw));

				WriteInfo($"MAC address found: {config.Selected.MACAddress}");
				WriteInfo();
			}

			WriteDebug($"Target MAC address is {config.Selected.MACAddress}");
			WriteDebug();

			try
			{
				SendWOL(config.Selected.MACAddressRaw);
			}
			catch (TaskException)
			{
				throw;
			}
			catch (Exception e)
			{
				return WriteError($"An error occured while sending WOL package: {e.Message}", e);
			}

			return WriteInfoOutput("Done.", $"WOL Package sent to {config.Selected.MACAddress}");
		}

		private void SendWOL(byte[] macaddr)
		{
			var packet = new List<byte>(); 
			for (var i = 0; i < 6; i++) packet.Add(0xFF);
			for (var i = 0; i < 16; i++) packet.AddRange(macaddr);
			
			WriteDebug($"Sending UDP package to [{IPAddress.Broadcast}]:{42} with data\n{EncodingConverter.ByteArrayToHexDump(packet, ":", 6, "", true)}");
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
