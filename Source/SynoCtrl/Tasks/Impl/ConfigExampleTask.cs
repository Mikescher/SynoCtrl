using System;
using System.IO;
using SynoCtrl.Util;

namespace SynoCtrl.Tasks.Impl
{
	public class ConfigExampleTask : SCTask
	{
		private readonly string destination;

		public ConfigExampleTask(string filename)
		{
			destination = filename;
		}

		protected override int Execute()
		{
			var path = Path.GetFullPath(Environment.ExpandEnvironmentVariables(destination));

			if (File.Exists(path) && !SynoCtrlProgram.Arguments["--override"].IsTrue) throw new TaskException($"File already exists: '{path}'");

			File.WriteAllText(path, Properties.Resources.config_example);
			WriteDebug($"Writing example config to '{path}'");
			WriteDebug();
			WriteDebug();
			WriteDebug(Properties.Resources.config_example);
			WriteDebug();
			WriteDebug();

			WriteInfoOutput($"Success.", $"Example TOML config file written to {destination}\n"+
			                             $"Edit the config file and copy it to one of these destinations to use it:\n"+
			                             @"    %program_location%\synoctrl.toml"   + "\n" +
			                             @"    %userprofile%\.config\synoctrl.toml"+ "\n" +
			                             @"    %userprofile%\synoctrl.toml"        + "\n" +
			                             @"    %appdata%\synoctrl\synoctrl.toml"   + "\n");

			return 0;
		}

	}
}