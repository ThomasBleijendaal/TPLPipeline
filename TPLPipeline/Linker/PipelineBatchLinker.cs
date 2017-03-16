using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
	public class PipelineBatchLinker
	{
		private ActionBlock<IPipelineJobElement> Input { get; set; }

		public PipelineBatchLinker(ISourceBlock<IPipelineJobElement> from, ITargetBlock<IEnumerable<IPipelineJobElement>> to) : this(from, to, null) { }

		public PipelineBatchLinker(ISourceBlock<IPipelineJobElement> from, ITargetBlock<IEnumerable<IPipelineJobElement>> to, Predicate<IPipelineJobElement> predicate)
		{
			if(predicate == null)
			{
				predicate = e => true;
			}

			Input = new ActionBlock<IPipelineJobElement>(
				element =>
				{
					var job = element.Job;

					if (job.IsCompleted(element.CurrentStepName, predicate))
					{						
						var elements = job.MergeElements(predicate);

						if (elements != null)
						{
							to.Post(elements);
						}
					}
				});

			from.LinkTo(Input, predicate);
		}
	}
}
