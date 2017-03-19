using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TPLPipeline
{
	public interface IPipelineJob
	{
		string Id { get; }

		void Complete(string stepName);
		bool IsCompleted<T>(string stepName);
		bool IsCompleted<T>(string stepName, Predicate<IPipelineJobElement<T>> predicate);

		IEnumerable<IPipelineJobElement<T>> MergeElements<T>();
		IEnumerable<IPipelineJobElement<T>> MergeElements<T>(Predicate<IPipelineJobElement<T>> predicate);
		IPipelineJobElement<T> MergeToSingleElement<T>(IEnumerable<IPipelineJobElement<T>> elements);
		IPipelineJobElement<Tuple<T1, T2>> MergeToSingleElement<T1, T2>(Tuple<IPipelineJobElement<T1>, IPipelineJobElement<T2>> elements);

		void OnJobStart();
		void OnJobComplete();

		IEnumerable<IJobElement> Elements();
		IEnumerable<IPipelineJobElement<T>> Elements<T>();
		void UpdateElement(IJobElement newElement);

		void AddData<T>(T data, DataProperty[] properties = null);

		Task<bool> Completion { get; }
	}
}
