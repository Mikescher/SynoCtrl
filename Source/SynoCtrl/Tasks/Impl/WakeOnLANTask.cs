﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SynoCtrl.Util;

namespace SynoCtrl.Tasks.Impl
{
	public class WakeOnLANTask : SCTask
	{
		protected override int Execute()
		{
			var config = FindConfig();

			if (config.Selected.MacAddressRaw == null) return WriteError("No MAC address specified");

			WriteDebug($"Target MAC address is {config.Selected.MacAddress}");
			WriteDebug();

			try
			{
				SendWOL(config.Selected.MacAddressRaw);
			}
			catch (Exception e)
			{
				return WriteError($"An error occured while sending WOL package: {e.Message}", e);
			}

			return WriteInfoOutput("Done.", $"WOL Package sent to {config.Selected.MacAddress}");
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