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
		public void ParseCorrect(string s)
		{
			Assert.AreEqual(s, Expr.ParseExpr(s).ToSExpr());
		}

		[Test]
		[TestCase("(lambda (x) x)", 1, 1)]
		[TestCase("(lambda (x) x)", 0, 0)]
		[TestCase("(lambda (x) x)", 5, 5)]
		[TestCase("(lambda (x) (shl1 x))", 1, 2)]
		[TestCase("(lambda (x) (shl1 x))", 3, 6)]
		[TestCase("(lambda (x) (xor x x))", 3, 0)]
		[TestCase("(lambda (x) (xor x x))", -1, 0)]
		[TestCase("(lambda (x) (xor 0 (and 1 (or x (plus x (not (shl1 (shr1 (shr4 (shr16 x))))))))))", 0, 1)]
		[TestCase("(lambda (x) (xor 0 (and x (or x (plus x (not (shl1 (shr1 (shr4 (shr16 x))))))))))", 12345678, 12345678)]
		[TestCase("(lambda (x) (not (shl1 (shr1 (shr4 (shr16 x))))))", 1<<20, -1)]
		[TestCase("(lambda (x) (fold x 0 (lambda (x acc) (plus x acc)))", 0, 0)]
		[TestCase("(lambda (x) (fold x 0 (lambda (x acc) (plus x acc)))", 1, 1)]
		[TestCase("(lambda (x) (fold x (plus 1 1) (lambda (x acc) (plus x acc)))", 65535, 512)]
		[TestCase("(lambda (x) (if0 0 1 0))", 1, 1)]
		[TestCase("(lambda (x) (if0 0 1 0))", 0, 1)]
		[TestCase("(lambda (x) (if0 x 0 1))", 0, 0)]
		[TestCase("(lambda (x) (if0 x 0 1))", 1, 1)]
		public void Eval(string sExpr, long arg, long expected)
		{
			Assert.AreEqual(expected, Expr.Eval(sExpr, arg));
		}
	}
}