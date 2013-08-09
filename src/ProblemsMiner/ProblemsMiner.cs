using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using lib.Web;
using log4net;

namespace ProblemsMiner
{
    internal class ProblemsMiner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Program));
        private readonly string _problemsFilename;
        private readonly string _resultsDir;
	    private WebApi webApi;

        public ProblemsMiner(string problemsFilename, string resultsDir)
        {
            _problemsFilename = problemsFilename;
            _resultsDir = resultsDir;
        }

        public void DownloadTrainProblemsSamples(int passes)
        {
            webApi = new WebApi();
            Problem[] problems = ReadProblems(_problemsFilename);
            for (int i = 0; i < passes; i++)
            {
                DownloadTrainProblems(_resultsDir, problems);
            }
        }

        private void DownloadTrainProblems(string saveBasePath, IEnumerable<Problem> problems)
        {
            foreach (Problem problem in problems.Where(p => p.Size > 3).Shuffle())
            {
                var trainRequest = new TrainRequest(problem.Size, problem.FoldOperators);
                TrainProblem trainProblem = webApi.GetTrainProblem(trainRequest);
                File.AppendAllText(TrainProblemFilename(saveBasePath, trainProblem, problem),
                                   trainProblem + Environment.NewLine);
            }
        }

		public void DownloadTrainProblemsResults(bool onlyRelevant, Int64[] arguments)
		{
			webApi = new WebApi();
			Problem[] problems = ReadProblems(_problemsFilename);
			for (int i = 0; i < problems.Length; ++i)
			{
				var problem = problems[i];
				string relevant = Path.Combine(_resultsDir, String.Format("{0}.txt", problem.Id));
				string unrelevant = Path.Combine(_resultsDir,String.Format("{0}.unrelevant.txt", problems[i].Id));
				TrainProblem[] trainProblems = ReadTrainProblems(relevant);
				TrainProblem[] unrelevantTrainProblems = ReadTrainProblems(unrelevant);
				if (!onlyRelevant)
					unrelevantTrainProblems = ReadTrainProblems(unrelevant);

				foreach (var trainProblem in trainProblems.Union(unrelevantTrainProblems))
				{
					string savepath = TrainProblemResultFilename(_resultsDir, trainProblem, problem);
					for (int k = 0; k < arguments.Length/255; ++k)
					{
						var args = arguments.Skip(k*255).Take(255).ToArray();
						var results = DownloadTrainProblemResult(trainProblem, args);
						SaveTrainProblemResult(savepath, results);
					}
				}
			}
		}

		

		private IDictionary<Int64, Int64> DownloadTrainProblemResult(TrainProblem trainProblem, Int64[] arguments)
		{
			EvalRequest request = new EvalRequest()
				{
					program = trainProblem.challenge,
					arguments = arguments.Select(i => i.ToString("X")).ToArray()
				};
			var response = webApi.Eval(request);
			Dictionary<Int64, Int64> results = new Dictionary<long, long>();
			if (response.outputs.Length != arguments.Length)
				return null;
			for (int i = 0; i < response.outputs.Length; ++i)
			{
				Int64 result = Convert.ToInt64(response.outputs[i], 16);
				results[arguments[i]] = result;
			}
			return results;
		}

		private void SaveTrainProblemResult(string savePath, IDictionary<Int64, Int64> result)
		{
			
			string res = "";
			foreach (var xy in result)
				res += "" + xy.Key + "\t" + xy.Value + "\n";

			File.AppendAllText(savePath, res);
		}

	    public static string TrainProblemResultFilename(string basePath, TrainProblem trainProblem, Problem problem)
		{
			bool isRelevant = trainProblem.operators.OrderBy(t => t).SequenceEqual(problem.AllOperators.OrderBy(t => t));
			string filename = String.Format(isRelevant ? "{0}_{1}.result" : "{0}_{1}.unrelevant.result", problem.Id, trainProblem.id);
			return Path.Combine(basePath, filename);
		}

        private static string TrainProblemFilename(string basePath, TrainProblem trainProblem, Problem problem)
        {
            bool isRelevant = trainProblem.operators.OrderBy(t => t).SequenceEqual(problem.AllOperators.OrderBy(t => t));
            string filename = String.Format(isRelevant ? "{0}.txt" : "{0}.unrelevant.txt", problem.Id);
            return Path.Combine(basePath, filename);
        }

		

        private static Problem[] ReadProblems(string path)
        {
            string[] problemsText = File.ReadAllLines(path);
            Problem[] problems = problemsText.Select(Problem.Parse).Where(p => p != null).ToArray();
            Log("Readed file '{0}'. {1} problems loaded.", path, problems.Count());
            return problems;
        }


		private static TrainProblem[] ReadTrainProblems(string path)
		{
			try
			{
				string[] problemsText = File.ReadAllLines(path);
				TrainProblem[] trainProblems = problemsText.Select(TrainProblem.Parse).Where(p => p != null).ToArray();
				return trainProblems;
			}
			catch (Exception)
			{
				return new TrainProblem[0];
			}
			
		}

        private static void Log(string text, params object[] values)
        {
            log.DebugFormat(text, values);
        }
    }



	[TestFixture]
	public class ProblemsMiner_Test
	{
		[Test]
		public void DownloadTrainsProblemsTest()
		{
			HashSet<Int64> arguments = new HashSet<Int64>();
			arguments.Add(Int64.MaxValue);
			arguments.Add(Int64.MinValue);
			for (int i=-1; i<=100; ++i)
				arguments.Add(i);
			Random rnd = new Random(Environment.TickCount);
			while (arguments.Count < 255)
				arguments.Add((((long)rnd.Next())<<32) + ((long)rnd.Next()));


			ProblemsMiner miner = new ProblemsMiner(@"..\..\..\..\problems.txt", @"..\..\..\..\problems-samples\");
			miner.DownloadTrainProblemsResults(true, arguments.ToArray());

		}
	}
}