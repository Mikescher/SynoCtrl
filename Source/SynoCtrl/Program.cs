using System;
using System.Diagnostics;
using System.Reflection;
using DocoptNet;

namespace SynoCtrl
{
	public static class Program
	{
		public static readonly Version VERSION = GetInformationalVersion();

		public static void Main(string[] args)
		{
			var arguments = new Docopt().Apply(Properties.Resources.cmd_schema, args, true, VERSION);
		}

		private static Version GetInformationalVersion()
		{
			try
			{
				var assembly = Assembly.GetAssembly(typeof(Program));

				var loc = assembly.Location;
				var vi = FileVersionInfo.GetVersionInfo(loc);
				return new Version(vi.FileMajorPart, vi.FileMinorPart, vi.FileBuildPart, vi.FilePrivatePart);
			}
			catch (Exception)
			{
				return new Version(0, 0, 0, 0);
			}
		}
	}
}
