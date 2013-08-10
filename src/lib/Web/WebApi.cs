using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Web.Script.Serialization;
using log4net;

namespace lib.Web
{
    public class WebApi
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (WebApi));
        public static string AuthKey = "0071PimxQKpGJdtDE76gsjAoaOagBVX3tdGOfCQH" + "vpsH1H";
        private readonly JavaScriptSerializer json = new JavaScriptSerializer();

        public static string GetUrl(string command)
        {
            return string.Format("http://icfpc2013.cloudapp.net/{0}?auth={1}", command, AuthKey);
        }

        public string GetString(string command, object arg)
        {
            string body = new JavaScriptSerializer().Serialize(arg);
//            log.Debug("REQUEST " + command + " " + body);
            string answer = Encoding.ASCII.GetString(GetResponse(GetUrl(command), Encoding.ASCII.GetBytes(body)));
//            log.Debug("RESPONSE " + answer);
            return answer;
        }

        public string GetString(string command)
        {
//            log.Debug("REQUEST " + command);
            string answer = Encoding.ASCII.GetString(GetResponse(GetUrl(command)));
            Console.WriteLine(answer);
//            log.Debug("RESPONSE " + answer);
            return answer;
        }

		public List<MyProblemJson> MyProblems()
		{
			return json.Deserialize<List<MyProblemJson>>(GetString("myproblems"));
		}

		public TrainResponse Train(TrainRequest request)
        {
			return Call<TrainRequest, TrainResponse>("train", request);
        }

        public EvalResponse Eval(EvalRequest request)
        {
            return Call<EvalRequest, EvalResponse>("eval", request);
        }

		public GuessResponse Guess(GuessRequest request)
		{
			return Call<GuessRequest, GuessResponse>("guess", request);
		}

		private TOut Call<TIn, TOut>(string command, TIn request)
        {
            return json.Deserialize<TOut>(GetString(command, request));
        }

        public Status GetStatus()
        {
			var downloadData = new WebClient().DownloadData(GetUrl("status"));
            return json.Deserialize<Status>(Encoding.ASCII.GetString(downloadData));
        }

		private static byte[] GetResponse(string address, byte[] bytes = null)
        {
            while (true)
            {
                try
                {
                    if (bytes != null)
                        return new WebClient().UploadData(address, bytes);
                    return new WebClient().DownloadData(address);
                }
                catch (WebException e)
                {
                    if (e.Status == WebExceptionStatus.ConnectFailure)
                    {
                        Thread.Sleep(5000);
                        continue;
                    }
                    var errorResponse = (HttpWebResponse) e.Response;
                    if ((int) errorResponse.StatusCode == 429)
                    {
                        log.Debug("Got 429 Status. Waiting for a second...");
                        Thread.Sleep(1000);
                        continue;
                    }
                    throw;
                }
            }
        }
    }
}