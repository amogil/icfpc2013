using System;
using System.Collections.Generic;
using System.Linq;
using lib.Brute;
using lib.Lang;
using lib.Web;

namespace lib.AlphaProtocol
{
    public class AlphaProtocol
    {
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
                ulong input = Convert.ToUInt64(guessResponse.values.First(), 16);
                ulong output = Convert.ToUInt64(guessResponse.values.ElementAt(1), 16);
                return Tuple.Create(input, output);
            }
            throw new ApplicationException("Error GuessResponse");
        }

        public static void PostSolution(string problemId, int size, string[] operations)
        {
            var random = new Random();
            byte[][] trees = new BinaryBruteForcer(operations).Enumerate(size - 1).ToArray();
            ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();

            ulong[] outputs = Eval(problemId, inputs);

            byte[][] solutions = Guesser.Guesser.Guess(trees, inputs, outputs).ToArray();
            while (true)
            {
                byte[] solution = solutions.First();

                string formula = String.Format("(lambda (x) {0})", solution.ToSExpr().Item1);
                Tuple<ulong, ulong> result = CheckGuess(problemId, formula);

                if (result == null)
                    return; // WIN!!!!!!!!!

                Tuple<ulong, ulong> anCase = result;

                inputs = inputs.Concat(new[] {anCase.Item1}).ToArray(); // ACHTUNG!!!!
                outputs = outputs.Concat(new[] {anCase.Item2}).ToArray();
                solutions = Guesser.Guesser.Guess(solutions, inputs, outputs).ToArray();
            }
        }

        public static IEnumerable<byte[]> AddCase(IEnumerable<byte[]> trees, Tuple<ulong, ulong> anCase)
        {
            return trees.Where(tree => tree.Eval(anCase.Item1) == anCase.Item2);
        }
    }
}