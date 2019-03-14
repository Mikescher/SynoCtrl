using System;
using DocoptNet;
using SynoCtrl.Properties;

namespace SynoCtrl
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			var arguments = new Docopt().Apply(Resources.docopt, args, version: "Naval Fate 2.0", exit: true);
			Console.WriteLine("Hello World!");
		}
	}
}
