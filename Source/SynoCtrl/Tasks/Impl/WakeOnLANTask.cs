using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using MSHC.Math.Encryption;
using MSHC.Util.Helper;

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
				SendWOL(config.Selected.MACAddressRaw, config.Selected.IPAddressRaw);
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

		private void SendWOL(byte[] macaddr, IPAddress ipaddr)
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

			if (ipaddr != null && ipaddr.AddressFamily == AddressFamily.InterNetwork)
			{
				WriteDebug($"Trying to determine the broadcast adress for [{ipaddr}] in all available adapters");
				WriteDebug();

				foreach (var iface in NetworkInterface.GetAllNetworkInterfaces().Where(p => p.NetworkInterfaceType != NetworkInterfaceType.Loopback).OrderBy(p => p.OperationalStatus))
				{
					var anymatch = false;
					foreach (var ipinfo in iface.GetIPProperties().UnicastAddresses.AsEnumerable().Where(p => p.Address.AddressFamily == AddressFamily.InterNetwork))
					{
						if (ipaddr.IsPartOfSubnet(ipinfo.Address, ipinfo.IPv4Mask))
                        {
							WriteDebug($"Found matching subnet [{ipinfo.Address}|{ipinfo.IPv4Mask}] in adapter \"{iface.Name}\"");
							WriteDebug();

							anymatch = true;

							var broadcast = ipinfo.Address.GetBroadcastAddress(ipinfo.IPv4Mask);
							if (broadcast == null) continue;

							WriteDebug($"Sending UDP package to [{broadcast}]:{42} with data\n{EncodingConverter.ByteArrayToHexDump(packet, ":", 6, "", true)}");
							WriteDebug();

							using (var client = new UdpClient())
							{
								client.Connect(broadcast, 42);
								client.Send(packet.ToArray(), packet.Count);
								client.Close();
							}
						}
					}
					if (! anymatch)
					{
						WriteDebug($"Found no matching subnet in adapter \"{iface.Name}\"");
						WriteDebug();
					}
				}
			}

		}
	}
}
