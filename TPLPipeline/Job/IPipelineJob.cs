using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TPLPipeLine
{
	public interface IPipelineJob
	{
		string Id { get; }

		void Complete();
		bool IsCompleted(int stepNr);
		bool IsFullyBegun(int stepNr);

		bool IsMerged { get; }
		IEnumerable<IPipelineJobElement> MergeElements();
		IPipelineJobElement MergeToSingleElement();

		void OnJobStart();
		void OnJobComplete();

		IEnumerable<IPipelineJobElement> Elements();

		IEnumerable<object> Data { set; }
		
		Task<bool> Completion { get; }
	}
}
