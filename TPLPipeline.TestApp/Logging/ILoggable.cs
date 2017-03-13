using System.Collections.Generic;

namespace TPLPipeLine
{
	public interface ILoggable
	{
		void AddEvent(int ms, int nr, string e);
		Dictionary<string, int[]> events { get; set; }
	}
}
