using System;
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

		public ulong[] Eval(string problemId, ulong[] inputs)
		{
			return Eval(new EvalRequest { id = problemId }, inputs);
		}

		public ulong[] EvalProgram(string program, ulong[] inputs)
		{
			return Eval(new EvalRequest { program = program }, inputs);
		}

		private ulong[] Eval(EvalRequest request, ulong[] inputs)
		{
			request.arguments = inputs.Select(ui => ui.ToString("X")).ToArray();

			var response = webApi.Eval(request);
			if (!response.status.Equals("ok", StringComparison.OrdinalIgnoreCase))
				throw new ApplicationException(string.Format("Error EvalResponse ({0}): {1}", response.status, response.message));

			var values = response.outputs.Select(str => Convert.ToUInt64(str, 16)).ToArray();
			return values;
		}

		[CanBeNull]
		public WrongAnswer Guess(string problemId, string formuala)
		{
			var request = new GuessRequest
			{
				id = problemId,
				program = formuala
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

		[CanBeNull]
		public TrainResponse Train(TrainType trainType = TrainType.Simple, int? size = null)
		{
			var request = new TrainRequest(size);
			switch (trainType)
			{
				case TrainType.Simple:
					break;
				case TrainType.Fold:
					request.operators = new[] {"fold"};
					break;
				case TrainType.Tfold:
					request.operators = new[] { "tfold" };
					break;
				case TrainType.Bonus:
					request.size = 42;
					break;
				default:
					throw new ArgumentOutOfRangeException("trainType");
			}

			var response = webApi.Train(request);
			return response;
		}
	}
}