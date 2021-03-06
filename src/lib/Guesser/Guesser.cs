﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using lib.Brute;
using lib.Lang;

namespace lib.Guesser
{
    internal class Guesser
    {
        public static IEnumerable<byte[]> Guess(IEnumerable<byte[]> formulas, ulong[] inputs, ulong[] outputs)
        {
            if (inputs.Length != outputs.Length)
                throw new ArgumentException();
            return
                formulas.AsParallel()
                        .WithDegreeOfParallelism(Environment.ProcessorCount*2)
                        .Where(e => IsSolve(e, inputs, outputs));
        }

        private static bool IsSolve(byte[] e, IEnumerable<ulong> inputs, ulong[] outputs)
        {
            return !inputs.Where((t, i) => e.Eval(t) != outputs[i]).Any();
        }
    }

    [TestFixture]
    public class Guesser_Test
    {
        [TestCase("(lambda (x) (not x))", 3, new[] {"not"}, 1)]
        [TestCase("(lambda (x) (shr4 x))", 3, new[] {"shr4"}, 1)]
        [TestCase("(lambda (x) (plus x 1))", 4, new[] {"plus"}, 2)]
        [TestCase("(lambda (x) (and (xor (shr4 x) x) x))", 7, new[] {"and", "shr4", "xor"}, 8)]
        [TestCase("(lambda (x) (plus (or 1 x) (shl1 x)))", 7, new[] {"or", "plus", "shl1"}, 8)]
        [TestCase("(lambda (x_9692) (if0 (and (shr16 (xor 0 x_9692)) 1) 1 x_9692))",
            10, new[] {"and", "if0", "shr16", "xor"}, 52)]
        [TestCase("(lambda (x_6107) (fold x_6107 (and 1 0) (lambda (x_6108 x_6109) (if0 x_6109 x_6108 x_6109))))",
            11, new[] {"and", "fold", "if0"}, 9)]
//        [TestCase(
//            "(lambda (x_16756) (shl1 (fold (plus 0 x_16756) 1 (lambda (x_16757 x_16758) (not (and x_16757 x_16758)))))",
//            12, new[] {"and", "fold", "not", "plus", "shl1"}, 68)]
//        [TestCase(
//            "(lambda (x_19856) (fold (xor (shr16 1) x_19856) x_19856 (lambda (x_19857 x_19858) (not (or x_19858 x_19857)))))"
//            , 12, new[] {"fold", "not", "or", "shr16", "xor"}, 68)]
//        [TestCase(
//            "(lambda (x_13969) (shr16 (shr1 (xor (if0 (and x_13969 (plus x_13969 x_13969)) 1 0) x_13969))))",
//            13, new[] {"and", "if0", "plus", "shr1", "shr16", "xor"}, 9999)]
        public void Test(string function, int size, string[] operations, int equalFormulas)
        {
            Expr formula = Expr.ParseFunction(function);
            var random = new Random();

            IEnumerable<byte[]> trees = new BinaryBruteForcer(operations).Enumerate(size - 1);
            ulong[] inputs = Enumerable.Range(1, 256).Select(e => random.NextUInt64()).ToArray();
            ulong[] outputs = inputs.Select(i => formula.Eval(new Vars(i))).ToArray();
            trees = Guesser.Guess(trees, inputs, outputs);
            Assert.AreEqual(equalFormulas, trees.Count());
        }
    }
}