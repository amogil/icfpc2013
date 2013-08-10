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
        public ProblemsReader Source;
	    private WebApi webApi;

        public ProblemsMiner(ProblemsReader source)
        {
            this.Source = source;
        }

        public void DownloadTrainProblemsSamples(int passes)
        {
            webApi = new WebApi();
            Problem[] problems = Source.ReadProblems();
            for (int i = 0; i < passes; i++)
            {
                DownloadTrainProblems(problems);
            }
        }

        private void DownloadTrainProblems(IEnumerable<Problem> problems)
        {
            foreach (Problem problem in problems.Where(p => p.Size > 3).Shuffle())
            {
                var trainRequest = new TrainRequest(problem.Size, problem.FoldOperators);
                TrainProblem trainProblem = webApi.GetTrainProblem(trainRequest);
                Source.SaveTrainProblem(trainProblem, problem);
            }
        }

        public void DownloadTrainProblemsResults(bool onlyRelevant, UInt64[] arguments)
        {
            Problem[] problems = Source.ReadProblems();
            DownloadTrainProblemsResults(problems, onlyRelevant, arguments);
        }

		public void DownloadTrainProblemsResults(Problem[] problems, bool onlyRelevant, UInt64[] arguments)
		{
			webApi = new WebApi();
			for (int i = 0; i < problems.Length; ++i)
			{
				var problem = problems[i];

                TrainProblem[] trainProblems = Source.ReadTrainProblems(problem, true);
                TrainProblem[] unrelevantTrainProblems = new TrainProblem[0];
				if (!onlyRelevant)
					unrelevantTrainProblems = Source.ReadTrainProblems(problem, false);

				foreach (var trainProblem in trainProblems.Union(unrelevantTrainProblems))
				{
				    var currentResults = Source.ReadResultsForTrainProblem(trainProblem, problem);
				    if (arguments.Any(arg => !currentResults.ContainsKey(arg)))
				    {
				        for (int k = 0; k < arguments.Length/255; ++k)
				        {
				            var args = arguments.Skip(k*255).Take(255).ToArray();
				            var results = DownloadTrainProblemResult(trainProblem, args);
				            Source.SaveResultsForTrainProblem(trainProblem, problem, results);
				        }
				    }
				}
			}
		}

        private IDictionary<UInt64, UInt64> DownloadTrainProblemResult(TrainProblem trainProblem, UInt64[] arguments)
		{
			EvalRequest request = new EvalRequest()
				{
					program = trainProblem.challenge,
					arguments = arguments.Select(i => i.ToString("X")).ToArray()
				};
			var response = webApi.Eval(request);
            Dictionary<UInt64, UInt64> results = new Dictionary<ulong, ulong>();
			if (response.outputs.Length != arguments.Length)
				return null;
			for (int i = 0; i < response.outputs.Length; ++i)
			{
                UInt64 result = Convert.ToUInt64(response.outputs[i], 16);
				results[arguments[i]] = result;
			}
			return results;
		}

        /// <summary>
        /// Загружает результаты вычислений для данной НЕТЕСТОВОЙ проблемы
        /// сохраняет их в Source
        /// </summary>
        public IDictionary<UInt64, UInt64> DownloadProblemResult(Problem problem, UInt64[] arguments)
        {
            EvalRequest request = new EvalRequest()
            {
                id = problem.Id,
                arguments = arguments.Select(i => i.ToString("X")).ToArray()
            };
            var response = webApi.Eval(request);
            Dictionary<UInt64, UInt64> results = new Dictionary<ulong, ulong>();
			if (response.outputs.Length != arguments.Length)
				return null;
			for (int i = 0; i < response.outputs.Length; ++i)
			{
                UInt64 result = Convert.ToUInt64(response.outputs[i], 16);
				results[arguments[i]] = result;
			}
            Source.SaveResultsForProblem(problem, results);
			return results;
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
		[Explicit]
		public void DownloadTrainsProblemsTest()
		{
            HashSet<UInt64> arguments = new HashSet<UInt64>();
            arguments.Add(UInt64.MaxValue);
            arguments.Add(UInt64.MinValue);
			for (long i=-1; i<=100; ++i)
				arguments.Add((ulong)i);
			Random rnd = new Random(Environment.TickCount);
			while (arguments.Count < 255)
				arguments.Add((((ulong)rnd.Next())<<32) + ((ulong)rnd.Next()));

            ProblemsReader source = new ProblemsReader()
            {
                ProblemsFilename = @"..\..\..\..\problems.txt",
                ProblemsResultsPath = @"..\..\..\..\problems-results\",
                TrainProblemsPath = @"..\..\..\..\problems-samples\",
                TrainProblemsResultsPath = @"..\..\..\..\problems-samples\"
            };
			ProblemsMiner miner = new ProblemsMiner(source);
			//miner.DownloadTrainProblemsResults(true, arguments.ToArray());
            miner.DownloadTrainProblemsResults(source.ReadProblems().Where(p => p.Size > 12).ToArray(), true, arguments.ToArray());

		}
	}
}