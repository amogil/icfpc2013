using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Text.RegularExpressions;

using lib.Lang;
using ProblemsMiner;

namespace lib.Brute
{
    [TestFixture]
    internal class Full_Test
    {
        public string[] AllOps = new string[]
            {
                // Unary
                "not", "shl1", "shr1", "shr4", "shr16",

                //Binary
                "and" , "or" , "xor" , "plus",

                //Special
                "if0", "fold"
            };

        [Test]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        public void TestInclude(int size)
        {
            var bruter = new Force();
            var solutions = new HashSet<string>(bruter.Solve(size - 1, AllOps).Select( expr => expr.GetUnified().ToSExpr()));

            Console.WriteLine(string.Join("\n", solutions));
           
            foreach (var problem in GetAllSamples().Where( p => p.Size == size))
            {
               CollectionAssert.Contains(solutions, problem.Solution.GetUnified().ToSExpr());
            }
            
        }

        private IEnumerable<ProblemsMiner.Problem> GetAllSamples()
        {
            var files = System.IO.Directory.GetFiles("../../../../problems-samples");
            foreach (var file in files)
            {
                var problemsDesriptions = System.IO.File.ReadAllLines(file);
                foreach (var problem in problemsDesriptions)
                {
                    yield return ProblemsMiner.Problem.ParseTraining(problem.Trim());
                }
            }
        }

    }
}
