using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline.TestApp
{
	public class Pipeline : BasePipeline<Job>
	{
		public HttpClient HttpClient { get; set; } = new HttpClient();

		TransformManyBlock<IPipelineJob, IPipelineJobElement> PipelineBegin;
		TransformBlock<IPipelineJobElement, IPipelineJobElement> DownloadBlock;
		TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeBlock;
		ActionBlock<IPipelineJobElement> DiskWriteBlock;
		ActionBlock<IPipelineJobElement> ImageBlock;


		public Pipeline()
		{
			PipelineBegin = PipelineBlockFactory.StartBlock();

			DownloadBlock = TransformBlock<IJobElementStartData, byte[]>(
				async (job, request) =>
				{
					var data = await HttpClient.GetByteArrayAsync(request.Url);

					if (data == null)
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

					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					if (File.Exists(file))
					{
						File.Delete(file);
					}

					var fileHandle = File.OpenWrite(file);
					fileHandle.Write(data, 0, data.Length);
					fileHandle.Close();
				}, true);

			ImageBlock = ActionBlock<byte[]>(
				(job, data) =>
				{
					var file = job.FileName + ".png";
					var directory = Path.GetDirectoryName(file);

					if (!Directory.Exists(directory))
					{
						Directory.CreateDirectory(directory);
					}

					if (File.Exists(file))
					{
						File.Delete(file);
					}

					var fileHandle = File.OpenWrite(file);
					fileHandle.Write(data, 0, data.Length);
					fileHandle.Close();
				});



			PipelineBegin.LinkTo(DownloadBlock);
			DownloadBlock.LinkTo(MergeBlock, e => e.GetDataType(1) == typeof(Website));
			DownloadBlock.LinkTo(ImageBlock, e => e.GetDataType(1) == typeof(Thumbnail));
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
