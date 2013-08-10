﻿using System;
using System.Collections.Generic;
using System.IO;
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
        /*
                [TestCase(10)]
                [TestCase(11)]
                [TestCase(12)]
                [TestCase(13)]
        */
        public void TestInclude(int size)
        {
            var bruter = new Force();
            var solutions = new HashSet<string>(bruter.Solve(size - 1, AllOps).Select(expr => expr.ToSExpr()));

            //            System.IO.File.WriteAllText("allTrees.txt", string.Join("\n", solutions));

            foreach (var problem in GetAllSamples().Where(p => p.Size == size))
            {
                Assert.True(solutions.Contains(problem.Solution.GetUnified().ToSExpr()));
            }
        }
        [Test]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        [TestCase(7)]
        [TestCase(8)]
        [TestCase(9)]
        /*
                [TestCase(10)]
                [TestCase(11)]
                [TestCase(12)]
                [TestCase(13)]
        */
        public void TestIncludeBin(int size)
        {
            var bruter = new BinaryBruteForcer(AllOps);
            var solutions = new HashSet<string>(bruter.Enumerate(size - 1).Select(expr => expr.ToSExpr().Item1));

            //            System.IO.File.WriteAllText("allTrees.txt", string.Join("\n", solutions));
            Console.WriteLine("testing");
            foreach (var problem in GetAllSamples().Where(p => p.Size == size))
            {
                Assert.True(solutions.Contains(problem.Solution.GetUnified().ToSExpr()));
            }
        }

        private IEnumerable<Problem> GetAllSamples()
        {
            var files = System.IO.Directory.GetFiles("../../../../problems-samples");
            foreach (var file in files)
            {
                var problemsDesriptions = System.IO.File.ReadAllLines(file);
                foreach (var problem in problemsDesriptions)
                {
                    yield return Problem.ParseTraining(problem.Trim());
                }
            }
        }

    }
}
