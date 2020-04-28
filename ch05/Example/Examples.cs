using System;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Example
{
    public class Examples
    {
        [Fact]
        public void TestLinkToAssert()
        {
            var t1 = new TransformBlock<int, int>(x => x + 2);
            var t2 = new TransformBlock<int, int>(x => x * 2);
            var log = new TransformBlock<int, int>(x => { Console.WriteLine($"TransformBlock has {x}"); return x; });
            var test = new ActionBlock<int>(x => Assert.Equal((8 + 2) * 2, x));
            
            var options = new DataflowLinkOptions { PropagateCompletion = true };
            t1.LinkTo(t2, options);
            t2.LinkTo(log, options);
            log.LinkTo(test, options);
            
            t1.Post(8);
            t1.Complete();
            
            test.Completion.Wait();
        }
        
        [Fact]
        public void TestBoundedCapacity()
        {
            var t1 = new TransformBlock<int, int>(x => x + 2);
            var t2 = new TransformBlock<int, string>(x => (x * 2).ToString());
            var b = new BufferBlock<string>(new DataflowBlockOptions { BoundedCapacity = 1 });
            
            var log = new TransformBlock<string, string>(x => { Console.WriteLine($"TransformBlock has {x}"); return x; });
            var test = new ActionBlock<string>(x => Assert.Equal("8", x));
            
            var options = new DataflowLinkOptions { PropagateCompletion = true };
            t1.LinkTo(t2, options);
            t2.LinkTo(b, options);
            b.LinkTo(log, options);
            log.LinkTo(test, options);
            
            
            t1.Post(2);
            t1.Complete();
            
            test.Completion.Wait();
        }
        
        [Fact]
        public void TestDegreeOfParallelism()
        {
            var t1 = new TransformBlock<int, int>(x => x * 4);
            var t2 = new TransformBlock<int, string>(
                x => $"Hello number {x}",
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = DataflowBlockOptions.Unbounded }
            );
            
            var log = new TransformBlock<string, string>(x => { Console.WriteLine($"TransformBlock has {x}"); return x; });
            var test = new ActionBlock<string>(x => Assert.Equal("Hello number 8", x));
            
            var options = new DataflowLinkOptions { PropagateCompletion = true };
            t1.LinkTo(t2, options);
            t2.LinkTo(log, options);
            log.LinkTo(test, options);
            
            
            t1.Post(2);
            t1.Complete();
            
            test.Completion.Wait();
        }
        
        
    }
}
