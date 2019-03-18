using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SynoCtrl.Tasks;

namespace SynoCtrl.API
{
	public static class SynologyAPI // https://global.download.synology.com/download/Document/DeveloperGuide/Synology_Download_Station_Web_API.pdf
	                                // https://global.download.synology.com/download/Document/DeveloperGuide/Synology_File_Station_API_Guide.pdf
	                                // https://github.com/satreix/synology
	                                // https://github.com/yannickcr/node-synology
	                                // https://github.com/kwent/syno/blob/master/definitions/DSM/5.2/5967/
	{
		public const int DEFAULT_PORT_HTTP = 5000;
		public const int DEFAULT_PORT_TLS  = 5001;
	
		private static readonly Random RAND = new Random();

		public static void Status(IPAddress addr, long port, bool tls, string username, string password, List<StatusAPIValue> values)
		{
			var session = Login(addr, port, tls, username, password);

			var requests = values.Select(v => v.Endpoint).Distinct().ToList();
			var data = new Dictionary<StatusAPIEndpoint, JObject>();

			foreach (var req in requests)
			{
				data.Add(req, Query(addr, port, tls, req.API, req.Method, req.Version, session, req.Parameter));
			}

			var pad1 = values.Max(v => v.ID.Length);
			var pad2 = values.Max(v => v.Getter(data[v.Endpoint]).Length);

			foreach (var val in values)
			{
				var info = val.Getter(data[val.Endpoint]);
				var norm = $"{val.ID.PadRight(pad1, ' ')} : {info}";
				var verb = $"{val.ID.PadRight(pad1, ' ')} : {info.PadRight(pad2, ' ')}   # {val.Description}";

				if (SynoCtrlProgram.Arguments["--info"].IsTrue) info = norm = verb;

				SynoCtrlProgram.Logger.WriteInfoOutputVerbose(info, norm, verb);
			}

			Logout(addr, port, tls, session);
		}

		public static void Shutdown(IPAddress addr, long port, bool tls, string username, string password)
		{
			var session = Login(addr, port, tls, username, password);

			Query(addr, port, tls, "SYNO.Core.System", "shutdown", 1, session, null);
		}

		public static void Reboot(IPAddress addr, long port, bool tls, string username, string password)
		{
			var session = Login(addr, port, tls, username, password);

			Query(addr, port, tls, "SYNO.Core.System", "reboot", 1, session, null);
		}

		public static Tuple<string, string> Login(IPAddress addr, long port, bool tls, string username, string password)
		{
			var sn = $"SynoCtrl_{DateTime.Now.ToFileTimeUtc()}_{RAND.Next(int.MaxValue)}";

			var data = Query(addr, port, tls, "SYNO.API.Auth", "login", 6, null, new[]{P("account", username), P("passwd", password), P("format", "sid"), P("session", sn)});

			var sid = data["sid"].Value<string>();
			
			SynoCtrlProgram.Logger.WriteDebug($"Authentication with API successful. Cookie SID = {sid}");
			SynoCtrlProgram.Logger.WriteDebug();

			return Tuple.Create(sid, sn);
		}

		public static void Logout(IPAddress addr, long port, bool tls, Tuple<string, string> session)
		{
			Query(addr, port, tls, "auth.cgi", "SYNO.API.Auth", 2, "logout", null, new[]{ P("session", session.Item2) });
			
			SynoCtrlProgram.Logger.WriteDebug($"API session closed.");
			SynoCtrlProgram.Logger.WriteDebug();
		}

		private static Tuple<string, string> P(string key, string value) => Tuple.Create(key, value);
		
		private static JObject Query(IPAddress addr, long port, bool tls, string api, string method, int? version, Tuple<string, string> session, Tuple<string, string>[] parameter)
		{
			var info = Query(addr, port, tls, "query.cgi", "SYNO.API.Info", 1, "query", null, new[] { P("query", api) });

			if (!info.ContainsKey(api)) throw new TaskException($"API target {api}' not found");

			var path = info[api]["path"].Value<string>();
			var versMin = info[api]["minVersion"].Value<int>();
			var versMax = info[api]["maxVersion"].Value<int>();

			var vers = versMax;
			if (version != null && version>=versMin && version <= versMax) vers = version.Value;
	
			return Query(addr, port, tls, path, api, vers, method, session, parameter);
		}

		// ReSharper disable once DelegateSubtraction
		private static JObject Query(IPAddress addr, long port, bool tls, string endpoint, string api, int version, string method, Tuple<string, string> auth, Tuple<string, string>[] parameter)
		{
			try
			{
				using (var wc = new HttpClient())
				{
					var uristr = $"{(tls?"https":"http")}://{addr}:{port}/webapi/{endpoint}?api={api}&version={version}&method={method}";
					if (parameter != null) foreach (var up in parameter) uristr += $"&{up.Item1}={HttpUtility.UrlEncode(up.Item2)}";
					if (auth != null) uristr += $"&_sid={auth.Item1}";

					var uri = new Uri(uristr);

					SynoCtrlProgram.Logger.WriteDebug($"Sending API request to {uri}");

					var request = new HttpRequestMessage
					{
						RequestUri = uri,
						Method = HttpMethod.Get,
						Headers =
						{
							{"User-Agent", $"SynoCtrl_{SynoCtrlProgram.VERSION}"},
						}
					};
				
					HttpResponseMessage response;
					if (tls)
					{
						var prot = ServicePointManager.SecurityProtocol;
						ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;
						ServicePointManager.ServerCertificateValidationCallback += AcceptAll;
						response = wc.SendAsync(request).Result;
						ServicePointManager.ServerCertificateValidationCallback -= AcceptAll;
						ServicePointManager.SecurityProtocol = prot;
					}
					else
					{
						response = wc.SendAsync(request).Result;
					}

					if (!response.IsSuccessStatusCode)
					{
						string content = string.Empty;
						try
						{
							content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result);
						}
						catch (Exception)
						{
							// ignore
						}
						
						SynoCtrlProgram.Logger.WriteDebug($"API responded with status code {response.StatusCode}: {content}");
						SynoCtrlProgram.Logger.WriteDebug();
						
						throw new TaskException($"Login API Request failed with status code {response.StatusCode}");
					}
					else
					{
						var content = Encoding.UTF8.GetString(response.Content.ReadAsByteArrayAsync().Result);

						SynoCtrlProgram.Logger.WriteDebug($"API responded with status code {response.StatusCode}: {content}");
						SynoCtrlProgram.Logger.WriteDebug();
						
						var json = JObject.Parse(content);

						if (json.ContainsKey("success") && json.GetValue("success").Value<bool>()) return (JObject) json["data"];

						if (json["error"]?["code"] == null) throw new TaskException($"API query failed with {json["error"].ToString(Formatting.None)}");

						var errorcode = json["data"]["code"].Value<int>();

						var errormessage = SynologyAPIErrors.GetErrorMessage(api, errorcode);
						if (errormessage != null) throw new TaskException($"API query failed with error {errorcode}: {errormessage}");

						throw new TaskException($"API query failed with unknown error code {errorcode}");
					}
				}
			}
			catch (TaskException)
			{
				throw;
			}
			catch (AggregateException e)
			{
				throw new TaskException("Login API Request failed: " + (e.InnerExceptions.FirstOrDefault()?.Message ?? e.Message), e);
			}
			catch (HttpRequestException e)
			{
				throw new TaskException("Login API Request failed: " + e.Message, e);
			}
			catch (Exception e)
			{
				throw new TaskException("Login API Request failed: " + e.Message, e);
			}

		}

		private static bool AcceptAll(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			SynoCtrlProgram.Logger.WriteDebug($"Accept (unverified) TLS certificate {cert.Subject}");
			return true;
		}
	}
}
