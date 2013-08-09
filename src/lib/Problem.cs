using System;
using System.Linq;
using NUnit.Framework;
using lib.Lang;

namespace ProblemsMiner
{
    public class Problem
    {
        public string Id { get; private set; }
        public int Size { get; private set; }
        public string[] AllOperators { get; private set; }
        public lib.Lang.Expr Solution { get; private set; }

        public string[] FoldOperators
        {
            get
            {
                string[] foldOps = AllOperators.Where(o => o == "fold" || o == "tfold").ToArray();
                return foldOps.Length > 0 ? new[] {foldOps.Single()} : new string[0];
            }
        }

        public static Problem Parse(string text)
        {
            string[] elems = text.Split('\t').Where(e => e.Length > 0).ToArray();
            if (elems.Length == 0)
                return null;
            return new Problem
                {
                    Id = elems.First(),
                    Size = int.Parse(elems.ElementAt(1)),
                    AllOperators = elems.Skip(2).ToArray()
                };
        }

        public override string ToString()
        {
            return string.Format("Id: {0}, " +
                                 "Size: {1}, " +
                                 "Solution: {2}, " +
                                 "Operations: {3}", Id, Size, Solution.ToSExpr(),
                                 string.Join(", ", AllOperators));
        }

        /// <summary>
        /// Parses problem from training format:
        ///     kev: value, key1: value
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static Problem ParseTraining(string text)
        {
            var problem = new Problem();
            var keyvalues = text.Split(new[] {", " }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var keyvalue in keyvalues)
            {
                var kv = keyvalue.Split(':');
                switch (kv[0].Trim())
                {
                    case "Id":
                        problem.Id = kv[1].Trim();
                        break;
                    case "Size":
                        problem.Size = int.Parse(kv[1]);
                        break;
                    case "Operators":
                        problem.AllOperators = kv[1].Trim().Split(',').Select( x => x.Trim()).ToArray();
                        break;
                    case "Challenge":
                        problem.Solution = Expr.ParseFunction(kv[1].Trim());
                        break;
                }
            }

            return problem;
        }

        [Test]
        public void TestParser()
        {
            Assert.AreEqual(new Problem
                {
                    Id = "rKHWXidcNEEC6BmTkAeFSeGd",
                    Size = 6,
                    AllOperators = new string[] {"shl1","shr1","shr16","shr4"},
                    Solution = Expr.ParseFunction("(lambda (x_4637) (shr16 (shr1 (shl1 (shr4 x_4637)))))")
                }.ToString(), ParseTraining("Id: rKHWXidcNEEC6BmTkAeFSeGd, Challenge: (lambda (x_4637) (shr16 (shr1 (shl1 (shr4 x_4637))))), Size: 6, Operators: shl1,shr1,shr16,shr4").ToString());
        }
    }
}