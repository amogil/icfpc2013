using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace lib.Web
{
	[TestFixture]
	[Explicit]
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

		[Test]
		public void GetMyProblems()
		{
			var myProblemsJson = api.GetMyProblems();
			var myProblems = myProblemsJson
				.OrderBy(t => t.size)
				.Select(t => string.Format("{0}\t{1}\t{2}\t{3}\t{4}", t.id, t.size, t.solved, t.IsBonus ? "Bonus" : string.Empty, string.Join(",", t.OperatorsExceptBonus)))
				.ToList();
			File.WriteAllLines("../../../../problems.txt", myProblems);
			Console.Out.WriteLine("{0}", myProblems.Count);
		}

		private static readonly WebApi api = new WebApi();
	}
}