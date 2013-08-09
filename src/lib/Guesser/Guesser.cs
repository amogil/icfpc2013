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
            var formula = Expr.ParseFunction("(and (xor (shr4 x_6337) x_6337) x_6337))");

            //Size: 8, Operators: and,shl1,shr1
            var trees = new Force().Solve(7, "and", "shl1", "shr1").ToList();

            Console.WriteLine(trees.Count());
            foreach (var tree in trees)
            {
                Console.WriteLine(tree);
            }
        }
    }
}