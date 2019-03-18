using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using MSHC.Math.Encryption;
using SynoCtrl.API;
using SynoCtrl.Util;

namespace SynoCtrl.Tasks.Impl
{
	public class StatusTask : SCTask
	{
		protected override int Execute()
		{
			var config = FindConfig();
			
			if (config.Selected.IPAddressRaw == null)
			{
				if (config.Selected.MACAddressRaw == null) return WriteError("No IP address or MAC address specified");

				WriteInfo($"No IP address configured - trying to get IP address by MAC address ({config.Selected.MACAddress})");
				WriteInfo();

				config.Selected.SetIPAddress(new GetIPTask().ExecuteDirect(config.Selected.MACAddressRaw));

				WriteInfo($"IP address found: {config.Selected.IPAddress}");
				WriteInfo();
			}

			if (config.Selected.Port == null)
			{
				config.Selected.SetPort(config.Selected.UseTLS ? SynologyAPI.DEFAULT_PORT_TLS : SynologyAPI.DEFAULT_PORT_HTTP);
				
				WriteInfo($"No port found - using default DSM port {config.Selected.Port}");
				WriteInfo();
			}

			if (config.Selected.Name     == null) throw new TaskException("No username for API access specified");
			if (config.Selected.Password == null) throw new TaskException("No password for API access specified");

			WriteDebug($"Target IP address is {config.Selected.IPAddress}");
			WriteDebug();

			var values = new List<StatusAPIValue>();
			if (SynoCtrlProgram.Arguments["status-all"].IsTrue || SynoCtrlProgram.Arguments["<status_fields>"].Value.ToString().ToLower() == "all")
			{
				values = StatusAPIValues.VALUES.ToList();
			}
			else
			{
				foreach (var arg in SynoCtrlProgram.Arguments["<status_fields>"].Value.ToString().Split(',', ';'))
				{
					var match = StatusAPIValues.VALUES.FirstOrDefault(v => string.Equals(v.ID, arg.Trim(), StringComparison.CurrentCultureIgnoreCase));

					if (match == null) throw new TaskException($"Unknown status field: [{arg}]");

					if (!values.Contains(match)) values.Add(match);
				}
			}

			SynologyAPI.Status(config.Selected.IPAddressRaw, config.Selected.Port ?? -1, config.Selected.UseTLS, config.Selected.Username, config.Selected.Password, values);

			return 0;
		}

	}
}
