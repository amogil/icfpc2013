namespace lib.Web
{
	public class TrainProblem
	{
		public string id;
		public string challenge;
		public int size;
		public string[] operators;

		public override string ToString()
		{
			return string.Format("Id: {0}, Challenge: {1}, Size: {2}, Operators: {3}", id, challenge, size, string.Join(",", operators));
		}
	}
}