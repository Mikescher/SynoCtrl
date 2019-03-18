using SynoCtrl.API;

namespace SynoCtrl.Tasks.Impl
{
	public class StatusListTask : SCTask
	{
		protected override int Execute()
		{
			WriteInfo();
			WriteInfo("        Name        |   Description                                            ");
			WriteInfo("--------------------|----------------------------------------------------------");
			foreach (var v in StatusAPIValues.VALUES) WriteInfo($" {v.ID.PadRight(19, ' ')}| {v.Description}");
			WriteInfo();

			return 0;
		}

	}
}
