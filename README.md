# SynoCtrl [![Build status](https://ci.appveyor.com/api/projects/status/lk6vwv65v7cmy98b?svg=true)](https://ci.appveyor.com/project/Mikescher/synoctrl)

SynoCtrl is a command-line program to remotely control your Synology NAS

## Features

 - Show Synology status (`synoctrl status-all [<name>] [--info] [options]`)
 - Shutdown Synology NAS (`synoctrl shutdown [<name>] [options]`)
 - Reboot Synology NAS (`synoctrl reboot [<name>] [options]`)
 - Boot up Synology NAS via WOL (`synoctrl wol [<name>] [options]`)
 - Send ping package (`synoctrl ping [<name>] [options]`)
 - Get IP adress by MAC (`synoctrl getip [<name>] [options]`)
 - Get MAC address by IP via ARP (`synoctrl getmac [<name>] [options]`)

## Configuration

You do not need to create a configuration file and can use SynoCtrl completely portable only via command-line parameter.  
But (for convenience) you can specify properties of your NAS (or multiple NAS') in a config file and reference them in your program call:

~~~TOML
Default = "MyDevice"             # The default device to use if no specific device is specified
                                 # Can be removed if there shall be no default device.

[[Device]]
Name     = "MyDevice"            # Device name
IP       = "192.168.0.20"        # IP address (optional)
Port     = 1924                  # Port of DiskStation WebInterface (optional)
HTTPS    = false                 # Enable HTTPS Communication (optional)
Mac      = "00-11-22-33-44-55"   # MAC address (optional)
Username = "usr"                 # DSM Web interface username (optional)
Password = "pass"                # DSM Web interface password (optional)


# You can add more than one device:

# [[Device]]
# Name     = "Demo2"               # Device name
# IP       = "10.10.10.99"         # IP address (optional)
# Port     = 1924                  # Port of DiskStation WebInterface (optional)
# HTTPS    = false                 # Enable HTTPS Communication (optional)
# Mac      = "00-A1-B2-C3-D4-E5"   # MAC address  (optional)
# Username = "admin"               # DSM Web interface username (optional)
# Password = "admin"               # DSM Web interface password (optional)


# You can add more devices here and use them by their device-names in the command-line interface
# ...
~~~
*(This is [TOML](https://github.com/toml-lang/toml) syntax)*


Then you can call `synoctrl shutdown MyDevice` to shutdown the device with the name "MyDevice".  
If no config file exists or no device name is given you have to supply all necessary parameters via command-line parameter (In this case `--ip`, `--username`, `--password`)

All fields except the *Name* field are optional in the TOML config file are optional and can be removed, in this case you need to supply these parameters in the program call if they are necessary.

The config file location can be set with the command line parameter `--config`. If no `--config` parameter is set the standard locations are searched for a config file (in this order):
 - `%program_dir%\synoctrl.toml`
 - `%userprofile%\.config\synoctrl.toml`
 - `%userprofile%\synoctrl.toml`
 - `%appdata%\synoctrl\synoctrl.toml`

You can create a example config file with the command `synoctrl create-config synoctrl.toml`

## Options / Command-line parameter

| Parameter | Description |
| -- | -- |
| `-h` `--help` | Show the command-line help |
| `--version`  | Show the version |
| `--mac <mac>`  | The MAC address of the device |
| `--ip <ip>`  | The IP address of the device |
| `--port <port>`  | The Port of the DSM Web interface (if not set the default port 5000/5001 is used |
| `--https`  | Use a TLS encrypted connection to the DSM Web interface |
| `-u <user>` `--user <user>`  | The username for the DSM Web interface (and API) |
| `-p <pass>` `--password <pass>`  | The password for the DSM Web interface (and API) |
| `-c <config>` `--config <config>`  | Use the config file at this specific path, do not search in other places |
| `-s`, `--silent`  | Only output the result (or errors), useful if you want to parse the output |
| `-v`, `--verbose`  | Output debug/trace messages, useful for debugging |


## Commands

### Shutdown/Reboot

You can shutdown the device `Melkor` with the command `synoctrl shutdown MyDevice`.  
This uses the Synology DSM API to send an shutdown request.  
If you do not have a configuration file you can supply all parameter by yourself:  
`synoctrl shutdown --ip 192.168.0.100 --user admin --password admin`  
If you want to reboot your device (instead of shutdown) you can use the *reboot* command `synoctrl reboot MyDevice`

### Boot-up (Wake-on-LAN)

You can boot the device by sending a Wake-on-LAN package to the MAC address of your NAS.  
For this to work you have to [enable WOL in your device config](https://www.synology.com/en-us/knowledgebase/search/Wake%20on%20LAN).  
The command is `synoctrl wol MyDevice`, if you do not have a configured device the command is `synoctrl wol --mac 00:11:22:33:44:55`

### Get IP/MAC address

For convenience two commands are available to get the MAC address from an IP address (via ARP) and to get the IP address from a MAC address (via you local ARP table):  
`synoctrl getip MyDevice` and `synoctrl getmac MyDevice` or `synoctrl getip --mac 00:11:22:33:44:55` and `synoctrl getmac --ip 192.168.0.100`.  
For obvious reasons *getip* only works when the device is powered on (and accepts ARP requests) and *getmac* only works if the device is already in your local ARP table.

### Status

SynoCtrl can show many different status fields. All these data is gathered via the API, so if you don't have a config file with the respective data supplied you need to at least set the parameters `--ip`, `--username` and `--password`.

You can list all available status fields with `synoctrl status-list` (see [Appendix 1](#appendix-1-available-status-fields)).

The *status-all* command returns all available status fields, this can take a moment because multiple API endpoints have to be called. (`synoctrl status-all MyDevice`).

But the normal way is to call *status* together with a comma-separated list of fields you want to get:  
`synoctrl status Model,MemoryUsageReal,Uptime MyDevice` or `synoctrl status Load5min MyDevice`

If you only want the raw unformatted value you can add `--silent` and if you want the value together with the field description you can add `--info`

##Contribution

Contributions are always welcome, if you're missing something feel free to send a pull request. Also if you find a bug or have a feature request create an [issue](https://github.com/Mikescher/SynoCtrl/issues).

## License

[MIT](https://github.com/Mikescher/AlephNote/blob/master/LICENSE)


## Appendix 1: Available Status fields

~~~
          Name          |   Description
------------------------|------------------------------------------------------
 Model                  | The model of the device
 RAM                    | The amount of installed RAM
 Serial                 | The serial number
 Temperature            | The current device temperature
 DeviceTime             | The current device time
 Uptime                 | Time since last boot
 UptimeStr              | Time since last boot as formatted value
 VersionString          | Currently running DSM version
 VersionRaw             | Currently running DSM version number
 IsSystemCrashed        | Indicator if the system experienced a crash
 IsUpgradeReady         | Indicator if the system is ready to upgrade
 Hostname               | The DSM hostname
 UserIsAdmin            | If the logged-in user is an administrator
 UserSupportsSharing    | Whether the logged-in user can share file(s)/folder(s)
 UserID                 | UserID of the logged-in user
 ShareCount             | Number of shares
 ConnectionCount        | Number of active connections
 ArpIgnore              | Ignore ARP requests
 DnsManual              | The manual configured DNS server
 PrimaryDNS             | The configured primary DNS server
 SecondaryDNS           | The configured secondary DNS server
 WinDomain              | The configured Windows Domain
 Gateway                | The configured Gateway
 ServerName             | The server name
 GatewayIPv6            | The configured IPv6 Gateway
 Services               | All services
 ServicesEnabled        | Enabled services
 ClockSpeed             | Clock speed of the CPU
 CoreCount              | Number of CPU cores
 CPUFamily              | CPU family
 CPUSeries              | CPU series
 CPUVendor              | CPU vendor
 NTPEnabled             | Whether NTP enabled is
 FirmwareDate           | Date of the firmware
 FirmwareVersion        | Version of the firmware
 NTPServer              | Configued NTP server
 Timezone               | Configued timezone
 Load15min              | Load average in the last 15 minutes
 Load5min               | Load average in the last 5 minutes
 Load1min               | Load average in the last minute
 DiscCount              | Number of mounted disks
 RamAvailableReal       | Available (real) RAM in bytes
 RamAvailableSwap       | Available (swap) RAM in bytes
 BufferSize             | Buffer size
 CachedSize             | Cached size
 MemorySize             | Memory size
 MemoryUsageReal        | Memory usage (real)
 MemoryUsageSwap        | Memory usage (swap)
 TotalMemoryReal        | Total memory (real)
 TotalMemorySwap        | Total Memory (swap)
 NetworkLoadTransmit    | Network load Recieve (RX)
 NetworkLoadRecieve     | Network load Transmit (TX)
 DiskAccessReadCount    | Disk utilization Read count
 DiskAccessReadBytes    | Disk utilization Read amount in bytes
 DiskAccessWriteCount   | Disk utilization Write count
 DiskAccessWriteBytes   | Disk utilization Write amount in bytes
 UserCount              | Amount of configured users
~~~