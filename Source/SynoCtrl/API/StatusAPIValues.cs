using System;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SynoCtrl.API
{
	public static class StatusAPIValues
	{
		private static readonly StatusAPIEndpoint DSM_INFO      = StatusAPIEndpoint.Create("SYNO.DSM.Info",                "getinfo",    2, null);
		private static readonly StatusAPIEndpoint SYSTEM_STATUS = StatusAPIEndpoint.Create("SYNO.Core.System.Status",      "get",        1, null);
		private static readonly StatusAPIEndpoint FS_INFO       = StatusAPIEndpoint.Create("SYNO.FileStation.Info",        "get",        2, null);
		private static readonly StatusAPIEndpoint FS_LIST       = StatusAPIEndpoint.Create("SYNO.FileStation.List",        "list_share", 2, null);
		private static readonly StatusAPIEndpoint CURR_CONN     = StatusAPIEndpoint.Create("SYNO.Core.CurrentConnection",  "list",       1, new[] { P("additional", "[\"real_path\",\"size\",\"owner\",\"time\",\"perm\",\"mount_point_type\",\"volume_status\"]") });
		private static readonly StatusAPIEndpoint NETWORK       = StatusAPIEndpoint.Create("SYNO.Core.Network",            "get",        1, null);
		private static readonly StatusAPIEndpoint SERVICE       = StatusAPIEndpoint.Create("SYNO.Core.Service",            "get",        1, null);
		private static readonly StatusAPIEndpoint SYSTEM        = StatusAPIEndpoint.Create("SYNO.Core.System",             "info",       3, null);
		private static readonly StatusAPIEndpoint UTILIZATION   = StatusAPIEndpoint.Create("SYNO.Core.System.Utilization", "get",        1, null);
		private static readonly StatusAPIEndpoint USER          = StatusAPIEndpoint.Create("SYNO.Core.User",               "list",       1, null);

		public static readonly StatusAPIValue[] VALUES =
		{
			StatusAPIValue.Create("Model",                DSM_INFO,      o => o.Value<string>("model"),                                  "The model of the device"),
			StatusAPIValue.Create("RAM",                  DSM_INFO,      o => o.Value<string>("ram")+"MB",                               "The amount of installed RAM"),
			StatusAPIValue.Create("Serial",               DSM_INFO,      o => o.Value<string>("serial"),                                 "The serial number"),
			StatusAPIValue.Create("Temperature",          DSM_INFO,      o => o.Value<string>("temperature")+"°C",                       "The current device temperature"),
			StatusAPIValue.Create("DeviceTime",           DSM_INFO,      o => o.Value<string>("time"),                                   "The current device time"),
			StatusAPIValue.Create("Uptime",               DSM_INFO,      o => o.Value<string>("uptime")+"s",                             "Time since last boot"),
			StatusAPIValue.Create("UptimeStr",            SYSTEM,        o => o.Value<string>("up_time"),                                "Time since last boot as formatted value"),
			StatusAPIValue.Create("VersionString",        DSM_INFO,      o => o.Value<string>("version_string"),                         "Currently running DSM version"),
			StatusAPIValue.Create("VersionRaw",           DSM_INFO,      o => o.Value<string>("version"),                                "Currently running DSM version number"),

			StatusAPIValue.Create("IsSystemCrashed",      SYSTEM_STATUS, o => o.Value<string>("is_system_crashed"),                      "Indicator if the system experienced a crash"),
			StatusAPIValue.Create("IsUpgradeReady",       SYSTEM_STATUS, o => o.Value<string>("upgrade_ready"),                          "Indicator if the system is ready to upgrade"),

			StatusAPIValue.Create("Hostname",             FS_INFO,       o => o.Value<string>("hostname"),                               "The DSM hostname"),
			StatusAPIValue.Create("UserIsAdmin",          FS_INFO,       o => o.Value<string>("is_manager"),                             "If the logged-in user is an administrator"),
			StatusAPIValue.Create("UserSupportsSharing",  FS_INFO,       o => o.Value<string>("support_sharing"),                        "Whether the logged-in user can share file(s)/folder(s)"),
			StatusAPIValue.Create("UserID",               FS_INFO,       o => o.Value<string>("uid"),                                    "UserID of the logged-in user"),

			StatusAPIValue.Create("ShareCount",           FS_LIST,       o => o.Value<int>("total").ToString(),                          "Number of shares"),

			StatusAPIValue.Create("ConnectionCount",      CURR_CONN,     o => o.Value<int>("total").ToString(),                          "Number of active connections"),

			StatusAPIValue.Create("ArpIgnore",            NETWORK,     o => o.Value<bool>("arp_ignore").ToString(),                      "Ignore ARP requests"),
			StatusAPIValue.Create("DnsManual",            NETWORK,     o => o.Value<string>("dns_manual"),                               "The manual configured DNS server"),
			StatusAPIValue.Create("PrimaryDNS",           NETWORK,     o => o.Value<string>("dns_primary"),                              "The configured primary DNS server"),
			StatusAPIValue.Create("SecondaryDNS",         NETWORK,     o => o.Value<string>("dns_secondary"),                            "The configured secondary DNS server"),
			StatusAPIValue.Create("WinDomain",            NETWORK,     o => o.Value<string>("enable_windomain"),                         "The configured Windows Domain"),
			StatusAPIValue.Create("Gateway",              NETWORK,     o => o.Value<string>("gateway"),                                  "The configured Gateway"),
			StatusAPIValue.Create("ServerName",           NETWORK,     o => o.Value<string>("server_name"),                              "The server name"),
			StatusAPIValue.Create("GatewayIPv6",          NETWORK,     o => o.Value<string>("v6gateway"),                                "The configured IPv6 Gateway"),

			StatusAPIValue.Create("Services",             SERVICE,     o => CountServicesAll(o["service"]),                              "All services"),
			StatusAPIValue.Create("ServicesEnabled",      SERVICE,     o => CountServicesEnabled(o["service"]),                          "Enabled services"),

			StatusAPIValue.Create("ClockSpeed",           SYSTEM,      o => o.Value<string>("cpu_clock_speed")+"Hz",                     "Clock speed of the CPU"),
			StatusAPIValue.Create("CoreCount",            SYSTEM,      o => o.Value<string>("cpu_cores"),                                "Number of CPU cores"),
			StatusAPIValue.Create("CPUFamily",            SYSTEM,      o => o.Value<string>("cpu_family"),                               "CPU family"),
			StatusAPIValue.Create("CPUSeries",            SYSTEM,      o => o.Value<string>("cpu_series"),                               "CPU series"),
			StatusAPIValue.Create("CPUVendor",            SYSTEM,      o => o.Value<string>("cpu_vendor"),                               "CPU vendor"),
			StatusAPIValue.Create("NTPEnabled",           SYSTEM,      o => o.Value<string>("enabled_ntp"),                              "Whether NTP enabled is"),
			StatusAPIValue.Create("FirmwareDate",         SYSTEM,      o => o.Value<string>("firmware_date"),                            "Date of the firmware"),
			StatusAPIValue.Create("FirmwareVersion",      SYSTEM,      o => o.Value<string>("firmware_ver"),                            "Version of the firmware"),
			StatusAPIValue.Create("NTPServer",            SYSTEM,      o => o.Value<string>("ntp_server"),                               "Configued NTP server"),
			StatusAPIValue.Create("Timezone",             SYSTEM,      o => o.Value<string>("time_zone"),                                "Configued timezone"),

			StatusAPIValue.Create("Load15min",            UTILIZATION, o => o["cpu"].Value<int>("15min_load").ToString(),                "Load average in the last 15 minutes"),
			StatusAPIValue.Create("Load5min",             UTILIZATION, o => o["cpu"].Value<int>("5min_load").ToString(),                 "Load average in the last 5 minutes"),
			StatusAPIValue.Create("Load1min",             UTILIZATION, o => o["cpu"].Value<int>("1min_load").ToString(),                 "Load average in the last minute"),
			StatusAPIValue.Create("DiscCount",            UTILIZATION, o => o["disk"]["disk"].Count().ToString(),                        "Number of mounted disks"),

			StatusAPIValue.Create("RamAvailableReal",     UTILIZATION, o => o["memory"].Value<int>("avail_real") + " byte",              "Available (real) RAM in bytes"),
			StatusAPIValue.Create("RamAvailableSwap",     UTILIZATION, o => o["memory"].Value<int>("avail_swap") + " byte",              "Available (swap) RAM in bytes"),
			StatusAPIValue.Create("BufferSize",           UTILIZATION, o => o["memory"].Value<int>("buffer") + " byte",                  "Buffer size"),
			StatusAPIValue.Create("CachedSize",           UTILIZATION, o => o["memory"].Value<int>("cached") + " byte",                  "Cached size"),
			StatusAPIValue.Create("MemorySize",           UTILIZATION, o => o["memory"].Value<int>("memory_size") + " byte",             "Memory size"),
			StatusAPIValue.Create("MemoryUsageReal",      UTILIZATION, o => o["memory"].Value<int>("real_usage") + " byte",              "Memory usage (real)"),
			StatusAPIValue.Create("MemoryUsageSwap",      UTILIZATION, o => o["memory"].Value<int>("swap_usage") + " byte",              "Memory usage (swap)"),
			StatusAPIValue.Create("TotalMemoryReal",      UTILIZATION, o => o["memory"].Value<int>("total_real") + " byte",              "Total memory (real)"),
			StatusAPIValue.Create("TotalMemorySwap",      UTILIZATION, o => o["memory"].Value<int>("total_swap") + " byte",              "Total Memory (swap)"),

			StatusAPIValue.Create("NetworkLoadTransmit",  UTILIZATION, o => o["network"][0].Value<string>("rx"),                         "Network load Recieve (RX)"),
			StatusAPIValue.Create("NetworkLoadRecieve",   UTILIZATION, o => o["network"][0].Value<string>("tx"),                         "Network load Transmit (TX)"),

			StatusAPIValue.Create("DiskAccessReadCount",  UTILIZATION, o => o["space"]["total"].Value<string>("read_access"),            "Disk utilization Read count"),
			StatusAPIValue.Create("DiskAccessReadBytes",  UTILIZATION, o => o["space"]["total"].Value<string>("read_byte") + " bytes",   "Disk utilization Read amount in bytes"),
			StatusAPIValue.Create("DiskAccessWriteCount", UTILIZATION, o => o["space"]["total"].Value<string>("write_access"),           "Disk utilization Write count"),
			StatusAPIValue.Create("DiskAccessWriteBytes", UTILIZATION, o => o["space"]["total"].Value<string>("write_byte") + " bytes",  "Disk utilization Write amount in bytes"),

			StatusAPIValue.Create("UserCount",            USER,        o => o.Value<int>("total").ToString(),                            "Amount of configured users"),
		};
		
		private static string CountServicesAll(JToken obj)     => obj.Children().Count().ToString();
		private static string CountServicesEnabled(JToken obj) => obj.Children().Count(c => c.Value<bool>("enable")).ToString();

		private static Tuple<string, string> P(string key, string value) => Tuple.Create(key, value);
	}
}
