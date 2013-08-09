using System.Linq;

namespace ProblemsMiner
{
    internal class Problem
    {
        public string Id { get; private set; }
        public int Size { get; private set; }
        public string[] AllOperators { get; private set; }

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
    }
}