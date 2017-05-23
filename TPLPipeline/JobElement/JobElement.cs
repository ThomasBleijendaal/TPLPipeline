using System;
using System.Collections.Generic;

namespace TPLPipeline
{
    public class JobElement<T> : IPipelineJobElement<T>
    {
        public IPipelineJob Job { get; private set; }
        public int Nr { get; private set; }

        public T Data;
        public List<string> Steps { get; private set; } = new List<string>();

        public Dictionary<string, string> Properties { get; private set; } = new Dictionary<string, string>();

        private string _currentStepName = "";
        public string CurrentStepName
        {
            get
            {
                return _currentStepName;
            }
            set
            {
                _currentStepName = $"{_currentStepName}_{value}";

                Data = default(T);
                Steps.Add(_currentStepName);
            }
        }

        public string CompletedStepName { get; set; }

        public bool Disabled { get; private set; } = false;


        public JobElement(IPipelineJob job, int element, T initData, DataProperty[] properties)
        {
            Job = job;
            Nr = element;

            CurrentStepName = "init";

            if (properties != null)
            {
                foreach (var property in properties)
                {
                    Properties.Add(property.Name, property.Value);
                }
            }

            Data = initData;
        }

        public JobElement(IJobElement element, T newData)
        {
            Job = element.Job;
            Nr = element.Nr;

            _currentStepName = element.CurrentStepName;
            CompletedStepName = element.CompletedStepName;
            Properties = element.Properties;

            Steps = element.Steps;

            Data = newData;

            Job.UpdateElement(this);
        }

        public T GetData()
        {
            return Data;
        }

        public IPipelineJobElement<Tnew> SetData<Tnew>(Tnew value)
        {
            if (Disabled)
            {
                throw new Exception("Called SetData on disabled JobElement. This happens when JobElements are mutated after the job has been merged.");
            }

            return new JobElement<Tnew>(this, value);
        }

        public void BeginStep(string stepName)
        {
            CurrentStepName = stepName;
        }

        public void CompleteStep(bool isLastStep = false)
        {
            CompletedStepName = CurrentStepName;

            if (isLastStep && Job.IsCompleted(CompletedStepName))
            {
                Job.Complete(CompletedStepName);
            }
        }

        public void Disable()
        {
            Disabled = true;
        }
    }

}
