using System;
using System.Collections.Generic;

namespace TPLPipeline
{
    public interface IPipelineJobElement<T> : IJobElement
    {
        T GetData();
        IPipelineJobElement<Tnew> SetData<Tnew>(Tnew value);
    }

    public interface IJobElement
    {
        List<string> Steps { get; }
        Dictionary<string, string> Properties { get; }

        IPipelineJob Job { get; }
        int Nr { get; }

        string CurrentStepName { get; }
        string CompletedStepName { get; }

        void BeginStep(string stepName);
        void CompleteStep(bool isLastStep = false);
        void Disable();
        bool Disabled { get; }
    }
}
