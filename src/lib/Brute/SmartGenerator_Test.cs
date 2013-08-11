using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Lang;
using lib.Web;

namespace lib.Brute
{
	[TestFixture]
	public class SmartGenerator_Test
	{

		[Test]
		public void IfOptimization()
		{
			var items = new SmartGenerator("if0", "shl1", "shr1").Enumerate(6);
			foreach (var t in items)
			{
				Console.WriteLine(t.Printable());
			}
		}
		[Test]
		public void BinaryOptimization()
		{
			var items = new SmartGenerator("and", "shl1", "shr1").Enumerate(6);
			foreach (var t in items)
			{
				Console.WriteLine(t.Printable());
			}
		}

		[Test]
		public void Test()
		{
//			CheckResult("|<00", new SmartGenerator("or", "shl1").Enumerate(4), 2);
			CheckResult("&Rrx>0", new SmartGenerator("and", "shr16", "shr4", "shr1").Enumerate(6), 2);
		}

		[TestCase("Id: Q8CaRCOpwfImQx4ZhFTW3b82, Challenge: (lambda (x_3842) (or (shr4 x_3842) 1)), Size: 5, Operators: or,shr4", TestName = "5")]
		[TestCase("Id: ErkXhpfABEVgAziRBgNyAAl7, Challenge: (lambda (x_11363) (shr1 (if0 (and (shr16 (shr4 x_11363)) (shr4 0)) 0 0))), Size: 11, Operators: and,if0,shr1,shr16,shr4")]
		[TestCase("Id: h8tm9Awt9fVxIpGAyJ49eA4J, Challenge: (lambda (x_10772) (shl1 (if0 (or (not (shl1 0)) 0) 0 (shr4 0)))), Size: 11, Operators: if0,not,or,shl1,shr4")]
		[TestCase("Id: AYTI87tdqN0qYuvM50gpb5UU, Challenge: (lambda (x_11454) (shr16 (if0 (and (shr4 (shr16 (shr1 x_11454))) x_11454) 1 x_11454))), Size: 11, Operators: and,if0,shr1,shr16,shr4")]
		[TestCase("Id: tP03Ueo4RbjUr7rTS5fP15Ym, Challenge: (lambda (x_10478) (shr4 (if0 (and (not (shr16 (shl1 1))) x_10478) 1 x_10478))), Size: 11, Operators: and,if0,not,shl1,shr16,shr4")]
		[TestCase("Id: 8xejannY4A6qJ4Jnq1AW3F1C, Challenge: (lambda (x_11226) (not (if0 (and (plus (shl1 x_11226) x_11226) x_11226) 0 x_11226))), Size: 11, Operators: and,if0,not,plus,shl1")]
		[TestCase("Id: sSV8yJ0TfPaiUMWDBT5vHAKN, Challenge: (lambda (x_10869) (shl1 (if0 (and (or (shr16 x_10869) x_10869) x_10869) x_10869 x_10869))), Size: 11, Operators: and,if0,or,shl1,shr16")]
		[TestCase("Id: BEBwd0AADByfBOYfXG8JH23E, Challenge: (lambda (x_13034) (and (if0 (xor (shr16 (not (not x_13034))) 1) 0 1) x_13034)), Size: 12, Operators: and,if0,not,shr16,xor",
			TestName = "12 1")]
		[TestCase("Id: lHObnL0KiFVC3c8vPkDDzAUc, Challenge: (lambda (x_12074) (shr4 (shr4 (if0 (or (shr16 x_12074) 1) 0 (shl1 (shr1 0)))))), Size: 12, Operators: if0,or,shl1,shr1,shr16,shr4",
			TestName = "12 2")]
		[TestCase("Id: DOwDVsOZZdTXvcDBF0qjYg0f, Challenge: (lambda (x_22843) (shr1 (shr4 (fold x_22843 1 (lambda (x_22844 x_22845) (or (plus x_22844 x_22845) (shr4 x_22845))))))), Size: 13, Operators: fold,or,plus,shr1,shr4",
			TestName = "13")]
		[TestCase("Id: Kw4oc5sgtg2UOXMTYpOzLZFV, Challenge: (lambda (x_16277) (fold x_16277 0 (lambda (x_16277 x_16278) (if0 (or (shr4 1) (shr16 (shr1 x_16277))) x_16277 1)))), Size: 14, Operators: if0,or,shr1,shr16,shr4,tfold",
			TestName = "14 tfold")]
		public void MeasureMaskOptimization(string line)
		{
			var trainProblem = TrainResponse.Parse(line);
			byte[] p = Parser.ParseFunction(trainProblem.challenge);
			var ans = p.Printable();

			var operations = trainProblem.operators;
			var random = new Random();
			var answers = Enumerable.Range(0, 100).Select(i => random.NextUInt64()).Select(p.Eval).ToArray();
			var mask = new Mask(answers);
			Console.WriteLine("          answers mask: {0}", mask);
			Console.WriteLine("challenge program mask: {0}", p.GetMask());

//			CheckResult(ans, new SmartGenerator(operations).Enumerate(trainProblem.size - 1));
//			CheckResult(ans, new BinaryBruteForcer(operations).Enumerate(trainProblem.size - 1));
			CheckResult(ans, new SmartGenerator(answers, operations).Enumerate(trainProblem.size - 1), 9);
			CheckResult(ans, new BinaryBruteForcer(new Mask(answers), operations).Enumerate(trainProblem.size - 1));
		}

		private static void CheckResult(string ans, IEnumerable<byte[]> result, int prefixSize = -1)
		{
			Console.WriteLine("answer: {0}", ans);
			var sw = Stopwatch.StartNew();
			var found = false;
			int c = 0;
			int printed = 0;
			foreach (var r in result)
			{
				var v = r.Printable();
				if (prefixSize >= 0 && printed < 100 && ans.StartsWith(v.Substring(0, prefixSize)))
				{
					Console.WriteLine(v);
					printed++;
				}
				found = found || v == ans;
				c++;
			}
			Console.WriteLine("trees generated: {0}", c);
			Console.WriteLine(sw.Elapsed);
			Assert.IsTrue(found);
		}
	}
}