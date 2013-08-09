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
        [Test]
        [TestCase(3)]
        public void TestInclude(int size)
        {
            var bruter = new Force();
            var solutions = new HashSet<Expr>(bruter.Solve(size - 1, new[] {"not", "shl1", "shr1", "shr4", "shr16"}));
            
        }

        private IEnumerable<ProblemsMiner.Problem> GetAllSamples(int size)
        {
            var files = System.IO.Directory.GetFiles("problems-samples");
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
