namespace lib.Lang
{
	public class Operation
	{
		public Operation(string name, int size, int argsCount)
		{
			Name = name;
			this.size = size;
			this.argsCount = argsCount;
		}

		public override string ToString()
		{
			return Name;
		}

		public string Name;
		public int size;
		public int argsCount;
	}

	public class Operations
	{
		private static Operation O(string name, int argsCount=0, int size = 1)
		{
			return new Operation(name, size, argsCount);
		}

		public static Operation[] all = new[]
			{
				O("0"), O("1"), O("x"), O("i"), O("a"), O("if0", 3), O("fold", 3, 2), O("not", 1), O("shl1", 1), O("shr1", 1), O("shr4", 1), O("shr16", 1),
				O("and", 2), O("or", 2), O("xor", 2), O("plus", 2)
			};

		public static string[] names = new[] { "0", "1", "x", "i", "a", "if0", "fold", "not", "shl1", "shr1", "shr4", "shr16", "and", "or", "xor", "plus" };
		public static int[] sizes = new[] { 1, 1, 1, 1, 1, 1, 2, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
		public static int[] args = new[] { 0, 0, 0, 0, 0, 3, 3, 1, 1, 1, 1, 1, 2, 2, 2, 2 };
	}
}