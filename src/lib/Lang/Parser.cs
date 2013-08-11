using System;
using System.Collections.Generic;
using System.Linq;

namespace lib.Lang
{
	public class Parser
	{
		public static IEnumerable<string> Tokenize(string s)
		{
			var items = s.Split(" \t\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			var opens = items.SelectMany(i => Extensions.AlternateWith(i.Split(new[] { '(' }), "("));
			var closes = opens.SelectMany(i => i.Split(new[] { ')' }).AlternateWith(")"));
			return closes.Where(item => !string.IsNullOrEmpty(item));
		}

		public static byte[] ParseExpr(string s, string argName = null, string foldItemName = null, string foldAccName = null)
		{
			return ParseExpr(Tokenize(s + " #").GetEnumerator().MoveNextOrFail(), argName, foldItemName, foldAccName).ToArray();
		}

		public static byte[] ParseFunction(string s)
		{
			var tokens = Tokenize(s + " #").GetEnumerator().MoveNextOrFail();
			tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var varName = tokens.ExtractToken();
			tokens.SkipToken(")");
			return ParseExpr(tokens, varName, null, null).ToArray();
		}

		public static IEnumerable<byte> ParseExpr(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			if (tokens.Current == "(")
				foreach (var b in ParseCall(tokens, argName, foldItemName, foldAccName))
					yield return b;
			else
			{
				var t = tokens.ExtractToken();
				if (t == "1") yield return 1;
				else if (t == "0") yield return 0;
				else if (t == "#") throw new FormatException("wrong format");
				else if (t == foldItemName) yield return 3;
				else if (t == foldAccName) yield return 4;
				else if (t == argName) yield return 2;
				else throw new FormatException("unknown var " + t);
			}
		}

		private static IEnumerable<byte> ParseCall(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			Func<IEnumerable<byte>> parse = () => ParseExpr(tokens, argName, foldItemName, foldAccName);
			tokens.SkipToken("(");
			var t = tokens.ExtractToken();
			IEnumerable<byte> res;
			if (t == "fold")
				res = ParseFold(tokens, argName, foldItemName, foldAccName);
			else
			{
				int op = Array.IndexOf(Operations.names, t);
				if (op < 0) throw new Exception(t);
				res = ParseFun((byte)op, Operations.args[op], parse);
			}
			foreach (var b in res) yield return b;
			tokens.SkipToken(")");
		}

		private static IEnumerable<byte> ParseFun(byte fun, int argsCount, Func<IEnumerable<byte>> parse)
		{
			yield return fun;
			for (int i = 0; i < argsCount; i++)
				foreach (var b in parse()) yield return b;
		}

		private static IEnumerable<byte> ParseFold(IEnumerator<string> tokens, string argName, string foldItemName, string foldAccName)
		{
			Func<IEnumerable<byte>> parse = () => ParseExpr(tokens, argName, foldItemName, foldAccName);
			yield return 6;
			foreach (var b in parse()) yield return b;
			foreach (var b in parse()) yield return b;
			tokens.SkipToken("(").SkipToken("lambda").SkipToken("(");
			var itemName = tokens.ExtractToken();
			var accName = tokens.ExtractToken();
			tokens.SkipToken(")");
			var expr = ParseExpr(tokens, argName, itemName, accName);
			foreach (var b in expr) yield return b;
			tokens.SkipToken(")");
		}
	}
}