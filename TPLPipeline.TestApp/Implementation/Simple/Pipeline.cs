using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline.TestApp.Implementation.Simple
{
    class Pipeline : BasePipeline<Job>
    {
        TransformManyBlock<Job, IPipelineJobElement<string>> _start;
        ActionBlock<IPipelineJobElement<string>> _finish;

        public Pipeline()
        {
            _start = StartBlock<string>();
            _finish = ActionBlock<string>(async (job, data) =>
            {
                await Task.Delay(100); /*Console.WriteLine($"{job.Id} finish block");*/
            }, true, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 25 });

            _start.LinkTo(_finish);
        }

        public override void Post(Job job)
        {
            //Console.WriteLine($"{job.Id} posting");

            for(var i = 0; i < 100; ++i)
                job.AddData(job.Id);

            _start.Post(job);
        }

        public override async Task PostAsync(Job job)
        {
            //Console.WriteLine($"{job.Id} posting async");

            for (var i = 0; i < 100; ++i)
                job.AddData(job.Id);

            _start.Post(job);

            await job.Completion;

            //Console.WriteLine($"{job.Id} job completed");
        }
    }
}
