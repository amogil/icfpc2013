using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using lib.Web;
using log4net;

namespace ProblemsMiner
{
    internal class ProblemsMiner
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (Program));
        private readonly string _problemsFilename;
        private readonly string _resultsDir;

        public ProblemsMiner(string problemsFilename, string resultsDir)
        {
            _problemsFilename = problemsFilename;
            _resultsDir = resultsDir;
        }

        public void DownloadProblemsSamples(int passes)
        {
            var webApi = new WebApi();
            Problem[] problems = ReadProblems(_problemsFilename);
            for (int i = 0; i < passes; i++)
            {
                DownloadProblems(_resultsDir, problems, webApi);
            }
        }

        private static void DownloadProblems(string saveBasePath, IEnumerable<Problem> problems, WebApi webApi)
        {
            foreach (Problem problem in problems.Where(p => p.Size > 3).Shuffle())
            {
                var trainRequest = new TrainRequest(problem.Size, problem.FoldOperators);
                TrainProblem trainProblem = webApi.GetTrainProblem(trainRequest);
                File.AppendAllText(TrainProblemFilename(saveBasePath, trainProblem, problem),
                                   trainProblem + Environment.NewLine);
            }
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

        private static void Log(string text, params object[] values)
        {
            log.DebugFormat(text, values);
        }
    }
}