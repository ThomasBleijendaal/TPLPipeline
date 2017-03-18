using System;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
	public class PipelineMergeLinker<T1, T2>
	{
		private JoinBlock<IPipelineJobElement, IPipelineJobElement> Input { get; set; }
		private ActionBlock<Tuple<IPipelineJobElement, IPipelineJobElement>> Output { get; set; }

		public PipelineMergeLinker(ISourceBlock<IPipelineJobElement> from1, ISourceBlock<IPipelineJobElement> from2, ITargetBlock<IPipelineJobElement> to) : this(from1, from2, to, null, null) { }
		public PipelineMergeLinker(ISourceBlock<IPipelineJobElement> from1, ISourceBlock<IPipelineJobElement> from2, ITargetBlock<IPipelineJobElement> to, Predicate<IPipelineJobElement> predicate1, Predicate<IPipelineJobElement> predicate2)
		{
			if (predicate1 == null)
			{
				predicate1 = e => true;
			}

			if (predicate2 == null)
			{
				predicate2 = e => true;
			}

			Input = new JoinBlock<IPipelineJobElement, IPipelineJobElement>();
			Output = new ActionBlock<Tuple<IPipelineJobElement, IPipelineJobElement>>(
				elements =>
				{
					var job = elements.Item1.Job;
					job.MergeToSingleElement<T1, T2>(elements);
					to.Post(elements.Item1);
				});

			from1.LinkTo(Input.Target1, predicate1);
			from2.LinkTo(Input.Target2, predicate2);

			Input.LinkTo(Output);
		}
	}
}
