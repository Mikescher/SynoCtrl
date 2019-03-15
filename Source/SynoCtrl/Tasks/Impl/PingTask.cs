using System;
using System.Net;
using System.Net.NetworkInformation;

namespace SynoCtrl.Tasks.Impl
{
	public class PingTask : SCTask
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
			
			WriteDebug($"Pinging {config.Selected.IPAddress}");
			WriteDebug();

			var r = PingHost(config.Selected.IPAddressRaw);

			if (r != null)
			{
				if (r.Status == IPStatus.Success) return WriteInfoOutput($"Success.", $"Ping to {config.Selected.IPAddress} succeeded in {r.RoundtripTime}ms");

				throw new Exception("Ping failed due to " + r.Status);
			}

			throw new Exception("Ping failed");
		}


		public PingReply PingHost(IPAddress addr)
		{
			Ping socket = null;
			try
			{
				socket = new Ping();
				return socket.Send(addr);
			}
			catch (PingException e)
			{
				throw new TaskException("Ping failed due to " + e.Message, e);
			}
			finally
			{
				socket?.Dispose();
			}
		}
	}
}
