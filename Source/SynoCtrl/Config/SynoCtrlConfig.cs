using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSHC.Serialization;
using SynoCtrl.Tasks;

namespace SynoCtrl.Config
{
	public class SynoCtrlConfig
	{
		private static readonly string[] DEFAULT_PATHS =
		{
			@"synoctrl.toml",
			@"%userprofile%\.config\synoctrl.toml",
			@"%userprofile%\synoctrl.toml",
			@"%appdata%\synoctrl\synoctrl.toml",
		};

		public SingleDeviceConfig Selected;

		private SingleDeviceConfig _default;
		private List<SingleDeviceConfig> _configs;
		
		public static SynoCtrlConfig FindConfig()
		{
			if (SynoCtrlProgram.Arguments["--config"] != null)
			{
				var path = Path.GetFullPath(Environment.ExpandEnvironmentVariables($"{SynoCtrlProgram.Arguments["--config"].Value}"));
				SynoCtrlProgram.Logger.WriteDebug($"Loading config from '{path}'");
				if (!File.Exists(path))
				{
					SynoCtrlProgram.Logger.WriteDebug();
					throw new TaskException($"Configfile '{path}' not found");
				}
				try
				{
					var r = PatchConfigWithParams(Load(path));
					SynoCtrlProgram.Logger.WriteDebug($"Loaded config from '{path}'");
					SynoCtrlProgram.Logger.WriteDebug();
					return r;
				}
				catch (SynoCtrlConfigParseException e)
				{
					SynoCtrlProgram.Logger.WriteDebug();
					throw new TaskException(e.Message, e);
				}
			}

			foreach (var spath in DEFAULT_PATHS)
			{
				var path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(spath));
				SynoCtrlProgram.Logger.WriteDebug($"Trying to load config from '{path}'");
				if (!File.Exists(path))
				{ 
					SynoCtrlProgram.Logger.WriteDebug($"Config file does not exist: '{path}'");
					SynoCtrlProgram.Logger.WriteDebug();
					continue;
				}
				try
				{
					var r = PatchConfigWithParams(Load(path));
					SynoCtrlProgram.Logger.WriteDebug($"Loaded config from '{path}'");
					SynoCtrlProgram.Logger.WriteDebug();
					return r;
				}
				catch (SynoCtrlConfigParseException e)
				{
					SynoCtrlProgram.Logger.WriteDebug();
					throw new TaskException(e.Message, e);
				}
			}
			
			SynoCtrlProgram.Logger.WriteDebug("No config file loaded from the filesystem");
			SynoCtrlProgram.Logger.WriteDebug();

			if (SynoCtrlProgram.Arguments["<name>"] != null) throw new SynoCtrlConfigParseException($"Device '{SynoCtrlProgram.Arguments["<name>"].Value}' not found");

			return PatchConfigWithParams(CreateEmpty());
		}

		private static SynoCtrlConfig Load(string filename)
		{
			string content;
			try
			{
				content = File.ReadAllText(filename);
			}
			catch (IOException e)
			{
				throw new SynoCtrlConfigParseException("Could not load file: " + e.Message);
			}

			return Parse(content);
		}

		private static SynoCtrlConfig Parse(string content)
		{
			using(var reader = new StringReader(content))
			{
				TomlTable data;

				try
				{
					data = TOML.Parse(reader);
				}
				catch (TomlParseException e)
				{
					throw new SynoCtrlConfigParseException("Error in Toml Syntax: " + e.Message);
				}

				string defaultName = null;
				List<SingleDeviceConfig> devices = new List<SingleDeviceConfig>();

				if (data.HasKey("Default"))
				{
					if (data["Default"] is TomlString s)
					{
						if (!string.IsNullOrWhiteSpace(s)) defaultName = s;
					}
					else
					{
						throw new SynoCtrlConfigParseException("Key [Default] must be a string");
					}
				}

				if (data["Device"] is TomlArray a)
				{
					foreach (TomlNode n in a)
					{
						if (n is TomlTable t)
						{
							string cfgName = null;
							if (!t.HasKey("Name")) throw new SynoCtrlConfigParseException("Missing key [Name]");
							if (t["Name"] is TomlString vName) cfgName = vName;

							var cfg = new SingleDeviceConfig(cfgName, false);

							if (t.HasKey("IP")      ) { if (t["IP"]       is TomlString  vIP)       cfg.IPAddress  = vIP.Value.Trim();       else throw new SynoCtrlConfigParseException("Key [IP] must be a string"); }
							if (t.HasKey("Mac")     ) { if (t["Mac"]      is TomlString  vMac)      cfg.MACAddress = vMac.Value.Trim();      else throw new SynoCtrlConfigParseException("Key [Mac] must be a string"); }
							if (t.HasKey("Username")) { if (t["Username"] is TomlString  vUsername) cfg.Username   = vUsername.Value.Trim(); else throw new SynoCtrlConfigParseException("Key [Username] must be a string"); }
							if (t.HasKey("Password")) { if (t["Password"] is TomlString  vPassword) cfg.Password   = vPassword.Value.Trim(); else throw new SynoCtrlConfigParseException("Key [Password] must be a string"); }
							if (t.HasKey("Port"))     { if (t["Port"]     is TomlInteger vPort)     cfg.Port       = vPort.Value;            else throw new SynoCtrlConfigParseException("Key [Password] must be a string"); }
							if (t.HasKey("HTTPS"))    { if (t["HTTPS"]    is TomlBoolean vTLS)      cfg.UseTLS     = vTLS.Value;             else throw new SynoCtrlConfigParseException("Key [Password] must be a string"); }

							devices.Add(cfg);
						}
					}
				}

				var result = new SynoCtrlConfig { _configs = devices };
				if (defaultName != null)
				{
					result._default = result._configs.FirstOrDefault(c => string.Equals(c.Name, defaultName, StringComparison.CurrentCultureIgnoreCase));
					if (result._default == null) throw new SynoCtrlConfigParseException($"Device '{defaultName}' not found");
				} 
				else
				{
					result._default = new SingleDeviceConfig("%%AutoGenerated%%", true);
					result._configs.Add(result._default);
				}

				if (SynoCtrlProgram.Arguments["<name>"] != null)
				{
					var selName = $"{SynoCtrlProgram.Arguments["<name>"].Value}";
					result.Selected = result._configs.FirstOrDefault(c => string.Equals(c.Name, selName, StringComparison.CurrentCultureIgnoreCase));
					if (result.Selected == null) throw new SynoCtrlConfigParseException($"Device '{selName}' not found");
				}
				else
				{
					result.Selected = result._default;
				}

				return result;
			}
		}

		private static SynoCtrlConfig CreateEmpty()
		{
			var result = new SynoCtrlConfig();
			result._configs = new List<SingleDeviceConfig>();
			result._default = new SingleDeviceConfig("%%AutoGenerated%%", true);
			result._configs.Add(result._default);
			result.Selected = result._default;
			return result;
		}
		
		private static SynoCtrlConfig PatchConfigWithParams(SynoCtrlConfig cfg)
		{
			if (SynoCtrlProgram.Arguments["--mac"]      != null) cfg.Selected.MACAddress = GetStringArg("mac");
			if (SynoCtrlProgram.Arguments["--ip"]       != null) cfg.Selected.IPAddress  = GetStringArg("ip");
			if (SynoCtrlProgram.Arguments["--user"]     != null) cfg.Selected.Username   = GetStringArg("user");
			if (SynoCtrlProgram.Arguments["--password"] != null) cfg.Selected.Password   = GetStringArg("password");
			if (SynoCtrlProgram.Arguments["--port"]     != null) cfg.Selected.Port       = GetIntArg("port");
			if (SynoCtrlProgram.Arguments["--password"] != null) cfg.Selected.Password   = GetStringArg("password");

			return cfg;
		}

		private static int GetIntArg(string name)
		{
			if (SynoCtrlProgram.Arguments["--"+name] == null) throw new SynoCtrlConfigParseException($"Missing argument: --{name}");
			var v = $"{SynoCtrlProgram.Arguments["--"+name].Value}";

			if (int.TryParse(v, out var r)) return r;

			throw new SynoCtrlConfigParseException($"Argument --{name} must be an integer");
		}

		private static string GetStringArg(string name)
		{
			if (SynoCtrlProgram.Arguments["--"+name] == null) throw new SynoCtrlConfigParseException($"Missing argument: --{name}");
			return $"{SynoCtrlProgram.Arguments["--"+name].Value}";
		}
	}
}
