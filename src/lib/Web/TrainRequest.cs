namespace lib.Web
{
	public class TrainRequest
	{
		public TrainRequest(int? size = null, string[] operators = null)
		{
			this.size = size;
			this.operators = operators;
		}

		public int? size;
		public string[] operators;
	}
}