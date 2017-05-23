using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TPLPipeline
{
    public abstract class BaseJob : IPipelineJob
    {
        private List<IJobElement> Elements = new List<IJobElement>();
        private TaskCompletionSource<bool> CompletionTcs = new TaskCompletionSource<bool>();

        private bool Merged { get; set; }

        public Task<bool> Completion => CompletionTcs.Task;
        public string Id { get; private set; }

        private bool _completed = false;
        private object _completedLock = new object();

        bool IPipelineJob.IsCompleted(string stepName)
        {
            return Elements.All(e => e.Disabled || !e.Disabled && (e.CurrentStepName.EndsWith(stepName)));
        }
        bool IPipelineJob.IsCompleted<T>(string stepName, Predicate<IPipelineJobElement<T>> predicate)
        {
            return ((IPipelineJob)this).Elements<T>()
                ?.Where(e => predicate(e) && !e.Disabled && (e.CurrentStepName?.EndsWith(stepName) ?? false))
                .All(e => e.CompletedStepName?.EndsWith(stepName) ?? false) ?? false;
        }

        public abstract void OnJobStart();
        public abstract void OnJobComplete();

        public BaseJob()
        {
            Id = Guid.NewGuid().ToString();
        }

        void IPipelineJob.Complete(string stepName)
        {
            if (!_completed && (Elements.TrueForAll(e => e.Disabled || (e.CompletedStepName?.EndsWith(stepName) ?? false))))
            {
                var thisThreadCompleted = false;

                // TODO: remove lock
                lock (_completedLock)
                {
                    if (!_completed)
                    {
                        _completed = true;
                        thisThreadCompleted = true;
                    }
                }

                if (thisThreadCompleted)
                {
                    CompletionTcs.TrySetResult(true);
                    OnJobComplete();
                }
            }
        }

        IEnumerable<IJobElement> IPipelineJob.Elements()
        {
            return Elements;
        }

        IEnumerable<IPipelineJobElement<T>> IPipelineJob.Elements<T>()
        {
            if (Elements.TrueForAll(x => x as IPipelineJobElement<T> != null))
            {
                return Elements.Select(x => x as IPipelineJobElement<T>);
            }
            else
            {
                return null;
            }
        }

        void IPipelineJob.UpdateElement(IJobElement newElement)
        {
            Elements[newElement.Nr] = newElement;
        }

        IEnumerable<IPipelineJobElement<T>> IPipelineJob.MergeElements<T>()
        {
            return (this as IPipelineJob).MergeElements<T>(e => true);
        }
        IEnumerable<IPipelineJobElement<T>> IPipelineJob.MergeElements<T>(Predicate<IPipelineJobElement<T>> predicate)
        {
            if (!Merged)
            {
                Merged = true;
                var elements = Elements.Select(x => (IPipelineJobElement<T>)x).Where(e => predicate(e)).ToList();

                elements.GetRange(1, elements.Count - 1).ForEach(element => element.Disable());

                return elements;
            }
            else
            {
                return null;
            }
        }

        IPipelineJobElement<T> IPipelineJob.MergeToSingleElement<T>(IEnumerable<IPipelineJobElement<T>> elements)
        {
            var elementList = elements.ToList();

            foreach (var element in elementList.GetRange(1, elementList.Count - 1))
            {
                element.Disable();
            }
            return elementList.First();
        }

        IPipelineJobElement<Tuple<T1, T2>> IPipelineJob.MergeToSingleElement<T1, T2>(Tuple<IPipelineJobElement<T1>, IPipelineJobElement<T2>> elements)
        {
            var newData = Tuple.Create(elements.Item1.GetData(), elements.Item2.GetData());

            elements.Item2.Disable();
            var mergedElement = elements.Item1.SetData(newData);

            return mergedElement;
        }

        public void AddData<T>(T value, DataProperty[] properties = null)
        {
            Elements.Add(new JobElement<T>(this, Elements.Count, value, properties));
        }

    }
}
