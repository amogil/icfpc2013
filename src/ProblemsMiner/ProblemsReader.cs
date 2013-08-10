﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib.Web;
using log4net;

namespace ProblemsMiner
{
    class ProblemsReader
    {
        public static ILog log;

        public string ProblemsFilename;
        public string ProblemsResultsPath;
        public string TrainProblemsPath;
        public string TrainProblemsResultsPath;

        public string TrainProblemResultFilename(TrainProblem trainProblem, Problem problem)
        {
            bool isRelevant = trainProblem.operators.OrderBy(t => t).SequenceEqual(problem.AllOperators.OrderBy(t => t));
            string filename = String.Format(isRelevant ? "{0}_{1}.result" : "{0}_{1}.unrelevant.result", problem.Id, trainProblem.id);
            return Path.Combine(TrainProblemsResultsPath, filename);
        }

        public string TrainProblemFilename(TrainProblem trainProblem, Problem problem)
        {
            bool isRelevant = trainProblem.operators.OrderBy(t => t).SequenceEqual(problem.AllOperators.OrderBy(t => t));
            string filename = String.Format(isRelevant ? "{0}.txt" : "{0}.unrelevant.txt", problem.Id);
            return Path.Combine(TrainProblemsPath, filename);
        }

        public string ProblemResultFilename(Problem problem)
        {
            string filename = String.Format("{0}.result", problem.Id);
            return Path.Combine(ProblemsResultsPath, filename);
        }

        public Problem[] ReadProblems()
        {
            string path = ProblemsFilename;
            string[] problemsText = File.ReadAllLines(path);
            Problem[] problems = problemsText.Select(Problem.Parse).Where(p => p != null).ToArray();
            Log("Readed file '{0}'. {1} problems loaded.", path, problems.Count());
            return problems;
        }

        public TrainProblem[] ReadTrainProblems(Problem associatedProblem, bool isRelevant)
        {
            try
            {
                string relevant = Path.Combine(TrainProblemsPath, String.Format("{0}.txt", associatedProblem.Id));
                string unrelevant = Path.Combine(TrainProblemsPath, String.Format("{0}.unrelevant.txt", associatedProblem.Id));
                string path = isRelevant ? relevant : unrelevant;
                string[] problemsText = File.ReadAllLines(path);
                TrainProblem[] trainProblems = problemsText.Select(TrainProblem.Parse).Where(p => p != null).ToArray();
                Log("Readed file '{0}'. {1} train problems loaded.", path, trainProblems.Count());
                return trainProblems;
            }
            catch (Exception)
            {
                return new TrainProblem[0];
            }
        }

        public Dictionary<UInt64, UInt64> ReadResultsForProblem(Problem problem)
        {
            string path = ProblemResultFilename(problem);
            return ReadResults(path);
        }

        public Dictionary<UInt64, UInt64> ReadResultsForTrainProblem(TrainProblem trainProblem, Problem problem)
        {
            string path = TrainProblemResultFilename(trainProblem, problem);
            return ReadResults(path);
        }

        public void SaveResultsForProblem(Problem problem, Dictionary<UInt64, UInt64> results, bool rewrite = false)
        {
            string path = ProblemResultFilename(problem);
            SaveResults(path, results, rewrite);
        }

        public void SaveResultsForTrainProblem(TrainProblem trainProblem, Problem problem, IDictionary<UInt64, UInt64> results, bool rewrite =false)
        {
            string path = TrainProblemResultFilename(trainProblem, problem);
            SaveResults(path, results, rewrite);
        }

        public void SaveTrainProblem(TrainProblem trainProblem, Problem problem)
        {
            File.AppendAllText(TrainProblemFilename(trainProblem, problem),
                                   trainProblem + Environment.NewLine);
        }

        private Dictionary<UInt64, UInt64> ReadResults(string path)
        {
            try
            {
                Dictionary<UInt64, UInt64> res = new Dictionary<ulong, ulong>();
                var results = File.ReadAllLines(path);
                foreach (var result in results)
                {
                    try
                    {
                        var xy = result.Split('\t').Select(i => i.Trim()).ToArray();
                        var x = UInt64.Parse(xy[0]);
                        var y = UInt64.Parse(xy[1]);
                        res[x] = y;
                    }
                    catch (Exception ex)
                    {
                        Log("Cant parse result " + result + " in file " + path);
                    }

                }
                return res;
            }
            catch (Exception ex)
            {
                Log("Can't read results from file " + path + ". " + ex.Message);
                return new Dictionary<ulong, ulong>();
            }
        }

        private void SaveResults(string path, IDictionary<UInt64, UInt64> results, bool rewrite)
        {
            try
            {
                Dictionary<UInt64, UInt64> currentResults = rewrite
                    ? new Dictionary<UInt64, UInt64>()
                    : ReadResults(path);
                foreach (var result in results)
                    currentResults[result.Key] = result.Value;
                List<string> res = new List<string>();
                foreach (var xy in currentResults)
                    res.Add("" + xy.Key + "\t" + xy.Value + Environment.NewLine);
                File.WriteAllLines(path, res);
            }
            catch (Exception ex)
            {
                Log("Can't save results in file " + path + ". " + ex.Message);
            }

        }

        private static void Log(string text, params object[] values)
        {
            if (log != null)
                log.DebugFormat(text, values);
        }
    }
}