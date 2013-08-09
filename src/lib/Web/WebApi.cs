﻿using System;
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
		private Status lastStatus;
		private DateTime lastStatusTime;

		public static string GetUrl(string command)
		{
			return string.Format("http://icfpc2013.cloudapp.net/{0}?auth={1}", command, AuthKey);
		}

		public string GetString(string command, object arg)
		{
			string body = new JavaScriptSerializer().Serialize(arg);
			log.Debug("REQUEST " + command + " " + body);
			string answer = Encoding.ASCII.GetString(GetResponse(GetUrl(command), Encoding.ASCII.GetBytes(body)));
			log.Debug("RESPONSE " + answer);
			return answer;
		}

		public string GetString(string command)
		{
			log.Debug("REQUEST " + command);
			string answer = Encoding.ASCII.GetString(GetResponse(GetUrl(command)));
			Console.WriteLine(answer);
			log.Debug("RESPONSE " + answer);
			return answer;
		}

		public TrainProblem GetTrainProblem(TrainRequest request)
		{
			return Call<TrainRequest, TrainProblem>("train", request);
		}

		public EvalResponse Eval(EvalRequest request)
		{
			return Call<EvalRequest, EvalResponse>("eval", request);
		}
		
		public TOut Call<TIn, TOut>(string command, TIn request)
		{
			return json.Deserialize<TOut>(GetString(command, request));
		}

		public Status GetStatus()
		{
			byte[] downloadData = new WebClient().DownloadData(GetUrl("status"));
			return json.Deserialize<Status>(Encoding.ASCII.GetString(downloadData));
		}

		private byte[] GetResponse(string address, byte[] bytes = null)
		{
			try
			{
				if (lastStatus == null)
				{
					lastStatus = UpdateLastStatus();
					log.Debug(lastStatus.requestWindow);
				}
				lastStatus.requestWindow.amount++;
				if (lastStatus.requestWindow.amount >= lastStatus.requestWindow.limit)
				{
					double wait = Math.Max(0, lastStatus.requestWindow.resetsIn - (DateTime.Now - lastStatusTime).TotalSeconds);
					if (wait > 0)
					{
						log.Debug(lastStatus.requestWindow);
						log.Debug("Waiting " + wait);
						Thread.Sleep((int) (wait*1000));
					}
					lastStatus = null;
				}
				if (bytes != null)
					return new WebClient().UploadData(address, bytes);
				else
					return new WebClient().DownloadData(address);
			}
			catch (WebException e)
			{
				log.Error("ERROR: " + e.Status);
				throw;
			}
		}

		private Status UpdateLastStatus()
		{
			lastStatusTime = DateTime.Now;
			return GetStatus();
		}
	}
}