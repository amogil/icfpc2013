﻿using System.Linq;

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

	public static class Operations
	{
		private static Operation O(string name, int argsCount=0, int size = 1)
		{
			return new Operation(name, size, argsCount);
		}

		public static Operation[] all = new[]
			{
				O("0"), O("1"), O("x"), O("i"), O("a"), 
				//5
				O("if0", 3), O("fold", 3, 2), 
				//7
				O("not", 1), O("shl1", 1), O("shr1", 1), O("shr4", 1), O("shr16", 1),
				//12
				O("and", 2), O("or", 2), O("xor", 2), O("plus", 2)
			};

		public static readonly string[] names = all.Select(op => op.Name).ToArray();
		public static readonly int[] sizes = all.Select(op => op.size).ToArray();
		public static readonly int[] args = all.Select(op => op.argsCount).ToArray();
	}
}