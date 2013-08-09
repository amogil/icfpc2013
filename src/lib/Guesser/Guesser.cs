using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using lib.Brute;
using lib.Lang;

namespace lib.Guesser
{
    internal class Guesser
    {
        public static IEnumerable<Expr> Guess(IEnumerable<Expr> formulas, ulong[] inputs, ulong[] outputs)
        {
            if (inputs.Length != outputs.Length)
                throw new ArgumentException();
            return formulas.Where(e => IsSolve(e, inputs, outputs));
        }

        private static bool IsSolve(Expr e, IEnumerable<ulong> inputs, ulong[] outputs)
        {
            return !inputs.Where((t, i) => e.Eval(new Vars(t)) != outputs[i]).Any();
        }
    }

    [TestFixture]
    public class Guesser_Test
    {
//        [TestCase("(lambda (x) (not x))", 3, new[] {"not"}, 1)]
//        [TestCase("(lambda (x) (shr4 x))", 3, new[] {"shr4"}, 1)]
//        [TestCase("(lambda (x) (plus x 1))", 4, new[] {"plus"}, 2)]
//        [TestCase("(lambda (x) (and (xor (shr4 x) x) x))", 7, new[] {"and", "shr4", "xor"}, 8)]
//        [TestCase("(lambda (x) (plus (or 1 x) (shl1 x)))", 7, new[] {"or", "plus", "shl1"}, 8)]
            [TestCase("(lambda (x) (fold x 0 (lambda (x y) (plus (or x y) x))))", 10, new[] {"or", "plus", "fold"}, 8)]
        public void Test(string function, int size, string[] operations, int equalFormulas)
        {
            Expr formula = Expr.ParseFunction(function);
            var random = new Random();

            Expr[] trees = new Force().Solve(size - 1, operations).ToArray();


            foreach (Expr tree in trees)
            {
                Console.WriteLine(tree);
            }

            ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();
            ulong[] outputs = inputs.Select(i => formula.Eval(new Vars(i))).ToArray();
            trees = Guesser.Guess(trees, inputs, outputs).ToArray();
            Assert.AreEqual(equalFormulas, trees.Count());
        }
    }
}