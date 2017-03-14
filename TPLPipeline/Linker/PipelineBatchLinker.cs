using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline
{
	public class PipelineBatchLinker
	{
		private ActionBlock<IPipelineJobElement> Input { get; set; }

		public PipelineBatchLinker(ISourceBlock<IPipelineJobElement> from, ITargetBlock<IEnumerable<IPipelineJobElement>> to)
		{
			Input = new ActionBlock<IPipelineJobElement>(
				element =>
				{
					var job = element.Job;

					if (job.IsCompleted(element.CurrentStep))
					{
						Console.WriteLine($"Merging job {job.Id}");

						var elements = job.MergeElements();

						if (elements != null)
						{
							to.Post(elements);
						}
					}
				});

			from.LinkTo(Input);
		}
	}
}
