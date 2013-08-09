using System;
using System.Threading;
using NUnit.Framework;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;

namespace lib.Web
{
	[TestFixture]
	public class WebApi_Test
	{
		readonly static WebApi api = new WebApi();
		private static readonly ILog log = LogManager.GetLogger(typeof (WebApi_Test));

		[TestCase(3)]
		[TestCase(4)]
		[TestCase(5)]
		[TestCase(6)]
		[TestCase(8)]
		[TestCase(9)]
		[TestCase(10)]
		[Test]
		public void GetTrain(int size, params string[] operators)
		{
			var trainProblem = api.GetTrainProblem(new TrainRequest(size, operators));
			Console.WriteLine(trainProblem);
			Assert.AreEqual(size, trainProblem.size);
		}

		[Test]
		public void Status()
		{
			var status = api.GetStatus();
			Console.WriteLine("CpuTotalTime: {0}", status.cpuTotalTime);
			Console.WriteLine("NumReq: {0}", status.numRequests);
			Console.WriteLine("Req: {0}", status.requestWindow);
			Console.WriteLine("Cpu: {0}", status.cpuWindow);
		}
	}
}