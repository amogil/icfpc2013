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

        private static bool IsSolve(Expr e, ulong[] inputs, ulong[] outputs)
        {
            return !inputs.Where((t, i) => e.Eval(new Vars(t)) != outputs[i]).Any();
        }
    }

    [TestFixture]
    public class Guesser_Test
    {
        [Test]
        public void Test()
        {
            Expr formula = Expr.ParseFunction("(lambda (x) (and (xor (shr4 x) x) x))");
            var operations = new[] {"and", "shr4", "xor"};
            int size = 7;
            var random = new Random();

            Expr[] trees = new Force().Solve(size - 1, operations).ToArray();

            do
            {
                ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();
                ulong[] outputs = inputs.Select(i => formula.Eval(new Vars(i))).ToArray();                
                trees = Guesser.Guess(trees, inputs, outputs).ToArray();
            } while (trees.Length > 1);
        }
    }
}