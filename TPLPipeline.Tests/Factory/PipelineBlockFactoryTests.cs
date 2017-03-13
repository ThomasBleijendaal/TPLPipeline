using Microsoft.VisualStudio.TestTools.UnitTesting;
using TPLPipeLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPLPipeLine.Tests
{
	[TestClass()]
	public class PipelineBlockFactoryTests
	{
		[TestMethod()]
		public void TransformBlockTest()
		{
			var block = PipelineBlockFactory.TransformBlock<IPipelineJob, string, string>((job, input) => input);
			Assert.IsNotNull(block);
		}
	}
}