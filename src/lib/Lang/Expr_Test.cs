using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace lib.Lang
{
	[TestFixture]
	public class Expr_Test
	{
		[Test]
		public void Tokenize()
		{
			CollectionAssert.AreEqual(new[] { "(", "x", ")" }, Expr.Tokenize("(x)"));
			CollectionAssert.AreEqual(new[] { "x" }, Expr.Tokenize("x"));
			CollectionAssert.AreEqual(new[] { "(", ")" }, Expr.Tokenize("()"));
			CollectionAssert.AreEqual(new[] { "(", ")" }, Expr.Tokenize("( )"));
			CollectionAssert.AreEqual(new[] { "(", ")" }, Expr.Tokenize("(\t\r\n)"));
			CollectionAssert.AreEqual(new[] { "(", "x", ")" }, Expr.Tokenize("(\t\r\n x)"));
			CollectionAssert.AreEqual(new[] { "(", "lambda", "(", "x", ")", "(", "shl1", "x", ")", ")" }, Expr.Tokenize("(lambda (x) (shl1 x))"));

		}

		[Test]
		public void Parse()
		{
			Assert.AreEqual("0", Expr.ParseExpr("0").ToSExpr());
			Assert.AreEqual("(shl1 1)", Expr.ParseExpr(" ( shl1      1) ").ToSExpr());
			Assert.AreEqual("(fold 0 0 (lambda (x i) (shl1 x)))", Expr.ParseExpr("(fold 0 0 (lambda (x   i )  ( shl1  x ) ))").ToSExpr());
		}

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
		[TestCase("(fold 0 1 (lambda (x y) (plus x y)))")]
		[TestCase("(xor (fold 0 1 (lambda (x y) (plus x y))) 1)")]
		public void ParseCorrect(string s)
		{
			Assert.AreEqual(s, Expr.ParseExpr(s).ToSExpr());
		}

		[Test]
		[TestCaseSource("GetEvalTestCases")]
		public void Eval(TestCase t)
		{
			var actual = Expr.Eval(t.Program, t.Arg);
			Assert.That(actual, Is.EqualTo(t.ExpectedValue), string.Format("Was: 0x{0:x}", actual));
		}

		[Test]
		[TestCaseSource("GetEvalTestCases")]
		public void EvalBin(TestCase t)
		{
			byte[] p = Parser.ParseFunction(t.Program);
			var actual = p.Eval(t.Arg);
			Assert.That(actual, Is.EqualTo(t.ExpectedValue), string.Format("Was: 0x{0:x}", actual));
		}
		
		private static IEnumerable<TestCase> GetEvalTestCases()
		{
			yield return new TestCase("(lambda (x) 0)", 1, 0);
			yield return new TestCase("(lambda (x) 0)", 0, 0);
			yield return new TestCase("(lambda (x) 1)", 1, 1);
			yield return new TestCase("(lambda (x) 1)", 0, 1);

			yield return new TestCase("(lambda (x) x)", 1, 1);
			yield return new TestCase("(lambda (x) x)", 0, 0);
			yield return new TestCase("(lambda (x) x)", 5, 5);

			yield return new TestCase("(lambda (x) (not x))", 0, 0xffffffffffffffff);
			yield return new TestCase("(lambda (x) (not x))", 1, 0xfffffffffffffffe);
			yield return new TestCase("(lambda (x) (not x))", 0xffffffffffffffff, 0);
			yield return new TestCase("(lambda (x) (not x))", 0xfffffffffffffffe, 1);

			yield return new TestCase("(lambda (x) (shl1 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shl1 x))", 1, 2);
			yield return new TestCase("(lambda (x) (shl1 x))", 0xffffffffffffffff, 0xfffffffffffffffe);
			yield return new TestCase("(lambda (x) (shl1 x))", 0xfffffffffffffffe, 0xfffffffffffffffc);
			yield return new TestCase("(lambda (x) (shl1 x))", 3, 6);

			yield return new TestCase("(lambda (x) (shr1 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shr1 x))", 1, 0);
			yield return new TestCase("(lambda (x) (shr1 x))", 0xffffffffffffffff, 0x7fffffffffffffff);
			yield return new TestCase("(lambda (x) (shr1 x))", 0xfffffffffffffffe, 0x7fffffffffffffff);

			yield return new TestCase("(lambda (x) (shr4 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shr4 x))", 1, 0);
			yield return new TestCase("(lambda (x) (shr4 x))", 0xf, 0);
			yield return new TestCase("(lambda (x) (shr4 x))", 0x10, 1);
			yield return new TestCase("(lambda (x) (shr4 x))", 0xffffffffffffffff, 0x0fffffffffffffff);

			yield return new TestCase("(lambda (x) (shr16 x))", 0, 0);
			yield return new TestCase("(lambda (x) (shr16 x))", 1, 0);
			yield return new TestCase("(lambda (x) (shr16 x))", 0xffff, 0);
			yield return new TestCase("(lambda (x) (shr16 x))", 0x10000, 1);
			yield return new TestCase("(lambda (x) (shr16 x))", 0xffffffffffffffff, 0x0000ffffffffffff);

			yield return new TestCase("(lambda (x) (and x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (and x 0))", 1, 0);
			yield return new TestCase("(lambda (x) (and x 1))", 0, 0);
			yield return new TestCase("(lambda (x) (and x 1))", 1, 1);

			yield return new TestCase("(lambda (x) (or x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (or x 0))", 1, 1);
			yield return new TestCase("(lambda (x) (or x 1))", 0, 1);
			yield return new TestCase("(lambda (x) (or x 1))", 1, 1);

			yield return new TestCase("(lambda (x) (xor x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (xor x 0))", 1, 1);
			yield return new TestCase("(lambda (x) (xor x 1))", 0, 1);
			yield return new TestCase("(lambda (x) (xor x 1))", 1, 0);
			yield return new TestCase("(lambda (x) (xor x x))", 3, 0);
			yield return new TestCase("(lambda (x) (xor x x))", unchecked ((ulong) (-1)), 0);

			yield return new TestCase("(lambda (x) (plus x 0))", 0, 0);
			yield return new TestCase("(lambda (x) (plus x 0))", 1, 1);
			yield return new TestCase("(lambda (x) (plus x 1))", 0, 1);
			yield return new TestCase("(lambda (x) (plus x 1))", 1, 2);
			yield return new TestCase("(lambda (x) (plus x 1))", 0xfffffffffffffffe, 0xffffffffffffffff);
			yield return new TestCase("(lambda (x) (plus x 1))", 0xffffffffffffffff, 0);

			yield return new TestCase("(lambda (x) (xor 0 (and 1 (or x (plus x (not (shl1 (shr1 (shr4 (shr16 x))))))))))", 0, 1);
			yield return new TestCase("(lambda (x) (xor 0 (and x (or x (plus x (not (shl1 (shr1 (shr4 (shr16 x))))))))))", 12345678, 12345678);
			yield return new TestCase("(lambda (x) (not (shl1 (shr1 (shr4 (shr16 x))))))", 1 << 20, unchecked((ulong)(-1)));

			yield return new TestCase("(lambda (x) (fold x 0 (lambda (x acc) (plus x acc)))", 0, 0);
			yield return new TestCase("(lambda (x) (fold x 0 (lambda (x acc) (plus x acc)))", 1, 1);
			yield return new TestCase("(lambda (x) (fold x (plus 1 1) (lambda (x acc) (plus x acc)))", 65535, 512);

			yield return new TestCase("(lambda (x) (if0 0 1 0))", 1, 1);
			yield return new TestCase("(lambda (x) (if0 0 1 0))", 0, 1);
			yield return new TestCase("(lambda (x) (if0 x 0 1))", 0, 0);
			yield return new TestCase("(lambda (x) (if0 x 0 1))", 1, 1);
            // (fold x_27005 0 (lambda (x_27005 x_27006) (shr1 (if0 x_27005 (if0 (plus (shl1 (shr1 0)) x_27005) x_27005 x_27006) x_27005))))
            yield return new TestCase("(lambda (x) (plus (fold (plus x 1) 0 (lambda (x acc) (plus x acc))) x))", 1, 3);
            yield return new TestCase("(lambda (x) (plus x (fold (plus x 1) 0 (lambda (x acc) (plus x acc)))))", 1, 3);
		}

		public class TestCase
		{
			public TestCase(string program, ulong arg, ulong expectedValue)
			{
				Program = program;
				Arg = arg;
				ExpectedValue = expectedValue;
			}

			public string Program { get; set; }
			public UInt64 Arg { get; set; }
			public UInt64 ExpectedValue { get; set; }

			public override string ToString()
			{
				return string.Format("{0}, P(0x{1:x}) = 0x{2:x}", Program, Arg, ExpectedValue);
			}
		}
	}
}