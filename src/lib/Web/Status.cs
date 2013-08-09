namespace lib.Web
{
	public class Status
	{
		public class RequestWindow
		{
			public double amount;
			public double limit;
			public double resetsIn;

			public override string ToString()
			{
				return string.Format("Amount: {0}, Limit: {1}, ResetsIn: {2}", amount, limit, resetsIn);
			}
		}

		public class CpuWindow
		{
			public double amount;
			public double limit;
			public double resetsIn;

			public override string ToString()
			{
				return string.Format("Amount: {0}, Limit: {1}, ResetsIn: {2}", amount, limit, resetsIn);
			}
		};

		public int contestScore;
		public double cpuTotalTime;
		public CpuWindow cpuWindow;
		public int easyChairId;
		public int lightningScore;
		public int mismatches;
		public int numRequests;
		public RequestWindow requestWindow;
		public int trainingScore;

	}
}