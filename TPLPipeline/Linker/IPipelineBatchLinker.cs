using System.Collections.Generic;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeLine
{
	public interface IPipelineBatchLinker
	{
		ITargetBlock<IPipelineJobElement> Input { get; }
		ISourceBlock<IEnumerable<IPipelineJobElement>> Output { get; }
	}
}
