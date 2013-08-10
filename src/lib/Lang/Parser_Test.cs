using System;
using NUnit.Framework;

namespace lib.Lang
{
	[TestFixture]
	public class Parser_Test
	{
		[TestCase("0")]
		[TestCase("1")]
		[TestCase("(shl1 1)")]
		[TestCase("(shr1 1)")]
		[TestCase("(shr4 1)")]
		[TestCase("(shr16 1)")]
		[TestCase("(not 1)")]
		[TestCase("(and 1 0)")]
		[TestCase("(or 1 0)")]
		[TestCase("(xor 1 0)")]
		[TestCase("(plus 1 0)")]
		[TestCase("(if0 0 1 0)")]
		[TestCase("(fold 0 1 (lambda (i a) (plus i a)))")]
		[TestCase("(xor (fold 0 1 (lambda (i a) (plus i a))) 1)")]
		public void ParseCorrect(string s)
		{
			Console.WriteLine(Parser.ParseExpr(s).Printable());
			Assert.AreEqual(s, Parser.ParseExpr(s).ToSExpr().Item1);
		}
	}
}