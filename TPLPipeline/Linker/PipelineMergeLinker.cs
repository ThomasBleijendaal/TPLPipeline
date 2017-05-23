using System;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
    public class PipelineMergeLinker<T1, T2>
    {
        private JoinBlock<IPipelineJobElement<T1>, IPipelineJobElement<T2>> Input { get; set; }
        private ActionBlock<Tuple<IPipelineJobElement<T1>, IPipelineJobElement<T2>>> Output { get; set; }

        public PipelineMergeLinker(ISourceBlock<IPipelineJobElement<T1>> from1, ISourceBlock<IPipelineJobElement<T2>> from2, ITargetBlock<IPipelineJobElement<Tuple<T1, T2>>> to) : this(from1, from2, to, null, null) { }
        public PipelineMergeLinker(ISourceBlock<IPipelineJobElement<T1>> from1, ISourceBlock<IPipelineJobElement<T2>> from2, ITargetBlock<IPipelineJobElement<Tuple<T1, T2>>> to, Predicate<IPipelineJobElement<T1>> predicate1, Predicate<IPipelineJobElement<T2>> predicate2)
        {
            if (predicate1 == null)
            {
                predicate1 = e => true;
            }

            if (predicate2 == null)
            {
                predicate2 = e => true;
            }

            Input = new JoinBlock<IPipelineJobElement<T1>, IPipelineJobElement<T2>>();
            Output = new ActionBlock<Tuple<IPipelineJobElement<T1>, IPipelineJobElement<T2>>>(
                elements =>
                {
                    var job = elements.Item1.Job;
                    var mergedElement = job.MergeToSingleElement(elements);

                    to.Post(mergedElement);
                });

            from1.LinkTo(Input.Target1, predicate1);
            from2.LinkTo(Input.Target2, predicate2);

            Input.LinkTo(Output);
        }
    }
}
