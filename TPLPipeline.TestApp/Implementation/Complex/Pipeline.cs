using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace TPLPipeline.TestApp.Implementation.Complex
{
    class Pipeline : BasePipeline<Job>
    {
        TransformManyBlock<Job, IPipelineJobElement<string>> _start;
        TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<string>> _path1a;
        TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<string>> _path1b;
        TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<string>> _path1c;
        TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<string>> _path1d;
        TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<string>> _path2a;
        TransformBlock<IPipelineJobElement<string>, IPipelineJobElement<string>> _path2b;
        TransformBlock<IPipelineJobElement<Tuple<string,string>>, IPipelineJobElement<string>> _merge;
        ActionBlock<IPipelineJobElement<string>> _finish;

        public Pipeline()
        {
            _start = StartBlock<string>();
            _path1a = TransformBlock<string, string>(async (job, data) =>
            {
                Console.WriteLine("1a: " + data);
                await Task.Delay(100);

                return data;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _path1b = TransformBlock<string, string>(async (job, data) =>
            {
                Console.WriteLine("1b: " + data);
                await Task.Delay(100);

                return data;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _path1c = TransformBlock<string, string>(async (job, data) =>
            {
                Console.WriteLine("1c: " + data);
                await Task.Delay(100);

                return data;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _path1d = TransformBlock<string, string>(async (job, data) =>
            {
                Console.WriteLine("1d: " + data);
                await Task.Delay(100);

                return data;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _path2a = TransformBlock<string, string>(async (job, data) =>
            {
                Console.WriteLine("2a: " + data);
                await Task.Delay(100);

                return data;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });
            _path2b = TransformBlock<string, string>(async (job, data) =>
            {
                Console.WriteLine("2b: " + data);
                await Task.Delay(100);

                return data;
            }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _merge = TransformBlock<Tuple<string, string>, string>(async (job, data) =>
            {
                Console.WriteLine("m: " + data);
                await Task.Delay(100);

                 return data.Item1 + data.Item2;
             }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _finish = ActionBlock<string>(async (job, data) =>
            {
                Console.WriteLine("f: " + data);
                await Task.Delay(100);
            }, true, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

            _start.LinkTo(_path1a, e => e.Properties.ContainsKey("Even"));
            _start.LinkTo(_path2a, e => e.Properties.ContainsKey("Odd"));

            _path1a.LinkTo(_path1b);
            _path1b.LinkTo(_path1c);
            _path1c.LinkTo(_path1d);

            _path2a.LinkTo(_path2b);

            _merge.LinkFrom(_path1d, _path2b);
            _merge.LinkTo(_finish);
        }

        public override void Post(Job job)
        {
            //Console.WriteLine($"{job.Id} posting");

            for(var i = 0; i < 10; ++i)
                job.AddData("data " + i, new DataProperty[] { new DataProperty { Name = (i % 2 == 0) ? "Even" : "Odd", Value = "" } });

            _start.Post(job);
        }

        public override async Task PostAsync(Job job)
        {
            //Console.WriteLine($"{job.Id} posting async");

            for (var i = 0; i < 10; ++i)
                job.AddData("data " + i, new DataProperty[] { new DataProperty { Name = (i % 2 == 0) ? "Even" : "Odd", Value = "" } });

            _start.Post(job);

            await job.Completion;

            //Console.WriteLine($"{job.Id} job completed");
        }
    }
}
