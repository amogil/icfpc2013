using System;
using System.Collections.Generic;
using System.Linq;
using lib.Brute;
using lib.Lang;
using lib.Web;
using log4net;

namespace lib.AlphaProtocol
{
    public class AlphaProtocol
    {
        private static readonly ILog log = LogManager.GetLogger(typeof (AlphaProtocol));

        public static ulong[] Eval(string problemId, ulong[] inputs)
        {
            var evalRequest = new EvalRequest
                {
                    id = problemId,
                    arguments = inputs.Select(ui => ui.ToString("X")).ToArray()
                };
            var webApi = new WebApi();

            EvalResponse evalResponse = webApi.Eval(evalRequest);
            if (evalResponse.status != "ok")
                throw new ApplicationException("Error EvalResponse");

            return evalResponse.outputs.Select(str => Convert.ToUInt64(str, 16)).ToArray();
        }

        public static Tuple<ulong, ulong> CheckGuess(string problemId, string formuala)
        {
            var guessRequest = new GuessRequest {id = problemId, program = formuala};
            var webApi = new WebApi();

            GuessResponse guessResponse = webApi.Guess(guessRequest);

            if (guessResponse.status == "win")
                return null;
            if (guessResponse.status == "mismatch")
            {
                ulong input = Convert.ToUInt64(guessResponse.values[0], 16);
                ulong output = Convert.ToUInt64(guessResponse.values[1], 16);
                return Tuple.Create(input, output);
            }
            throw new ApplicationException("Error GuessResponse");
        }

        public static void PostSolution(string problemId, int size, string[] operations, bool renameTFoldToFold = false)
        {
            log.DebugFormat("Trying to solve problem {0}...", problemId);
            var random = new Random();
            if (renameTFoldToFold)
                operations = operations.Select(o => o == "tfold" ? "fold" : o).ToArray();
            var trees = new BinaryBruteForcer(operations).Enumerate(size - 1);
            ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();

            log.Debug("Trees and samples generated");

            ulong[] outputs = Eval(problemId, inputs);

            log.Debug("Eval result for samples received");

            byte[][] solutions = Guesser.Guesser.Guess(trees, inputs, outputs).ToArray();

            log.DebugFormat("Solutions generated. Total {0}", solutions.Length);

            while (true)
            {
                byte[] solution = solutions.First();

                log.Debug("Asking the first guess");

                string formula = String.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
                Tuple<ulong, ulong> result = CheckGuess(problemId, formula);

                log.Debug("Guess answer received");

                if (result == null)
                {
                    log.DebugFormat("Problem solved!!!. Problem Id: {0}", problemId);
                    return;
                }

                log.Debug("New case received");

                Tuple<ulong, ulong> anCase = result;

                inputs = inputs.Concat(new[] {anCase.Item1}).ToArray();
                outputs = outputs.Concat(new[] {anCase.Item2}).ToArray();

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