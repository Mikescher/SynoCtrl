using System;
using Newtonsoft.Json.Linq;

namespace SynoCtrl.API
{
	public class StatusAPIValue
	{
		public readonly string ID;
		public readonly StatusAPIEndpoint Endpoint;
		public readonly Func<JObject, string> Getter;
		public readonly string Description;

		private StatusAPIValue(string i, StatusAPIEndpoint e, Func<JObject, string> g, string d)
		{
			ID = i;
			Endpoint = e;
			Getter = g;
			Description = d;
		}

		public static StatusAPIValue Create(string id, StatusAPIEndpoint ep, Func<JObject, string> getter, string desc)
		{
			return new StatusAPIValue(id, ep, getter, desc);
		}
	}
}
