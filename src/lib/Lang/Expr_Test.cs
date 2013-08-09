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
			Assert.AreEqual("x", Expr.Parse("x").ToSExpr());
			Assert.AreEqual("(shl1 x)", Expr.Parse(" ( shl1      x) ").ToSExpr());
			Assert.AreEqual("(fold 0 0 (lambda (x i) (shl1 x)))", Expr.Parse("(fold 0 0 (lambda (x   i )  ( shl1  x ) ))").ToSExpr());
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
			Assert.AreEqual(s, Expr.Parse(s).ToSExpr());
		}
	}
}