using SynoCtrl.Util;

namespace SynoCtrl.Tasks.Impl
{
	public class ShutdownTask : SCTask
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

			SynologyAPI.Shutdown(config.Selected.IPAddressRaw, config.Selected.Port ?? -1, config.Selected.UseTLS, config.Selected.Username, config.Selected.Password);

			WriteInfoOutput($"Success.", $"Shutdown message sent to to {config.Selected.IPAddress}");

			return 0;
		}

	}
}
