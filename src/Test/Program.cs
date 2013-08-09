using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using lib.Brute;

namespace Test
{
	class Program
	{
		static void Main(string[] args)
		{
			Test(10, "fold not or shr1 shr4");
		}

		public static void Test(int size, string ops)
		{
			var force = new Force();
			var trees = force.Solve(size, ops.Split(' ')).ToList();
			Console.WriteLine(trees.Count());
		}
	}
}
