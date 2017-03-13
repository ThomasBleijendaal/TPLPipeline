using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeLine
{
	public class Pipeline : BasePipeline<Job>
	{
		public HttpClient HttpClient { get; set; } = new HttpClient();

		TransformManyBlock<IPipelineJob, IPipelineJobElement> PipelineBegin;
		TransformBlock<IPipelineJobElement, IPipelineJobElement> DownloadBlock;
		TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeBlock;
		ActionBlock<IPipelineJobElement> DiskWriteBlock;
		
		public Pipeline()
		{
			PipelineBegin = PipelineBlockFactory.StartBlock();

			DownloadBlock = TransformBlock<string, byte[]>(
				async (job, requestUri) =>
				{
					var data = await HttpClient.GetByteArrayAsync(requestUri);

					if(data == null)
					{
						;
					}

					return data;
				}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 25 });

			MergeBlock = MergeTransformBlock<byte[], byte[]>(
				(job, byteArrays) =>
				{
					var length = 0;

					foreach (var byteArray in byteArrays)
					{
						length += byteArray.Length;
					}

					var mergedArray = new byte[length];
					var index = 0;

					foreach (var byteArray in byteArrays)
					{
						byteArray.CopyTo(mergedArray, index);

						index += byteArray.Length;
					}

					return mergedArray;
				}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

			DiskWriteBlock = ActionBlock<byte[]>(
				(job, data) =>
				{
					var file = job.FileName;

					var directory = Path.GetDirectoryName(file);

					if(!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					if(File.Exists(file))
					{
						File.Delete(file);
					}

					var fileHandle = File.OpenWrite(file);
					fileHandle.Write(data, 0, data.Length);
					fileHandle.Close();
				}, true);

			PipelineBegin.LinkTo(DownloadBlock);
			DownloadBlock.LinkTo(MergeBlock);
			MergeBlock.LinkTo(DiskWriteBlock);
		}
		public override void Post(Job job)
		{
			PipelineBegin.Post(job);
		}

		public override Task PostAsync(Job job)
		{
			PipelineBegin.Post(job);
			return job.Completion;
		}
	}
}
