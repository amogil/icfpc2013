using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using lib.Lang;
using lib.Web;
using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;

namespace ProblemsMiner
{

    class EvalTester
    {
        public static ILog log;
        public ProblemsReader Source;
        public List<TestResult> Tests = new List<TestResult>(); 

        public EvalTester(ProblemsReader source)
        {
            Source = source;
        }

        public bool Test(TrainResponse trainProblem, Problem problem)
        {
            var results = Source.ReadResultsForTrainProblem(trainProblem, problem);
            var test = new TestResult()
                    {
                        problem = problem,
                        trainProblem = trainProblem,
                        isSuccess = results.Count > 0 ? 1 : 0
                    };

            try
            {

                var expr = Expr.ParseFunction(trainProblem.challenge);

                foreach (var xy in results)
                {
                    var x = xy.Key;
                    var y = xy.Value;
                    var real = expr.Eval(new Vars(x));
                    if (real != y)
                    {
                        test.isSuccess = -1;
                        test.x_expected_real.Add(new Tuple<ulong, ulong, ulong>(x, y, real));
                    }
                }
                
            }
            catch (Exception ex)
            {
                test.isSuccess = -1;
                test.exeption = ex.Message;
            }
            Tests.Add(test);
            return test.isSuccess >= 0;
        }

        public void Test(Problem[] problems)
        {
            foreach (var problem in problems)
                foreach ( var trainProblem in
                        Source.ReadTrainProblems(problem, true).Union(Source.ReadTrainProblems(problem, false)))
                    Test(trainProblem, problem);
        }

        public class TestResult
        {
            public Problem problem;
            public TrainResponse trainProblem;
            public int isSuccess;
            public List<Tuple<UInt64, UInt64, UInt64>> x_expected_real  = new List<Tuple<ulong, ulong, ulong>>();
            public string exeption = "";

            public override string ToString()
            {
                string str = "";
                str += "problemId:\t" + problem.Id+ Environment.NewLine;
                str += "trainId:\t"+ trainProblem.id+Environment.NewLine;
                str += "challenge:\t" + trainProblem.ToString()+Environment.NewLine;
                str += "result:\t" + (isSuccess>0? "success" : isSuccess<0? "fail" : "unknown");
                str += "errors: [x                 expected           real]" + Environment.NewLine;
                foreach (var tuple in x_expected_real)
                    str += String.Format("{0}\t{1}\t{2}", tuple.Item1, tuple.Item2, tuple.Item3);

                if (!String.IsNullOrWhiteSpace(exeption))
                    str += exeption + Environment.NewLine;
                return str;
            }
        }
    }

    [TestFixture]
    public class EvalTesting
    {
        [Test]
        public void Test()
        {
            ProblemsReader source = new ProblemsReader()
            {
                ProblemsFilename = @"..\..\..\..\problems.txt",
                ProblemsResultsPath = @"..\..\..\..\problems-results\",
                TrainProblemsPath = @"..\..\..\..\problems-samples\",
                TrainProblemsResultsPath = @"..\..\..\..\problems-samples\"
            };
            
            EvalTester tester = new EvalTester(source);
            tester.Test(source.ReadProblems().ToArray());

            Dictionary<string, statistic> statistics = new Dictionary<string, statistic>();
            statistics["all"] = new statistic();
            foreach (var testResult in tester.Tests)
            {
                foreach (var op in testResult.trainProblem.operators)
                {
                    if (!statistics.ContainsKey(op))
                        statistics[op] = new statistic();
                    if (testResult.isSuccess > 0)
                        statistics[op].success++;
                    else if (testResult.isSuccess < 0)
                        statistics[op].fail++;
                    else
                        statistics[op].unknown++;
                }
                if (testResult.isSuccess > 0)
                    statistics["all"].success++;
                else if (testResult.isSuccess < 0)
                    statistics["all"].fail++;
                else
                    statistics["all"].unknown++;
            }

            foreach (var stat in statistics)
                Console.WriteLine(String.Format("{0} sucess:{1}  fail:{2}  unknown: {3}", stat.Key, stat.Value.success, stat.Value.fail, stat.Value.unknown));

            foreach (var testResult in tester.Tests)
                Console.WriteLine(testResult);





        }


        protected class statistic
        {
            public int success, unknown, fail;
        }

    }

}
