using System.Linq;

namespace lib.Lang
{
	public class Operation
	{
		public Operation(string name, int priority, int size, int argsCount)
		{
			Name = name;
			this.priority = priority;
			this.size = size;
			this.argsCount = argsCount;
		}

		public override string ToString()
		{
			return Name;
		}

		public string Name;
		public readonly int priority;
		public int size;
		public int argsCount;
	}

	public static class Operations
	{
		private static Operation O(string name, int priority, int argsCount=0, int size = 1)
		{
			return new Operation(name, priority, size, argsCount);
		}

		public static Operation[] all = new[]
			{
				O("0", 1), O("1", 2), O("x", 3), O("i", 4), O("a", 5), 
				//5
				O("if0", 31, 3), O("fold", 32, 3, 2), 
				//7
				O("not", 11, 1), O("shl1", 12, 1), O("shr1", 13, 1), O("shr4", 14, 1), O("shr16", 15, 1),
				//12
				O("and", 21, 2), O("or", 22, 2), O("xor", 23, 2), O("plus", 24, 2)
			};

		public const int If = 5;
		public const int Fold = 6;
		public const int X = 2;
		public const int I = 3;
		public const int A = 4;

		public static readonly string[] names = all.Select(op => op.Name).ToArray();
		public static readonly int[] sizes = all.Select(op => op.size).ToArray();
		public static readonly int[] args = all.Select(op => op.argsCount).ToArray();
	}
}