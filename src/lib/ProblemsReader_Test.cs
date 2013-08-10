using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using lib.Lang;
using lib.Web;

namespace lib
{
	[TestFixture]
	public class ProblemsReader_Test
	{
		[Test]
		public void Test()
		{
			for (int i = 3; i < 15; i++)
			{
				Console.WriteLine("SIZE: " + i);
				var ps = GetTrainingProblemsWithAnswer(i);
				foreach (var p in ps)
				{
					Console.WriteLine(p);
				}
			}
		}

		public IEnumerable<Tuple<TrainResponse, Mask>> GetTrainingProblemsWithAnswer(int size)
		{
			var source = new ProblemsReader()
				{
					ProblemsFilename = @"..\..\..\..\problems.txt",
					ProblemsResultsPath = @"..\..\..\..\problems-results\",
					TrainProblemsPath = @"..\..\..\..\problems-samples\",
					TrainProblemsResultsPath = @"..\..\..\..\problems-samples\"
				};
			var realProblems = source.ReadProblems().Where(p => p.AllOperators.All(op => /*!op.EndsWith("fold") && */p.Size == size)).ToArray();

			foreach (var realP in realProblems)
			{
				IEnumerable<TrainResponse> trainingProblems = source.ReadTrainProblems(realP, false).Concat(source.ReadTrainProblems(realP, true));
				foreach (var p in trainingProblems)
				{
					ulong[] ulongs = source.ReadResultsForTrainProblem(p, realP).Select(kv => kv.Value).ToArray();
					if (ulongs.Length > 0) yield return Tuple.Create(p, new Mask(ulongs));
				}
			}
		}
	}
}