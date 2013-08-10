using System;
using System.Linq;

namespace lib.Web
{
	public class TrainResponse
	{
		public string id;
		public string challenge;
		public int size;
		public string[] operators;

		public string[] OperatorsExceptBonus
		{
			get { return operators.Where(t => !t.Equals("bonus", StringComparison.OrdinalIgnoreCase)).ToArray(); }
		}

		public bool IsBonus
		{
			get { return operators.Contains("bonus"); }
		}

		public override string ToString()
		{
			return string.Format("Id: {0}, Challenge: {1}, Size: {2}, Operators: {3}, IsBonus: {4}", id, challenge, size, string.Join(",", operators), IsBonus);
		}

		public static TrainResponse Parse(string text)
		{
			var problem = new TrainResponse();
			try
			{
				var values = text.Split(new []{", "}, StringSplitOptions.RemoveEmptyEntries);
				foreach (var value in values)
				{
					if (value.StartsWith("Id:"))
						problem.id = value.Substring("Id:".Length).Trim();
					if (value.StartsWith("Challenge:"))
						problem.challenge = value.Substring("Challenge:".Length).Trim();
					if (value.StartsWith("Size:"))
						problem.size = int.Parse(value.Substring("Size:".Length).Trim());
					if (value.StartsWith("Operators:"))
						problem.operators = value.Substring("Operators:".Length).Trim().Split(',');
				}
				return problem;
			}
			catch (Exception)
			{
				return null;
			}
			
		}
	}
}