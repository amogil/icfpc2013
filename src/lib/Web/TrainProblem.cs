using System;

namespace lib.Web
{
	public class TrainProblem
	{
		public string id;
		public string challenge;
		public int size;
		public string[] operators;

		public override string ToString()
		{
			return string.Format("Id: {0}, Challenge: {1}, Size: {2}, Operators: {3}", id, challenge, size, string.Join(",", operators));
		}

		public static TrainProblem Parse(string text)
		{
			var problem = new TrainProblem();
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