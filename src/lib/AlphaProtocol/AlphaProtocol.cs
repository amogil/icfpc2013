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

        public static void PostSolution(string problemId, int size, string[] operations, bool renameTFoldToFold = false)
        {
			var gsc = new GameServerClient();

            log.DebugFormat("Trying to solve problem {0}...", problemId);
            var random = new Random();
            if (renameTFoldToFold)
                operations = operations.Select(o => o == "tfold" ? "fold" : o).ToArray();
            var trees = new BinaryBruteForcer(operations).Enumerate(size - 1);
            ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();

            log.Debug("Trees and samples generated");

			ulong[] outputs = gsc.Eval(problemId, inputs);

            log.Debug("Eval result for samples received");

            byte[][] solutions = Guesser.Guesser.Guess(trees, inputs, outputs).ToArray();

            log.DebugFormat("Solutions generated. Total {0}", solutions.Length);

            while (true)
            {
                byte[] solution = solutions.First();

                log.Debug("Asking the first guess");

				var formula = String.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
				var wrongAnswer = gsc.Guess(problemId, formula);

                log.Debug("Guess answer received");

				if (wrongAnswer == null)
                {
                    log.DebugFormat("Problem solved!!!. Problem Id: {0}", problemId);
                    return;
                }

				log.Debug(string.Format("WrongAnswer received: {0}", wrongAnswer));

				inputs = inputs.Concat(new[] { wrongAnswer.Arg }).ToArray();
				outputs = outputs.Concat(new[] { wrongAnswer.CorrectValue }).ToArray();

                solutions = Guesser.Guesser.Guess(solutions, inputs, outputs).ToArray();
                log.DebugFormat("Solutions generated. Total {0}", solutions.Length);
            }
        }

        public static IEnumerable<byte[]> AddCase(IEnumerable<byte[]> trees, Tuple<ulong, ulong> anCase)
        {
            return trees.Where(tree => tree.Eval(anCase.Item1) == anCase.Item2);
        }
    }
}