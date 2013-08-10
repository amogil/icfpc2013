using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using lib.Lang;
using lib.Web;

namespace lib.Brute
{
    public class BinaryBruteForcer
    {
        private readonly Mask answersMask;
        private readonly int a;
        private readonly int b;
        private readonly byte[] outsideFoldOperations;
        private readonly byte[] noFoldOperations;
        private readonly byte[] inFoldOperations;
        private bool tFold;
        private const int outsideFold = 0;
        private const int insideFold = 1;
        private const int insideFoldFunc = 2;

        public BinaryBruteForcer(params string[] ops)
            : this(null, 1, 1, ops)
        {

        }

        public BinaryBruteForcer(Mask answersMask, params string[] ops)
            : this(answersMask, 3, 1, ops)
        {
        }

        public BinaryBruteForcer(Mask answersMask, int a, int b, params string[] ops)
        {
            this.answersMask = answersMask;
            this.a = a;
            this.b = b;
            tFold = ops.Contains("tfold");
            outsideFoldOperations =
                new byte[] {0, 1, 2}.Concat(
                    ops.Where(t => t != "tfold").Select(o => (byte) Array.IndexOf(Operations.names, o))).ToArray();
            noFoldOperations = outsideFoldOperations.Where(o => o != 6).ToArray();
            inFoldOperations = new byte[] {3, 4}.Concat(noFoldOperations).ToArray();
        }

        public IEnumerable<byte[]> Enumerate(int size)
        {
            if (tFold)
            {
                var prefix = new byte[30];
                prefix[0] = 6;
                var prefixSize = 1;
                return Enumerate(size - 2, 3, outsideFoldOperations, prefix, prefixSize, insideFold, 1);
            }
            else return Enumerate(size, 1, outsideFoldOperations, new byte[30], 0);
        }

        private bool EnoughSizeForOps(byte[] operations, int size, int freePlaces, int usedOps)
        {
            int needSize = freePlaces +
                           operations.Where(opIndex => opIndex > 4 && (usedOps & (1 << opIndex)) == 0)
                                     .Sum(
                                         opIndex => Operations.all[opIndex].size + Operations.all[opIndex].argsCount - 1);
            return needSize <= size;
        }

        private IEnumerable<byte[]> Enumerate(
            int size, int freePlaces,
            byte[] operations,
            byte[] prefix, int prefixSize, int foldPosition = 0, int foldPlaces = 0, int usedOps = 0)
        {
            if (!EnoughSizeForOps(operations, size, freePlaces, usedOps))
                yield break;
            if (size == 0 && freePlaces == 0)
            {
                var res = new byte[prefixSize];
                Array.Copy(prefix, res, prefixSize);
                //				res.ToSExpr();
                //				Debug.Assert(res.Count(b => b == 6) <= 1);
                yield return res;
            }
            else
            {
                if (answersMask != null && prefixSize > 0 && (prefixSize + b)%a == 0 &&
                    !answersMask.IncludedIn(prefix.GetMask(0, prefixSize - 1)))
                {
                    yield break;
                }
                var newOperations = operations;
                if (foldPosition == insideFold && freePlaces == foldPlaces)
                {
                    foldPosition = insideFoldFunc;
                    newOperations = inFoldOperations;
                }
                else if (foldPosition == insideFoldFunc && freePlaces == foldPlaces - 1)
                {
                    foldPosition = outsideFold;
                    newOperations = noFoldOperations;
                }
                foreach (var opIndex in newOperations)
                {
                    Operation op = Operations.all[opIndex];
                    if (freePlaces - 1 + op.argsCount == 0 && op.size != size) continue;
                    if (freePlaces - 1 + op.argsCount > size - op.size) continue;

                    var recOperations = newOperations;
                    if (foldPosition == insideFold && freePlaces == foldPlaces)
                    {
                        foldPosition = insideFoldFunc;
                        recOperations = inFoldOperations;
                    }
                    else if (foldPosition == insideFoldFunc && freePlaces == foldPlaces - 1)
                    {
                        foldPosition = outsideFold;
                        recOperations = noFoldOperations;
                    }
                    if (opIndex == 6)
                    {
                        foldPosition = insideFold;
                        foldPlaces = freePlaces;
                    }
                    prefix[prefixSize] = opIndex;
                    foreach (var expr in
                        Enumerate(
                            size - op.size,
                            freePlaces - 1 + op.argsCount,
                            opIndex == 6 ? noFoldOperations : recOperations,
                            prefix, prefixSize + 1,
                            foldPosition, foldPlaces, usedOps | (1 << opIndex)))
                        yield return expr;
                    if (opIndex == 6)
                    {
                        foldPosition = outsideFold;
                        foldPlaces = freePlaces;
                    }
                }
            }
        }

    }
    [TestFixture]
    public class BinaryBruteForcer_Test
    {
        [TestCase(3, "or not", 0)]
        [TestCase(4, "or not", 27)]
        [TestCase(5, "shr4 if0", 111)]
        [TestCase(3, "fold not", 3)]
        [TestCase(5, "fold not", 0)]
        [TestCase(8, "or not fold", 10407)]
        [TestCase(9, "or fold", 14427)]
        [TestCase(9, "not shl1 fold", 25968)]
        [TestCase(10, "fold not shr1 shr4", 671409)]
        [TestCase(10, "fold not or shr1 shr4", 5465043)]
        [TestCase(7, "plus shr16", 0)]
        public void TestAll(int size, string ops, int expectedCount)
        {
            var force = new BinaryBruteForcerOld(ops.Split(' '));
            Stopwatch sw = Stopwatch.StartNew();
            var all = force.Enumerate(size);
            Console.WriteLine("new calculated, size = {0}", all.Count());
            //Assert.AreEqual(expectedCount, all.Count());
            Console.WriteLine(sw.Elapsed);

            //            Console.WriteLine("All:");
            //            foreach (var ar in all)
            //                Console.WriteLine(ar.Printable());

            var force2 = new BinaryBruteForcer(ops.Split(' '));
            var all2 = force2.Enumerate(size);
            Console.WriteLine("new optim calculated, size = {0}", all2.Count());

            var setOfAll = new HashSet<string>(all2.Select(t => t.Printable()));
            var diff = all.Where(t => !setOfAll.Contains(t.Printable()));
            foreach (var ar in diff.Take(50))
                Console.WriteLine(ar.Printable());
        }

        [Test]
        public void Test1()
        {
            IEnumerable<byte[]> items = new BinaryBruteForcerOld("xor", "not").Enumerate(13);
            Console.WriteLine(items.Count());
            //			foreach (var item in items)
            //			{
            //				Console.WriteLine(item.Printable());
            //			}
        }


        [TestCase(
            "Id: Q8CaRCOpwfImQx4ZhFTW3b82, Challenge: (lambda (x_3842) (or (shr4 x_3842) 1)), Size: 5, Operators: or,shr4"
            )]
        [TestCase(
            "Id: ErkXhpfABEVgAziRBgNyAAl7, Challenge: (lambda (x_11363) (shr1 (if0 (and (shr16 (shr4 x_11363)) (shr4 0)) x_11363 x_11363))), Size: 11, Operators: and,if0,shr1,shr16,shr4"
            )]
        [TestCase(
            "Id: h8tm9Awt9fVxIpGAyJ49eA4J, Challenge: (lambda (x_10772) (shl1 (if0 (or (not (shl1 0)) (shr4 x_10772)) 0 x_10772))), Size: 11, Operators: if0,not,or,shl1,shr4"
            )]
        [TestCase(
            "Id: AYTI87tdqN0qYuvM50gpb5UU, Challenge: (lambda (x_11454) (shr16 (if0 (and (shr4 (shr16 (shr1 x_11454))) x_11454) 1 x_11454))), Size: 11, Operators: and,if0,shr1,shr16,shr4"
            )]
        [TestCase(
            "Id: tP03Ueo4RbjUr7rTS5fP15Ym, Challenge: (lambda (x_10478) (shr4 (if0 (and (not (shr16 (shl1 1))) x_10478) 1 x_10478))), Size: 11, Operators: and,if0,not,shl1,shr16,shr4"
            )]
        [TestCase(
            "Id: 8xejannY4A6qJ4Jnq1AW3F1C, Challenge: (lambda (x_11226) (not (if0 (and (plus (shl1 x_11226) x_11226) x_11226) 0 x_11226))), Size: 11, Operators: and,if0,not,plus,shl1"
            )]
        [TestCase(
            "Id: sSV8yJ0TfPaiUMWDBT5vHAKN, Challenge: (lambda (x_10869) (shl1 (if0 (and (or x_10869 (shr16 x_10869)) x_10869) x_10869 x_10869))), Size: 11, Operators: and,if0,or,shl1,shr16"
            )]
        [TestCase(
            "Id: BEBwd0AADByfBOYfXG8JH23E, Challenge: (lambda (x_13034) (and (if0 (xor (shr16 (not (not x_13034))) 1) 0 1) x_13034)), Size: 12, Operators: and,if0,not,shr16,xor"
            ,
            TestName = "12 1")]
        [TestCase(
            "Id: lHObnL0KiFVC3c8vPkDDzAUc, Challenge: (lambda (x_12074) (shr4 (shr4 (if0 (or (shr16 x_12074) 1) (shl1 (shr1 x_12074)) x_12074)))), Size: 12, Operators: if0,or,shl1,shr1,shr16,shr4"
            ,
            TestName = "12 2")]
        public void MeasureMaskOptimization(string line)
        {
            var trainProblem = TrainResponse.Parse(line);
            byte[] p = Parser.ParseFunction(trainProblem.challenge);
            var ans = p.Printable();

            var operations = trainProblem.operators;
            var random = new Random();
            var answers = Enumerable.Range(0, 100).Select(i => random.NextUInt64()).Select(p.Eval).ToArray();
            var mask = new Mask(answers);
            Console.WriteLine("          answers mask: {0}", mask);
            Console.WriteLine("challenge program mask: {0}", p.GetMask());
            Console.WriteLine("answer: {0}", ans);

            var sw = Stopwatch.StartNew();
            Assert.IsTrue(new BinaryBruteForcer(mask, 3, 1, operations).Enumerate(trainProblem.size - 1)
                                                                       .Select(f => f.Printable())
                                                                       .Any(found => found == ans));
            Console.WriteLine(sw.Elapsed);
            sw.Restart();
            Assert.IsTrue(
                new BinaryBruteForcer(operations).Enumerate(trainProblem.size - 1)
                                                 .Any(found => found.Printable() == ans));
            Console.WriteLine(sw.Elapsed);
        }

        [Test]
        public void Test()
        {
            IEnumerable<byte[]> items = new BinaryBruteForcer("xor", "not").Enumerate(13);
            Console.WriteLine(items.Count());
            //			foreach (var item in items)
            //			{
            //				Console.WriteLine(item.Printable());
            //			}
        }

        [Test]
        [TestCase("shr1", "shr4")]
        [TestCase("not", "and", "or", "not")]
        [TestCase("not", "x", "not", "x", "xor", "not")]
        [TestCase("not", "shr1", "not", "shr1", "shr4", "not", "fold", "shr1", "not", "shr16", "if0", "not")]
        [TestCase("0", "1", "0", "1", "0", "1", "0", "1", "0", "1", "0", "1", "0", "1", "0", "1", "0", "1")]
        [TestCase("fold", "fold", "fold", "fold", "fold", "if0", "if0", "if0", "if0", "if0", "fold", "fold", "if0",
            "if0")]
        public void TestSpeed(params string[] ops)
        {
            byte[] outsideFoldOperations = new byte[0];
            byte[] operationsIndexes = new byte[0];
            var stopwatch = Stopwatch.StartNew();
            for (var j = 0; j < 100000; j++)
            {
                outsideFoldOperations = new byte[] { 0, 1, 2 }.Concat(
                    ops.Join(Operations.names.Select((s, i) => Tuple.Create(s, i)),
                             i => i,
                             o => o.Item1,
                             (inner, outer) => (byte)outer.Item2)
                    ).ToArray();
            }
            stopwatch.Stop();
            var oldTime = stopwatch.Elapsed;

            stopwatch.Restart();
            for (var j = 0; j < 100000; j++)
            {
                operationsIndexes = new byte[] { 0, 1, 2 }.Concat(
                    ops.Select(o => (byte)Array.IndexOf(Operations.names, o))
                    ).ToArray();
            }
            stopwatch.Stop();
            var newTime = stopwatch.Elapsed;
            CollectionAssert.AreEqual(outsideFoldOperations, operationsIndexes);
            Console.Out.WriteLine("Old: " + oldTime + " new: " + newTime);
        }
    }
}