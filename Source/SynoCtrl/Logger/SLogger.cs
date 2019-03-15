using System;

namespace SynoCtrl.Logger
{
	public class SLogger
	{
		private readonly bool silent;
		private readonly bool verbose;

		public SLogger(bool silent, bool verbose)
		{
			this.silent = silent;
			this.verbose = verbose && !silent;
		}

		public int WriteError(string msg, Exception e = null)
		{
			Console.Error.WriteLine("[ERROR] " + msg);
			if (e != null && verbose) Console.Error.WriteLine("[TRACE] " + e);
			return -1;
		}

		public int WriteOutput(string msg)
		{
			Console.Out.WriteLine(msg);
			return 0;
		}

		public int WriteInfoOutput(string msgSilent, string msgNormal)
		{
			if (silent) 
				Console.Out.WriteLine(msgSilent);
			else
				Console.Out.WriteLine(msgNormal);
			return 0;
		}

		public void WriteInfo(string msg = "")
		{
			if (silent) return;
			Console.Out.WriteLine(msg);
		}

		public void WriteDebug(string msg = "")
		{
			if (!verbose) return;
			Console.Out.WriteLine(msg);
		}
	}
}
