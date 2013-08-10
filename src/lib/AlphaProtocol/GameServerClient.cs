using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using lib.Web;

namespace lib.AlphaProtocol
{
	public class GameServerClient
	{
		private readonly WebApi webApi;

		public GameServerClient()
		{
			webApi = new WebApi();
		}

		public List<ulong> Eval(string problemId, IEnumerable<ulong> inputs)
		{
			return Eval(new EvalRequest { id = problemId }, inputs);
		}

		public List<ulong> EvalProgram(string program, IEnumerable<ulong> inputs)
		{
			return Eval(new EvalRequest { program = program }, inputs);
		}

		private List<ulong> Eval(EvalRequest request, IEnumerable<ulong> inputs)
		{
			request.arguments = inputs.Select(ui => ui.ToString("X")).ToArray();

			var response = webApi.Eval(request);
			if (!response.status.Equals("ok", StringComparison.OrdinalIgnoreCase))
				throw new ApplicationException(string.Format("Error EvalResponse ({0}): {1}", response.status, response.message));

			var values = response.outputs.Select(str => Convert.ToUInt64(str, 16)).ToList();
			return values;
		}

		[CanBeNull]
		public WrongAnswer Guess(string problemId, string program)
		{
			var request = new GuessRequest
			{
				id = problemId,
				program = program
			};

			var response = webApi.Guess(request);
			if (response.status.Equals("win", StringComparison.OrdinalIgnoreCase))
				return null;

			if (response.status.Equals("mismatch", StringComparison.OrdinalIgnoreCase))
			{
				return new WrongAnswer
				{
					Arg = Convert.ToUInt64(response.values[0], 16),
					CorrectValue = Convert.ToUInt64(response.values[1], 16),
					ProvidedValue = Convert.ToUInt64(response.values[2], 16),
				};
			}

			throw new ApplicationException(string.Format("Error GuessResponse ({0}): {1}", response.status, response.message));
		}

		[NotNull]
		public TrainResponse Train(TrainProblemType trainProblemType = TrainProblemType.Any, int? size = null)
		{
			var request = new TrainRequest(size);
			switch (trainProblemType)
			{
				case TrainProblemType.Any:
					request.operators = null;
					break;
				case TrainProblemType.Simple:
					request.operators = new string[0];
					break;
				case TrainProblemType.Fold:
					request.operators = new[] { "fold" };
					break;
				case TrainProblemType.Tfold:
					request.operators = new[] { "tfold" };
					break;
				case TrainProblemType.Bonus:
					request.size = 42;
					break;
				default:
					throw new ArgumentOutOfRangeException("trainProblemType");
			}

			var response = webApi.Train(request);
			return response;
		}
	}
}