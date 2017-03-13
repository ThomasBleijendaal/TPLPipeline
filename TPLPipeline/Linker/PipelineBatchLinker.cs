using System;
using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeLine
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

					if (!job.IsMerged && job.IsCompleted(element.CurrentStep))
					{
						Console.WriteLine($"Merging job {job.Id}");

						var elements = job.MergeElements();
						to.Post(elements);
					}
				});

			from.LinkTo(Input);
		}
	}
}
