using System;
using System.Collections.Generic;

namespace TPLPipeline
{
	public class JobElement : IPipelineJobElement
	{
		public IPipelineJob Job { get; private set; }
		public int Element { get; private set; }

		private object Data;
		private List<string> Steps = new List<string>();
		private Dictionary<string, Type> StepTypes = new Dictionary<string, Type>();

		private string _CurrentStepName = "";
		public string CurrentStepName
		{
			get
			{
				return _CurrentStepName;
			}
			set
			{
				_CurrentStepName = $"{_CurrentStepName}_{value}";

				Console.WriteLine($"{Element} {_CurrentStepName}");

				Data = null;
				Steps.Add(_CurrentStepName);
			}
		}
		
		public string CompletedStepName { get; set; }

		public bool Disabled { get; private set; } = false;
		

		public JobElement(IPipelineJob job, int element, object initData)
		{
			Job = job;
			Element = element;

			CurrentStepName = "init";

			((IPipelineJobElement)this).SetData(initData);
		}

		T IPipelineJobElement.GetData<T>()
		{
			if (Data is T data)
			{
				return data;
			}
			else
			{
				throw new Exception($"Could not get correct type of data for this step. (Step {CompletedStepName}, Requested type {typeof(T)}, Stored data type {Data.GetType()})");
			}
		}

		Type IPipelineJobElement.GetDataType(int stepsBack)
		{
			var step = Steps[Steps.Count - (stepsBack + 1)];

			if(Data == null)
			{
				return typeof(void);
			}
			else
			{
				return StepTypes[step];
			}
		}

		void IPipelineJobElement.SetData<T>(T value)
		{
			if (Disabled)
			{
				throw new Exception("Called SetData on disabled JobElement. This happens when JobElements are mutated after the job has been merged.");
			}
			Data = value;
			StepTypes[CurrentStepName] = value.GetType();
		}

		void IPipelineJobElement.BeginStep(string stepName)
		{
			CurrentStepName = stepName;
		}

		void IPipelineJobElement.CompleteStep()
		{
			CompletedStepName = CurrentStepName;
		}

		void IPipelineJobElement.Disable()
		{
			Disabled = true;
		}
	}
}
