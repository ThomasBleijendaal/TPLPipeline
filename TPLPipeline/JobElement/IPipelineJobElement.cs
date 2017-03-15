using System;

namespace TPLPipeline
{
	public interface IPipelineJobElement
	{
		IPipelineJob Job { get; }
		int Element { get; }

		string CurrentStepName { get; }
		string CompletedStepName { get; }

		void BeginStep(string stepName);
		void CompleteStep();

		T GetData<T>();
		Type GetDataType(int stepsBack);
		void SetData<T>(T value);

		void Disable();
		bool Disabled { get; }

	}
}
