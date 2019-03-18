using System;
using SynoCtrl.Config;

namespace SynoCtrl.Tasks
{
	public abstract class SCTask
	{
		protected int WriteError(string msg, Exception e = null) => SynoCtrlProgram.Logger.WriteError(msg, e);

		protected int WriteOutput(string msg) => SynoCtrlProgram.Logger.WriteOutput(msg);

		protected int WriteInfoOutput(string msgSilent, string msgNormal) => SynoCtrlProgram.Logger.WriteInfoOutput(msgSilent, msgNormal);

		protected void WriteInfo(string msg = "") => SynoCtrlProgram.Logger.WriteInfo(msg);

		protected void WriteDebug(string msg = "") => SynoCtrlProgram.Logger.WriteDebug(msg);
		
		protected SynoCtrlConfig FindConfig() => SynoCtrlConfig.FindConfig();

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
