using System;
using NUnit.Framework;

namespace lib.Web
{
	[TestFixture]
	public class WebApi_Test
	{
		[TestCase(3)]
		[TestCase(4)]
		[TestCase(5)]
		[TestCase(6)]
		[TestCase(8)]
		[TestCase(9)]
		[TestCase(10)]
		[TestCase(10, "tfold")]
		[TestCase(12, "fold")]
		[Test]
		[Explicit]
		public void GetTrain(int size, params string[] operators)
		{
			TrainProblem trainProblem = api.GetTrainProblem(new TrainRequest(size, operators));
			Console.WriteLine(trainProblem);
			Assert.AreEqual(size, trainProblem.size);
		}

		[Test]
		[Explicit]
		public void Status()
		{
			Status status = api.GetStatus();
			Console.WriteLine("CpuTotalTime: {0}", status.cpuTotalTime);
			Console.WriteLine("NumReq: {0}", status.numRequests);
			Console.WriteLine("Req: {0}", status.requestWindow);
			Console.WriteLine("Cpu: {0}", status.cpuWindow);
		}

		private static readonly WebApi api = new WebApi();
	}
}