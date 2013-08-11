using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web.Script.Serialization;
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
			CheckResult("|<00", new SmartGenerator("or", "shl1").Enumerate(4), 2);
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
		[TestCase("Id: staDPTOgLqXkpBRbtKLvfBNS, Challenge: (lambda (x_16003) (fold x_16003 0 (lambda (x_16003 x_16004) (if0 (and (shr1 (shr4 0)) x_16004) (shr4 x_16003) x_16003)))), Size: 14, Operators: and,if0,shr1,shr4,tfold",
			TestName = "bad")]
		[TestCase("Id: zTXg50cq0UqB5iSi0PNUQi7k, Challenge: (lambda (x_17115) (fold x_17115 0 (lambda (x_17115 x_17116) (if0 (plus (not (shl1 1)) x_17116) (shr1 1) x_17115)))), Size: 14, Operators: if0,not,plus,shl1,shr1,tfold",
			TestName = "40sec")]
		[TestCase("Id: jze5RDoAhI187H2KJnRLl1sq, Challenge: (lambda (x_30947) (fold (and (not (not x_30947)) (shr16 x_30947)) 0 (lambda (x_30948 x_30949) (not (plus x_30948 x_30949))))), Size: 14, Operators: and,fold,not,plus,shr16",
			TestName = "not_slow_any_more")]
		public void MeasureMaskOptimization(string line)
		{
			var trainProblem = TrainResponse.Parse(line);
			Console.WriteLine(trainProblem.id);
			byte[] p = Parser.ParseFunction(trainProblem.challenge);
			var ans = p.Printable();

			var operations = trainProblem.operators;
			var random = new Random();
			var inputs = Enumerable.Range(0, 200).Select(i => random.NextUInt64())
				//.Concat(new[] { Convert.ToUInt64("0x0008004000000020", 16) })
				.ToArray();
			var answers = inputs.Select(p.Eval).ToArray();
			var mask = new Mask(answers);
			Console.WriteLine("          answers mask: {0}", mask);
			Console.WriteLine("challenge program mask: {0}", p.GetMask());

//			CheckResult(ans, new SmartGenerator(operations).Enumerate(trainProblem.size - 1));
//			CheckResult(ans, new BinaryBruteForcer(operations).Enumerate(trainProblem.size - 1));
//			Solve(inputs, answers, new SmartGenerator(inputs, answers, operations).Enumerate(trainProblem.size - 1));
			CheckResult(ans, new SmartGenerator(inputs, answers, operations).Enumerate(trainProblem.size - 1));
//			CheckResult(ans, new BinaryBruteForcer(new Mask(answers), operations).Enumerate(trainProblem.size - 1));
		}

		[Test]
		public void TestDub()
		{
			/*
			 {\"id\":\"qq570J9D61AKWI8QGhBNnIAM\",\"size\":15,\"operators\":[\"if0\",\"not\",\"or\",\"shl1\",\"shr1\",\"shr4\",\"xor\"],\"challenge\":\"(lambda (x_17940) (or (shr1 (not (xor (if0 (shr4 (shr4 (shl1 x_17940))) (shr1 x_17940) 1) x_17940))) x_17940))\"}


[\"0xD091BB5C22AE9EF6\",\"0xE7E1FAEED5C31F79\",\"0x2082352CF807B7DF\",\"0xE9D300053895AFE1\",\"0xA1E24BBA4EE4092B\",\"0x18F868638C16A625\",\"0x474BA8C43039CD1A\",\"0x8C006D5FFE2D7810\",\"0xF51F2AE7FF1816E4\",\"0xF702EF59F7BADAFA\",\"0x285954A1B9D09511\",\"0xF878C4B3FB2A0137\",\"0xF508E4AA1C1FE652\",\"0x7C419418CC50AA59\",\"0xCCDF2E5C4C0A1F3B\",\"0x2452A9DC01397D8D\",\"0x6BF88C311CCA797A\",\"0xEA6DA4AEA3C78807\",\"0xCACE1969E0E0D4AD\",\"0xF5A14BAB80F00988\",\"0xA7DE9F4CCC450CBA\",\"0x0924668F5C7DC380\",\"0xD96089C53640AC4C\",\"0xEF1A2E6DAE6D9426\",\"0xADC1965B6613BA46\",\"0xC1FB41C2BD9B0ECD\",\"0xBE3DEDFC7989C8EE\",\"0x6468FD6E6C0DF032\",\"0xA7CD66342C826D8B\",\"0x2BD2E4124D4A2DBE\",\"0xB4BF6FA7CC1A8959\",\"0x0826328251097330\",\"0x46E46CB0DF577EC2\",\"0x0BD1E364262C5564\",\"0x18DDA0C9FE7B45D9\",\"0xD2CE21C9D268409A\",\"0xB1E049E1200BFA47\",\"0x512D6E73C3851EEE\",\"0xF341C0817D973E48\",\"0x08D17554A9E20D28\",\"0x70518CE6203AC303\",\"0x61ADD0AB35D0430C\",\"0xC3F8E8920D1C8509\",\"0xCB92388E095436BF\",\"0x2FD6E20868A29AF9\",\"0x7D61330B753EC6FC\",\"0x7211EFEA7CD15133\",\"0xA574C4FFCB41F198\",\"0xB598EEF6EBBE7347\",\"0xC1332568CEBA5A70\",\"0x46A99459B4AD9F11\",\"0xAE00FEAA00B8B573\",\"0xA7B480B6B5F0B06C\",\"0x29A0EC27A4DAA010\",\"0x1E76A1C574BE9133\",\"0x7F94C950C61F6ED6\",\"0xF5B1C7A192E195F8\",\"0x572384D4E0732C88\",\"0x95D41B68CEE496C3\",\"0x394BBD52048CD47C\",\"0xC05309BED23D2D63\",\"0x414DE9C5D2229F23\",\"0x818666A3F0A8B109\",\"0xB2F6B12769A48341\"]

{\"status\":\"ok\",\"outputs\":[\"0xD7B7BB5DEEAEBEF6\",\"0xEFEFFAEED5DF7F7B\",\"0x6FBEF56DFBFFB7DF\",\"0xEBD77FFD7BB5AFEF\",\"0xAFEEDBBADEEDFB6B\",\"0x7BFBEBEFBDF6AEED\",\"0x5F5BABDDF7FBDD7A\",\"0xBDFFED5FFEED7BF7\",\"0xF57F6AEFFF7BF6ED\",\"0xF77EEF5BF7BADAFA\",\"0x6BDB55AFBBD7B577\",\"0xFBFBDDB7FB6AFF77\",\"0xF57BEDAAFDFFEED6\",\"0x7DDFB5FBDDD7AADB\",\"0xDDDF6EDDDDFAFF7B\",\"0x6DD6ABDDFF7B7DBD\",\"0x6BFBBDF77DDAFB7A\",\"0xEAEDADAEAFDFBBFF\",\"0xDADEFB6BEFEFD5AD\",\"0xF5AF5BABBFF7FBBB\",\"0xAFDEBF5DDDDD7DBA\",\"0x7B6DEEBF5DFDDFBF\",\"0xDB6FBBDD76DFADDD\",\"0xEF7AEEEDAEEDB5EE\",\"0xADDFB6DB6EF7BADE\",\"0xDFFB5FDEBDBB7EDD\",\"0xBEFDEDFDFBBBDBEE\",\"0x6DEBFD6EEDFDF7F6\",\"0xAFDD6EF5EDBEEDBB\",\"0x6BD6EDF6DD5AEDBE\",\"0xB5BF6FAFDDFABB5B\",\"0x7BEEF6BED77B7777\",\"0x5EEDEDB7DF577EDE\",\"0x7BD7EF6DEEEDD56D\",\"0x7BDDAFDBFEFB5DDB\",\"0xD6DEEFDBD6EBDFBA\",\"0xB7EFDBEF6FFBFADF\",\"0x576D6EF7DFBD7EEE\",\"0xF75FDFBF7DB77EDB\",\"0x7BD77555ABEEFD6B\",\"0x77D7BDEEEFFADF7F\",\"0x6FADD7AB75D7DF7D\",\"0xDFFBEBB6FD7DBD7B\",\"0xDBB6FBBEFB55F6BF\",\"0x6FD6EEFBEBAEBAFB\",\"0x7D6F777B757EDEFD\",\"0x76F7EFEAFDD75777\",\"0xAD75DDFFDB5FF7BB\",\"0xB5BBEEF6EBBEF75F\",\"0xDF776D6BDEBADAF7\",\"0x5EABB5DBB5ADBF77\",\"0xAEFFFEAAFFBBB577\",\"0xAFB5BFB6B5F7B7ED\",\"0x6BAFEDEFADDAAFF7\",\"0x7EF6AFDD75BEB777\",\"0x7FB5DB57DEFF6ED6\",\"0xF5B7DFAFB6EFB5FB\",\"0x576FBDD5EFF76DBB\",\"0xB5D5FB6BDEEDB6DF\",\"0x7B5BBD56FDBDD5FD\",\"0xDFD77BBED6FD6D6F\",\"0x5F5DEBDDD6EEBF6F\",\"0xBFBEEEAFF7ABB77B\",\"0xB6F6B76F6BADBF5F\"]}

{\"status\":\"mismatch\",\"values\":[\"0x8000000000000055\",\"0x9FFFFFFFFFFFFFD5\",\"0xBFFFFFFFFFFFFFD5\"]}
{\"status\":\"mismatch\",\"values\":[\"0x0000000000000000\",\"0x7FFFFFFFFFFFFFFF\",\"0x0000000000000000\"]}
{\"status\":\"mismatch\",\"values\":[\"0x55555555556AAA3F\",\"0x55555555556AAAFF\",\"0x5555555555AAA8F6\"]}
{\"status\":\"mismatch\",\"values\":[\"0x0000000000000014\",\"0x7FFFFFFFFFFFFFF4\",\"0xC000000000000016\"]}
{\"status\":\"mismatch\",\"values\":[\"0x800000000000006A\",\"0x9FFFFFFFFFFFFFFA\",\"0x9FFFFFFFFFFFFFEA\"]}
			 */
			var json = new JavaScriptSerializer();
			var problem = json.Deserialize<TrainResponse>("{\"id\":\"qq570J9D61AKWI8QGhBNnIAM\",\"size\":15,\"operators\":[\"if0\",\"not\",\"or\",\"shl1\",\"shr1\",\"shr4\",\"xor\"],\"challenge\":\"(lambda (x_17940) (or (shr1 (not (xor (if0 (shr4 (shr4 (shl1 x_17940))) (shr1 x_17940) 1) x_17940))) x_17940))\"}");
			Console.WriteLine(problem);
 			var inputs =  json.Deserialize<List<string>>("[\"0xD091BB5C22AE9EF6\",\"0xE7E1FAEED5C31F79\",\"0x2082352CF807B7DF\",\"0xE9D300053895AFE1\",\"0xA1E24BBA4EE4092B\",\"0x18F868638C16A625\",\"0x474BA8C43039CD1A\",\"0x8C006D5FFE2D7810\",\"0xF51F2AE7FF1816E4\",\"0xF702EF59F7BADAFA\",\"0x285954A1B9D09511\",\"0xF878C4B3FB2A0137\",\"0xF508E4AA1C1FE652\",\"0x7C419418CC50AA59\",\"0xCCDF2E5C4C0A1F3B\",\"0x2452A9DC01397D8D\",\"0x6BF88C311CCA797A\",\"0xEA6DA4AEA3C78807\",\"0xCACE1969E0E0D4AD\",\"0xF5A14BAB80F00988\",\"0xA7DE9F4CCC450CBA\",\"0x0924668F5C7DC380\",\"0xD96089C53640AC4C\",\"0xEF1A2E6DAE6D9426\",\"0xADC1965B6613BA46\",\"0xC1FB41C2BD9B0ECD\",\"0xBE3DEDFC7989C8EE\",\"0x6468FD6E6C0DF032\",\"0xA7CD66342C826D8B\",\"0x2BD2E4124D4A2DBE\",\"0xB4BF6FA7CC1A8959\",\"0x0826328251097330\",\"0x46E46CB0DF577EC2\",\"0x0BD1E364262C5564\",\"0x18DDA0C9FE7B45D9\",\"0xD2CE21C9D268409A\",\"0xB1E049E1200BFA47\",\"0x512D6E73C3851EEE\",\"0xF341C0817D973E48\",\"0x08D17554A9E20D28\",\"0x70518CE6203AC303\",\"0x61ADD0AB35D0430C\",\"0xC3F8E8920D1C8509\",\"0xCB92388E095436BF\",\"0x2FD6E20868A29AF9\",\"0x7D61330B753EC6FC\",\"0x7211EFEA7CD15133\",\"0xA574C4FFCB41F198\",\"0xB598EEF6EBBE7347\",\"0xC1332568CEBA5A70\",\"0x46A99459B4AD9F11\",\"0xAE00FEAA00B8B573\",\"0xA7B480B6B5F0B06C\",\"0x29A0EC27A4DAA010\",\"0x1E76A1C574BE9133\",\"0x7F94C950C61F6ED6\",\"0xF5B1C7A192E195F8\",\"0x572384D4E0732C88\",\"0x95D41B68CEE496C3\",\"0x394BBD52048CD47C\",\"0xC05309BED23D2D63\",\"0x414DE9C5D2229F23\",\"0x818666A3F0A8B109\",\"0xB2F6B12769A48341\"]")
				.Select(s => Convert.ToUInt64(s, 16)).ToArray();
			var outputs = json.Deserialize<List<string>>("[\"0xD7B7BB5DEEAEBEF6\",\"0xEFEFFAEED5DF7F7B\",\"0x6FBEF56DFBFFB7DF\",\"0xEBD77FFD7BB5AFEF\",\"0xAFEEDBBADEEDFB6B\",\"0x7BFBEBEFBDF6AEED\",\"0x5F5BABDDF7FBDD7A\",\"0xBDFFED5FFEED7BF7\",\"0xF57F6AEFFF7BF6ED\",\"0xF77EEF5BF7BADAFA\",\"0x6BDB55AFBBD7B577\",\"0xFBFBDDB7FB6AFF77\",\"0xF57BEDAAFDFFEED6\",\"0x7DDFB5FBDDD7AADB\",\"0xDDDF6EDDDDFAFF7B\",\"0x6DD6ABDDFF7B7DBD\",\"0x6BFBBDF77DDAFB7A\",\"0xEAEDADAEAFDFBBFF\",\"0xDADEFB6BEFEFD5AD\",\"0xF5AF5BABBFF7FBBB\",\"0xAFDEBF5DDDDD7DBA\",\"0x7B6DEEBF5DFDDFBF\",\"0xDB6FBBDD76DFADDD\",\"0xEF7AEEEDAEEDB5EE\",\"0xADDFB6DB6EF7BADE\",\"0xDFFB5FDEBDBB7EDD\",\"0xBEFDEDFDFBBBDBEE\",\"0x6DEBFD6EEDFDF7F6\",\"0xAFDD6EF5EDBEEDBB\",\"0x6BD6EDF6DD5AEDBE\",\"0xB5BF6FAFDDFABB5B\",\"0x7BEEF6BED77B7777\",\"0x5EEDEDB7DF577EDE\",\"0x7BD7EF6DEEEDD56D\",\"0x7BDDAFDBFEFB5DDB\",\"0xD6DEEFDBD6EBDFBA\",\"0xB7EFDBEF6FFBFADF\",\"0x576D6EF7DFBD7EEE\",\"0xF75FDFBF7DB77EDB\",\"0x7BD77555ABEEFD6B\",\"0x77D7BDEEEFFADF7F\",\"0x6FADD7AB75D7DF7D\",\"0xDFFBEBB6FD7DBD7B\",\"0xDBB6FBBEFB55F6BF\",\"0x6FD6EEFBEBAEBAFB\",\"0x7D6F777B757EDEFD\",\"0x76F7EFEAFDD75777\",\"0xAD75DDFFDB5FF7BB\",\"0xB5BBEEF6EBBEF75F\",\"0xDF776D6BDEBADAF7\",\"0x5EABB5DBB5ADBF77\",\"0xAEFFFEAAFFBBB577\",\"0xAFB5BFB6B5F7B7ED\",\"0x6BAFEDEFADDAAFF7\",\"0x7EF6AFDD75BEB777\",\"0x7FB5DB57DEFF6ED6\",\"0xF5B7DFAFB6EFB5FB\",\"0x576FBDD5EFF76DBB\",\"0xB5D5FB6BDEEDB6DF\",\"0x7B5BBD56FDBDD5FD\",\"0xDFD77BBED6FD6D6F\",\"0x5F5DEBDDD6EEBF6F\",\"0xBFBEEEAFF7ABB77B\",\"0xB6F6B76F6BADBF5F\"]")
				.Select(s => Convert.ToUInt64(s, 16)).ToArray();
			Console.WriteLine(new Mask(outputs));
			Console.WriteLine(new Mask(outputs));
			inputs = inputs.Concat(new[] { Convert.ToUInt64("0x8000000000000055", 16) }).ToArray();
			outputs = outputs.Concat(new[] { Convert.ToUInt64("0x9FFFFFFFFFFFFFD5", 16) }).ToArray();
			var gen = new SmartGenerator(inputs, outputs, problem.OperatorsExceptBonus);
			gen.filterInput = inputs.Last();
			gen.filterOutput = outputs.Last();
			var guesses = gen.Enumerate(problem.size - 1);
			Solve(inputs, outputs, guesses);

		}

		private void Solve(ulong[] inp, ulong[] outp, IEnumerable<byte[]> items)
		{
			var sw = Stopwatch.StartNew();
			var solutions = items.Select((t, i) => new{tree=t, isSolution=inp.Zip(outp, Tuple.Create).All(io => t.Eval(io.Item1) == io.Item2), index=i});
			var res = solutions.FirstOrDefault(s => s.isSolution);
			if (res == null) Console.WriteLine("No solution");
			else
			{
				Console.WriteLine("Steps: {0}", res.index);
				Console.WriteLine(res.tree.Printable());
				Console.WriteLine("(lambda (x) {0})", res.tree.ToSExpr().Item1);
			}
			Console.WriteLine(sw.Elapsed);
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