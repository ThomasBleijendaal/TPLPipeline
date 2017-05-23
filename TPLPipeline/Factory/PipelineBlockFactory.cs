using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
    public static class PipelineBlockFactory
    {
        public static string NewBlockName()
        {
            return Guid.NewGuid().ToString();
        }

        public static TransformManyBlock<Tjob, IPipelineJobElement<Tout>> StartBlock<Tjob, Tout>()
            where Tjob : IPipelineJob
        {
            return new TransformManyBlock<Tjob, IPipelineJobElement<Tout>>(
                job =>
                {
                    job.OnJobStart();

                    return job.Elements<Tout>();
                });
        }

        public static TransformBlock<IPipelineJobElement<Tin>, IPipelineJobElement<Tout>> TransformBlock<Tjob, Tin, Tout>(Func<Tjob, Tin, Tout> action, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new TransformBlock<IPipelineJobElement<Tin>, IPipelineJobElement<Tout>>(
                element =>
                {
                    var job = (Tjob)element.Job;
                    var data = element.GetData();

                    element.BeginStep(blockName);

                    var newElement = element.SetData(action(job, data));
                    newElement.CompleteStep();

                    return newElement;
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static TransformBlock<IPipelineJobElement<Tin>, IPipelineJobElement<Tout>> TransformBlock<Tjob, Tin, Tout>(Func<Tjob, Tin, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new TransformBlock<IPipelineJobElement<Tin>, IPipelineJobElement<Tout>>(
                async element =>
                {
                    var job = (Tjob)element.Job;
                    var data = element.GetData();

                    element.BeginStep(blockName);

                    var newElement = element.SetData(await action(job, data));
                    newElement.CompleteStep();

                    return newElement;
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static TransformBlock<IEnumerable<IPipelineJobElement<Tin>>, IPipelineJobElement<Tout>> MergeTransformBlock<Tjob, Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Tout> action, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new TransformBlock<IEnumerable<IPipelineJobElement<Tin>>, IPipelineJobElement<Tout>>(
                elements =>
                {
                    var job = elements.GetJob<Tjob>();
                    var data = elements.GetData();

                    foreach (var element in elements)
                    {
                        element.BeginStep(blockName);
                    }

                    var newData = action(job, data);

                    var mergedElement = job.MergeToSingleElement(elements);
                    var newElement = mergedElement.SetData(newData);

                    newElement.CompleteStep();

                    return newElement;
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static TransformBlock<IEnumerable<IPipelineJobElement<Tin>>, IPipelineJobElement<Tout>> MergeTransformBlock<Tjob, Tin, Tout>(Func<Tjob, IEnumerable<Tin>, Task<Tout>> action, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new TransformBlock<IEnumerable<IPipelineJobElement<Tin>>, IPipelineJobElement<Tout>>(
                async elements =>
                {
                    var job = elements.GetJob<Tjob>();
                    var data = elements.GetData();

                    foreach (var element in elements)
                    {
                        element.BeginStep(blockName);
                    }

                    var newData = await action(job, data);

                    var mergedElement = job.MergeToSingleElement(elements);
                    var newElement = mergedElement.SetData(newData);

                    newElement.CompleteStep();

                    return newElement;
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static ActionBlock<IPipelineJobElement<Tin>> ActionBlock<Tjob, Tin>(Action<Tjob, Tin> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new ActionBlock<IPipelineJobElement<Tin>>(
                element =>
                {
                    var job = (Tjob)element.Job;
                    var data = element.GetData();

                    element.BeginStep(blockName);
                    action(job, data);
                    element.CompleteStep(isLastStep);
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static ActionBlock<IPipelineJobElement<Tin>> ActionBlock<Tjob, Tin>(Func<Tjob, Tin, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new ActionBlock<IPipelineJobElement<Tin>>(
                async element =>
                {
                    var job = (Tjob)element.Job;
                    var data = element.GetData();

                    element.BeginStep(blockName);
                    await action(job, data);
                    element.CompleteStep(isLastStep);
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static ActionBlock<IEnumerable<IPipelineJobElement<Tin>>> MergeActionBlock<Tjob, Tin>(Action<Tjob, IEnumerable<Tin>> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new ActionBlock<IEnumerable<IPipelineJobElement<Tin>>>(
                elements =>
                {
                    var job = elements.GetJob<Tjob>();
                    var data = elements.GetData();

                    foreach (var element in elements)
                    {
                        element.BeginStep(blockName);
                    }

                    action(job, data);

                    foreach (var element in elements)
                    {
                        element.CompleteStep(isLastStep);
                    }
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static ActionBlock<IEnumerable<IPipelineJobElement<Tin>>> MergeActionBlock<Tjob, Tin>(Func<Tjob, IEnumerable<Tin>, Task> action, bool isLastStep = false, ExecutionDataflowBlockOptions options = null)
            where Tjob : IPipelineJob
        {
            var blockName = NewBlockName();

            return new ActionBlock<IEnumerable<IPipelineJobElement<Tin>>>(
                async elements =>
                {
                    var job = elements.GetJob<Tjob>();
                    var data = elements.GetData();

                    foreach (var element in elements)
                    {
                        element.BeginStep(blockName);
                    }

                    await action(job, data);

                    foreach (var element in elements)
                    {
                        element.CompleteStep(isLastStep);
                    }
                }, options ?? new ExecutionDataflowBlockOptions());
        }

        public static void LinkTo<T>(this ISourceBlock<IPipelineJobElement<T>> element, ITargetBlock<IEnumerable<IPipelineJobElement<T>>> target)
        {
            new PipelineBatchLinker<T>(element, target);
        }

        public static void LinkTo<T>(this ISourceBlock<IPipelineJobElement<T>> element, ITargetBlock<IEnumerable<IPipelineJobElement<T>>> target, Predicate<IPipelineJobElement<T>> predicate)
        {
            new PipelineBatchLinker<T>(element, target, predicate);
        }

        public static void LinkFrom<T1, T2>(this ITargetBlock<IPipelineJobElement<Tuple<T1, T2>>> element, ISourceBlock<IPipelineJobElement<T1>> from1, ISourceBlock<IPipelineJobElement<T2>> from2)
        {
            new PipelineMergeLinker<T1, T2>(from1, from2, element);
        }

        public static void LinkFrom<T1, T2>(this ITargetBlock<IPipelineJobElement<Tuple<T1, T2>>> element, ISourceBlock<IPipelineJobElement<T1>> from1, ISourceBlock<IPipelineJobElement<T2>> from2, Predicate<IPipelineJobElement<T1>> predicate1, Predicate<IPipelineJobElement<T2>> predicate2)
        {
            new PipelineMergeLinker<T1, T2>(from1, from2, element, predicate1, predicate2);
        }
    }
}
