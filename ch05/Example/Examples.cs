using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Example
{
    public class Examples
    {
        [Fact]
        public void TestLinkTo()
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
    }
}
