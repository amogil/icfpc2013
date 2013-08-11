﻿using System;
using System.Linq;
using NUnit.Framework;

namespace lib.Lang
{
	public static class MasksExtensions
	{
		public static bool Bit(this ulong bits, int i)
		{
			return ((bits >> i) & 1) == 1;
		}

		public static ulong SetBit(this ulong bits, int i)
		{
			return bits | ((ulong)1 << i);
		}

		public static ulong UnsetBit(this ulong bits, int i)
		{
			return bits & ~((ulong)1 << i);
		}

		public static TRes Eval<TRes>(this byte[] program, Func<byte, TRes[], TRes> process, TRes missing)
		{
			int offset;
			return Eval(program, process, missing, 0, program.Length-1, out offset);
		}

		public static TRes Eval<TRes>(this byte[] program, Func<byte, TRes[], TRes> process, TRes missing, int start, int lastIndex, out int offset)
		{
			if (start > lastIndex)
			{
				offset = start;
				return missing;
			}
			byte code = program[start];
			var st = start + 1;
			int argsCount = Operations.all[code].argsCount;
			var items = new TRes[argsCount];
			for (int i = 0; i < argsCount; i++)
			{
				items[i] = program.Eval(process, missing, st, lastIndex, out offset);
				st = offset;
			}
			offset = st;
			return process(code, items);
		}

		public static int Size(this byte[] program)
		{
			return program.Eval((b, items) => items.Sum() + Operations.all[b].size, 0);
		}

		public static int SizeOfIncomplete(this byte[] program)
		{
			return program.Eval((b, items) => items.Sum() + Operations.all[b].size, 1);
		}

		public static Mask GetMask(this byte[] program, ulong x, int start = 0, int lastIndex = -1)
		{
			int offset;
			if (lastIndex == -1) lastIndex = program.Length - 1;
			var xMask = new Mask(new[] { x });
			return program.Eval((f, args) => CalcMask(f, args, xMask), Mask.X, start, lastIndex, out offset);
		}

		public static Mask GetMask(this byte[] program, int start = 0, int lastIndex = -1)
		{
			int offset;
			if (lastIndex == -1) lastIndex = program.Length - 1;
			return program.Eval((f, args) => CalcMask(f, args), Mask.X, start, lastIndex, out offset);
		}

		private static Mask CalcMask(byte f, Mask[] args, Mask x = null)
		{
			if (f == 0) return Mask._0;
			if (f == 1) return Mask._1;
			if (f == 2) return x ?? Mask.X;
			//TODO: fold
			if (f == 5)
			{
				if (args[0].CantBeZero()) return args[2];
				if (args[0].IsZero()) return args[1];
				return args[1].Union(args[2]);
			}
			if (f == 6)
			{
				return args[2];
			}
			if (f == 7) return args[0].Not();
			if (f == 8) return args[0].ShiftLeft(1);
			if (f == 9) return args[0].ShiftRight(1);
			if (f == 10) return args[0].ShiftRight(4);
			if (f == 11) return args[0].ShiftRight(16);
			if (f == 12) return args[0].And(args[1]);
			if (f == 13) return args[0].Or(args[1]);
			if (f == 14) return args[0].Xor(args[1]);
			//if (f == 15) return args[0].Plus(args[1]);
			return Mask.X;
		}
	}

	[TestFixture]
	public class Masks_Test
	{
		[Test]
		public void TestSize()
		{
			Assert.AreEqual(3, Parser.ParseExpr("(and 1 0)").Size());
			Assert.AreEqual(7, Parser.ParseExpr("(fold 0 1 (lambda (i a) (plus i a)))").Size());
			Assert.AreEqual(5, new byte[]{6}.SizeOfIncomplete());
			Assert.AreEqual(1, new byte[]{1}.SizeOfIncomplete());
			Assert.AreEqual(2, new byte[]{7}.SizeOfIncomplete());
			Assert.AreEqual(3, new byte[]{15}.SizeOfIncomplete());
		}
	}
}
