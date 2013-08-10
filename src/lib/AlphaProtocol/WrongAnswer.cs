namespace lib.AlphaProtocol
{
	public class WrongAnswer
	{
		public ulong Arg;
		public ulong CorrectValue;
		public ulong ProvidedValue;

		public override string ToString()
		{
			return string.Format("Arg: {0}, CorrectValue: {1}, ProvidedValue: {2}", Arg, CorrectValue, ProvidedValue);
		}
	}
}