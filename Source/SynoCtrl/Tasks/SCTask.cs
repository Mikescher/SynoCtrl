using System;
using System.IO;
using SynoCtrl.Config;

namespace SynoCtrl.Tasks
{
	public abstract class SCTask
	{
		private static readonly string[] DEFAULT_PATHS =
		{
			@"synoctrl.toml",
			@"%userprofile%\.config\synoctrl.toml",
			@"%userprofile%\synoctrl.toml",
			@"%appdata%\synoctrl\synoctrl.toml",
		};

		protected int WriteError(string msg, Exception e = null) => SynoCtrlProgram.Logger.WriteError(msg, e);

		protected int WriteOutput(string msg) => SynoCtrlProgram.Logger.WriteOutput(msg);

		protected int WriteInfoOutput(string msgSilent, string msgNormal) => SynoCtrlProgram.Logger.WriteInfoOutput(msgSilent, msgNormal);

		protected void WriteInfo(string msg = "") => SynoCtrlProgram.Logger.WriteInfo(msg);

		protected void WriteDebug(string msg = "") => SynoCtrlProgram.Logger.WriteDebug(msg);

		protected SynoCtrlConfig FindConfig()
		{
			if (SynoCtrlProgram.Arguments["--config"] != null)
			{
				var path = Path.GetFullPath(Environment.ExpandEnvironmentVariables($"{SynoCtrlProgram.Arguments["--config"].Value}"));
				WriteDebug($"Loading config from '{path}'");
				if (!File.Exists(path))
				{
					WriteDebug();
					throw new TaskException($"Configfile '{path}' not found");
				}
				try
				{
					var r = PatchConfigWithParams(SynoCtrlConfig.Load(path));
					WriteDebug($"Loaded config from '{path}'");
					WriteDebug();
					return r;
				}
				catch (SynoCtrlConfigParseException e)
				{
					WriteDebug();
					throw new TaskException(e.Message, e);
				}
			}

			foreach (var spath in DEFAULT_PATHS)
			{
				var path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(spath));
				WriteDebug($"Trying to load config from '{path}'");
				if (!File.Exists(path))
				{ 
					WriteDebug($"Config file does not exist: '{path}'");
					WriteDebug();
					continue;
				}
				try
				{
					var r = PatchConfigWithParams(SynoCtrlConfig.Load(path));
					WriteDebug($"Loaded config from '{path}'");
					WriteDebug();
					return r;
				}
				catch (SynoCtrlConfigParseException e)
				{
					WriteDebug();
					throw new TaskException(e.Message, e);
				}
			}
			
			WriteDebug("No config file loaded from the filesystem");
			WriteDebug();

			if (SynoCtrlProgram.Arguments["<name>"] != null) throw new SynoCtrlConfigParseException($"Device '{SynoCtrlProgram.Arguments["<name>"].Value}' not found");

			return PatchConfigWithParams(SynoCtrlConfig.CreateEmpty());
		}

		private SynoCtrlConfig PatchConfigWithParams(SynoCtrlConfig cfg)
		{
			if (SynoCtrlProgram.Arguments["--mac"]      != null) cfg.Selected.MACAddress = $"{SynoCtrlProgram.Arguments["--mac"].Value}";
			if (SynoCtrlProgram.Arguments["--ip"]       != null) cfg.Selected.IPAddress  = $"{SynoCtrlProgram.Arguments["--ip"].Value}";
			if (SynoCtrlProgram.Arguments["--user"]     != null) cfg.Selected.Username   = $"{SynoCtrlProgram.Arguments["--user"].Value}";
			if (SynoCtrlProgram.Arguments["--password"] != null) cfg.Selected.Password   = $"{SynoCtrlProgram.Arguments["--password"].Value}";

			return cfg;
		}

		public int Run()
		{
			try
			{
				return Execute();
			}
			catch (TaskException e1)
			{
				return WriteError(e1.Message, e1.InnerException);
			}
			catch (Exception e2)
			{
				return WriteError(e2.Message, e2);
			}
		}

		protected abstract int Execute();
	}
}
