using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline.TestApp
{
	public class Pipeline : BasePipeline<Job>
	{
		public HttpClient HttpClient { get; set; } = new HttpClient();

		TransformManyBlock<Job, IPipelineJobElement<string>> PipelineBegin;
		TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<byte[]>> DownloadBlock;
		TransformBlock<IEnumerable<IPipelineJobElement<byte[]>>, IPipelineJobElement<byte[]>> MergeBlock;
		TransformBlock<IPipelineJobElement<byte[]>, IPipelineJobElement<bool>> DiskWriteBlock;
		TransformBlock<IPipelineJobElement<byte[]>, IPipelineJobElement<bool>> ImageBlock;
		ActionBlock<IPipelineJobElement<Tuple<bool,bool>>> FinishBlock;

		int delay = 0;
		
		public Pipeline()
		{
			PipelineBegin = PipelineBlockFactory.StartBlock<Job, string>();

			DownloadBlock = TransformBlock<string, byte[]>(
				async (job, request) =>
				{
					//var data = await HttpClient.GetByteArrayAsync(request);

					var data = " FJDKSLJF:KDSFJDSKLJF:KLDJSKJFLK:DSJJFKDSJF:JDSKJF:LKDSJLJF:DSKLJFL:KDSJLF:JDSL:JFDS";

					await Task.Delay(delay += 50);

					return Encoding.ASCII.GetBytes(data);
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
				}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

			DiskWriteBlock = TransformBlock<byte[], bool>(
				async (job, data) =>
				{
					return await WriteToFile(job.FileName, data);
				}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

			ImageBlock = TransformBlock<byte[], bool>(
				async (job, data) =>
				{
					return await WriteToFile(job.FileName + ".png", data);
				}, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

			FinishBlock = ActionBlock<Tuple<bool, bool>>(
				(job, flags) =>
				{
					if (flags.Item1)
					{
						Console.WriteLine("File successfull");
					}

					if (flags.Item2)
					{
						Console.WriteLine("Image successfull");
					}
				}, true);

			PipelineBegin.LinkTo(DownloadBlock);

			DownloadBlock.LinkTo(MergeBlock, e => e.Properties["Type"] == "Website");
			DownloadBlock.LinkTo(ImageBlock, e => e.Properties["Type"] == "Thumbnail");

			MergeBlock.LinkTo(DiskWriteBlock);
			
			FinishBlock.LinkFrom<bool, bool>(DiskWriteBlock, ImageBlock);
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

		private async Task<bool> WriteToFile(string fileName, byte[] data)
		{
			var directory = Path.GetDirectoryName(fileName);

			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}

			if (File.Exists(fileName))
			{
				File.Delete(fileName);
			}

			var fileHandle = File.OpenWrite(fileName);
			await fileHandle.WriteAsync(data, 0, data.Length);
			await Task.Delay(1000);
			fileHandle.Close();

			return true;
		}
	}
}
