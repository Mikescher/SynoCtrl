using System;

namespace SynoCtrl.API
{
	public class StatusAPIEndpoint
	{
		public readonly string API;
		public readonly string Method;
		public readonly int? Version;
		public readonly Tuple<string, string>[] Parameter;

		private StatusAPIEndpoint(string api, string m, int? v, Tuple<string, string>[] p)
		{
			API = api;
			Method = m;
			Version = v;
			Parameter = p;
		}

		public static StatusAPIEndpoint Create(string api, string method, int? version, Tuple<string, string>[] parameter)
		{
			return new StatusAPIEndpoint(api, method, version, parameter);
		}
	}
}
