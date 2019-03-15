using System;

namespace SynoCtrl.Tasks
{
	public class TaskException : Exception
	{
		public TaskException(string message) : base(message) { }
		public TaskException(string message, Exception innerException) : base(message, innerException) { }
	}
}
