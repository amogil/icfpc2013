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
		public void Train(int size, params string[] operators)
		{
			var trainProblem = api.Train(new TrainRequest(size, operators));
			Console.WriteLine(trainProblem);
			Assert.AreEqual(size, trainProblem.size);
		}

		[Test]
		public void Train_Bonus()
		{
			var trainProblem = api.Train(new TrainRequest(42));
			Console.WriteLine(trainProblem);
			Assert.That(trainProblem.IsBonus, Is.True);
			Assert.That(trainProblem.operators.Contains("fold"), Is.False);
			Assert.That(trainProblem.operators.Contains("tfold"), Is.False);
			Assert.That(trainProblem.operators.Contains("if0"), Is.True);
		}

		[Test]
		public void Status()
		{
			var status = api.GetStatus();
			Console.Out.WriteLine(status);
		}

		[Test]
		public void MyProblems()
		{
			var myProblemsJson = api.MyProblems();
			var myProblems = myProblemsJson
				.OrderBy(t => t.size)
				.Select(t => string.Format("{0}\t{1}\t{2}\t{3}\t{4}", t.id, t.size, t.solved, t.IsBonus ? "Bonus" : string.Empty, string.Join(",", t.OperatorsExceptBonus)))
				.ToList();
			File.WriteAllLines("../../../../problems.txt", myProblems);
			Console.Out.WriteLine("TotalCount: {0}, Solved: {1}, Failed: {2}", myProblems.Count, myProblemsJson.Count(t => t.solved == true), myProblemsJson.Count(t => t.solved == false));
		}

		private static readonly WebApi api = new WebApi();
	}
}