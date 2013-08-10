using System;
using System.Collections.Generic;
using System.Linq;
using lib.Brute;
using lib.Lang;
using log4net;

namespace lib.AlphaProtocol
{
    public class AlphaProtocol
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (AlphaProtocol));

        public static string PostSolution(string problemId, int size, string[] operations, bool renameTFoldToFold)
        {
            var gsc = new GameServerClient();

            log.DebugFormat("Trying to solve problem {0}...", problemId);
            var random = new Random();
            if (renameTFoldToFold)
                operations = operations.Select(o => o == "tfold" ? "fold" : o).ToArray();
            IEnumerable<byte[]> trees = new BinaryBruteForcer(operations).Enumerate(size - 1);
            ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();

            ulong[] outputs = gsc.Eval(problemId, inputs);

            log.Debug("Eval result for samples received");

            IEnumerable<byte[]> solutions = Guesser.Guesser.Guess(trees, inputs, outputs);

            while (true)
            {
                log.Debug("Start find solution");
                byte[] solution = solutions.First();
                log.Debug("First solution finded, asking the guess...");

                string formula = String.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
                WrongAnswer wrongAnswer = gsc.Guess(problemId, formula);

                log.Debug("Guess answer received");

                if (wrongAnswer == null)
                {
                    log.DebugFormat("Problem solved!!!. Problem Id: {0}", problemId);
                    return formula;
                }

                log.Debug(string.Format("WrongAnswer received: {0}", wrongAnswer));

                solutions = AddCase(solutions, wrongAnswer);
            }
        }

        public static IEnumerable<byte[]> AddCase(IEnumerable<byte[]> trees, WrongAnswer answer)
        {
            return trees.Where(tree => tree.Eval(answer.Arg) == answer.CorrectValue);
        }
    }
}