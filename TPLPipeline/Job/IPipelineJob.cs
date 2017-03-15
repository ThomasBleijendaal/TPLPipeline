using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TPLPipeline
{
	public interface IPipelineJob
	{
		string Id { get; }

		void Complete();
		bool IsCompleted(string stepName);
		bool IsCompleted(string stepName, Predicate<IPipelineJobElement> predicate);
		bool IsFullyBegun(string stepName);

		IEnumerable<IPipelineJobElement> MergeElements();
		IEnumerable<IPipelineJobElement> MergeElements(Predicate<IPipelineJobElement> predicate);
		IPipelineJobElement MergeToSingleElement();

		void OnJobStart();
		void OnJobComplete();

		IEnumerable<IPipelineJobElement> Elements();

		void AddData(object data);
		void AddDataRange(IEnumerable<object> data);

		Task<bool> Completion { get; }
	}
}
