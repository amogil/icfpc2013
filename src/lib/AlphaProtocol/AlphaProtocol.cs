using System;
using System.Collections.Generic;
using System.Globalization;
using lib.Brute;
using lib.Web;
using System.Linq;

namespace lib.AlphaProtocol
{
    internal class AlphaProtocol
    {
        public static IEnumerable<ulong> GetSecretProgramAnswers(string problemId, IEnumerable<ulong> inputs)
        {
			if (problemId != "rMwYy8UTFc9pCrJQuwuANkl6")
				return null;
			var problemSize = 8;
	        var problemOperators = new[] {"tfold", "xor"};
	        var problemBody = "(lambda (x_2637) (fold x_2637 0 (lambda (x_2637 x_2638) (xor x_2638 x_2637))))";

//	        var random = new Random(1234567890);
//	        var binaryBruteForcer = new BinaryBruteForcer();
//	        var inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64());
			var evalRequest = new EvalRequest { id = problemId, program = problemBody, arguments = inputs.Select(ui => ui.ToString("X")).ToArray() };
	        var webApi = new WebApi();

	        var evalResponse = webApi.Eval(evalRequest);
	        var outputs = evalResponse.outputs.Select(str => UInt64.Parse(str, NumberStyles.AllowHexSpecifier));
	        return outputs;
        }

		public static GuessResponse CheckGuess(string problemId, string guess)
		{
			if (problemId != "rMwYy8UTFc9pCrJQuwuANkl6")
				return null;

			var guessRequest = new GuessRequest {id = problemId, programm = guess};
			var webApi = new WebApi();

			var guessResponse = webApi.Guess(guessRequest);
			return guessResponse;
		}
    }
}