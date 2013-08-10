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

		public override string ToString()
		{
			return string.Format("ContestScore: {0}, LightningScore: {1}, TrainingScore: {2}, Mismatches: {3}, NumRequests: {4}, CpuTotalTime: {5}, EasyChairId: {6}, RequestWindow: {7}, CpuWindow: {8}", contestScore, lightningScore, trainingScore, mismatches, numRequests, cpuTotalTime, easyChairId, requestWindow, cpuWindow);
		}
	}
}