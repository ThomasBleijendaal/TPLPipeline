# TPL Pipeline

A TPL Dataflow based pipeline with job based tracking.

## Example

### Pipeline layout

```
[Start]
  |
  |
[Download]
  |_____________________ 
  |                     |
  | Condition           | Condition
  |                     |
[Merge files]         [Save as image]
  |                     |
  |                     |
[Save file]             |
  |_____________________|
  |
[Finish]
```

### Pipeline class
```
public class Pipeline : BasePipeline<Job>
{
  TransformManyBlock<IPipelineJob, IPipelineJobElement> PipelineBegin;
  TransformBlock<IPipelineJobElement, IPipelineJobElement> DownloadBlock;
  TransformBlock<IEnumerable<IPipelineJobElement>, IPipelineJobElement> MergeBlock;
  TransformBlock<IPipelineJobElement, IPipelineJobElement> DiskWriteBlock;
  TransformBlock<IPipelineJobElement, IPipelineJobElement> ImageBlock;
  ActionBlock<IPipelineJobElement> FinishBlock;
  
  public Pipeline()
  {
    PipelineBegin = PipelineBlockFactory.StartBlock();

    DownloadBlock = TransformBlock<IJobElementStartData, byte[]>(
      async (job, request) =>
      {
        [..]

      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 25 });

    MergeBlock = MergeTransformBlock<byte[], byte[]>(
      (job, byteArrays) =>
      {
        [..]

      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });
      
    DiskWriteBlock = TransformBlock<byte[], bool>(
      async (job, data) =>
      {
        [..]
      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

    ImageBlock = TransformBlock<byte[], bool>(
      async (job, data) =>
      {
        [..]
      }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1 });

    FinishBlock = ActionBlock<Tuple<bool, bool>>(
      (job, flags) =>
      {
        [..]
      }, true);

    PipelineBegin.LinkTo(DownloadBlock);

    DownloadBlock.LinkTo(MergeBlock, e => e.GetDataType(1) == typeof(Website));
    DownloadBlock.LinkTo(ImageBlock, e => e.GetDataType(1) == typeof(Thumbnail));

    MergeBlock.LinkTo(DiskWriteBlock);
      
    FinishBlock.LinkFrom<bool, bool>(DiskWriteBlock, ImageBlock);
  }
}
```

### Job class
```
public class Job : BaseJob
{
  public Job() : base()
  {

  }

  public override void OnJobStart()
  {
    Console.WriteLine("Job started");
  }

  public override void OnJobComplete()
  {
    Console.WriteLine("Job completed");
  }
}
```