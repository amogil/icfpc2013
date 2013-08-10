using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Web
{
	public class MyProblemJson
	{
		public string id;
		public int size;
		public string[] operators;
		public bool? solved;
		public double? timeLeft;

		public IEnumerable<string> OperatorsExceptBonus
		{
			get { return operators.Where(t => !t.Equals("bonus", StringComparison.OrdinalIgnoreCase)); }
		}

		public bool IsBonus
		{
			get { return operators.Contains("bonus"); }
		}
	}
}