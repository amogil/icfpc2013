using System;
using NUnit.Framework;

namespace lib.Lang
{
	[TestFixture]
	public class Mask_Test
	{
		[Test]
		public void Test()
		{
			Assert.AreEqual("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", Mask.X.ToString());
			Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000001", Mask._1.ToString());
			Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000000", Mask._0.ToString());
			Assert.AreEqual("1111111111111111111111111111111111111111111111111111111111111110", Parser.ParseExpr("(not 1)").GetMask().ToString());
			Assert.AreEqual("0000000000000000000000000000000000000000000000000000000000000001", Parser.ParseExpr("1").GetMask().ToString());
			Assert.AreEqual("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx0", new byte[] { 8 }.GetMask().ToString());
			Assert.AreEqual("0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", new byte[] { 9 }.GetMask().ToString());
			Assert.AreEqual("0000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", new byte[] { 10 }.GetMask().ToString());
			Assert.AreEqual("0000000000000000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", new byte[] { 11 }.GetMask().ToString());
			Console.WriteLine(new byte[] {13, 10}.GetMask().ToString());
			Console.WriteLine(new byte[] {10}.GetMask().ToString());
			Console.WriteLine(new byte[] {13}.GetMask().ToString());
		}

		[Test]
		public void Test1()
		{
			Console.WriteLine(new byte[] { 13, 10 }.GetMask().ToString());
		}



		[TestCase("01x01x01x", "000111xxx", "00001x0xx")]
		[TestCase("000", "111", "000")]
		[TestCase("111", "111", "111")]
		[TestCase("111", "xxx", "xxx")]
		public void And(string a, string b, string expectedRes)
		{
			Assert.AreEqual(new Mask(expectedRes).ToString(), new Mask(a).And(new Mask(b)).ToString());
		}

		[TestCase("x111", 1, "1110")]
		public void ShiftLeft(string a, int sh, string expectedRes)
		{
			Assert.AreEqual(new Mask(expectedRes).ToString(), new Mask(a).ShiftLeft(sh).ToString());
		}

		[TestCase("xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", 1, "0xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
		public void ShiftRight(string a, int sh, string expectedRes)
		{
			Assert.AreEqual(new Mask(expectedRes).ToString(), new Mask(a).ShiftRight(sh).ToString());
		}

		[TestCase("01x01x01x", "000111xxx", "01x111x1x")]
		[TestCase("000", "111", "111")]
		[TestCase("000", "000", "000")]
		[TestCase("000", "xxx", "xxx")]
		[TestCase("111", "111", "111")]
		[TestCase("111", "xxx", "111")]
		[TestCase("0000xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx", "xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx")]
		public void Or(string a, string b, string expectedRes)
		{
			Assert.AreEqual(new Mask(expectedRes).ToString(), new Mask(a).Or(new Mask(b)).ToString());
		}

		[TestCase("01x01x01x", "000111xxx", "01x10xxxx")]
		[TestCase("000", "111", "111")]
		[TestCase("000", "000", "000")]
		[TestCase("000", "xxx", "xxx")]
		[TestCase("111", "111", "000")]
		[TestCase("111", "xxx", "xxx")]
		public void Xor(string a, string b, string expectedRes)
		{
			Assert.AreEqual(new Mask(expectedRes).ToString(), new Mask(a).Xor(new Mask(b)).ToString());
		}

		[TestCase("111", "xxx", true)]
		[TestCase("0", "0", true)]
		[TestCase("0", "1", false)]
		[TestCase("0", "x", true)]
		[TestCase("1", "1", true)]
		[TestCase("1", "0", false)]
		[TestCase("1", "x", true)]
		[TestCase("x", "1", false)]
		[TestCase("x", "0", false)]
		[TestCase("x", "x", true)]
		public void IncludeIn(string small, string big, bool ok)
		{
			Assert.AreEqual(ok, new Mask(small).IncludedIn(new Mask(big)));
		}

		[TestCase("x", "x", "x")]
		[TestCase("x", "1", "x")]
		[TestCase("x", "0", "x")]
		[TestCase("1", "x", "x")]
		[TestCase("1", "1", "0")]
		[TestCase("1", "0", "1")]
		[TestCase("0", "x", "x")]
		[TestCase("0", "1", "1")]
		[TestCase("0", "0", "0")]
		[TestCase("01", "01", "10")]
		[TestCase("01", "0x", "xx")]
		[TestCase("0x", "0x", "xx")]
		[TestCase("00", "0x", "0x")]
		[TestCase("0x", "01", "xx")]
		[TestCase("011", "011", "110")]
		[TestCase("001", "0x1", "xx0")]
		[TestCase("001", "0x1", "xx0")]
		public void Plus(string a, string b, string res)
		{
			Assert.AreEqual(new Mask(res).ToString(), new Mask(a).Plus(new Mask(b)).ToString());
		}


		[Test]
		public void TestToString()
		{
			var v = new Mask("0").ToString();
			Console.WriteLine(v);
			Console.WriteLine(new Mask(v).ToString());
		}
	}
}