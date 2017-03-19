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
  TransformManyBlock<Job, IPipelineJobElement<string>> PipelineBegin;
  TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<byte[]>> DownloadBlock;
  TransformBlock<IEnumerable<IPipelineJobElement<byte[]>>, IPipelineJobElement<byte[]>> MergeBlock;
  TransformBlock<IPipelineJobElement<byte[]>, IPipelineJobElement<bool>> DiskWriteBlock;
  TransformBlock<IPipelineJobElement<byte[]>, IPipelineJobElement<bool>> ImageBlock;
  ActionBlock<IPipelineJobElement<Tuple<bool,bool>>> FinishBlock;
  
  public Pipeline()
  {
  public Pipeline()
  {
    PipelineBegin = StartBlock<string>();

    DownloadBlock = TransformBlock<string, byte[]>(
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

    DownloadBlock.LinkTo(MergeBlock, e => e.Properties["Type"] == "Website");
    DownloadBlock.LinkTo(ImageBlock, e => e.Properties["Type"] == "Thumbnail");

    MergeBlock.LinkTo(DiskWriteBlock);
      
    FinishBlock.LinkFrom(DiskWriteBlock, ImageBlock);
    
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