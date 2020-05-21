using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Example
{
    public class Examples
    {   
        [Fact]
        public void ExampleIssueCancelRequest()
        {
            using var cts = new CancellationTokenSource();
            var task = CancelableMethodAsync(
                "ExampleIssueCancelRequest", cts.Token);
            
            cts.Cancel();
            
            async Task<int> CancelableMethodAsync(
                string message, CancellationToken cancellationToken = default)
            {
                Console.WriteLine($"CancelableMethodAsync: START {message}");
                await Task.Delay(TimeSpan.FromSeconds(2));
                Console.WriteLine($"CancelableMethodAsync: END   {message}");
                return 8;
            }
        }
        
        [Fact]
        public void TestCancelableMethodThrowIfCancellationRequested()
        {
            using var cts = new CancellationTokenSource();
            _ = Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var task = CancelableMethodAsync(cts.Token);
                cts.Cancel();
            });
            
            async Task<int> CancelableMethodAsync(
                CancellationToken cancellationToken)
            {
                foreach (var x in Enumerable.Range(1, 100))
                {
                    Thread.Sleep(1);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                return 8;
            }
        }

        [Fact]
        public void TestCancelledEnumerableThrowsExceptionOnceAccessed()
        {
            using var cts = new CancellationTokenSource();
            var values = MultiplyBy2(Enumerable.Range(1, 100_000), cts.Token);

            cts.Cancel();

            Assert.Throws<OperationCanceledException>(
                () => Assert.Empty(values));

            IEnumerable<int> MultiplyBy2(
                IEnumerable<int> values, CancellationToken cancellationToken)
                => values.AsParallel()
                  .WithCancellation(cancellationToken)
                  .Select(x => x * 2);
        }

        [Fact]
        public void ExampleCancelFlowsThroughDataflowBlocks()
        {
            using var cts = new CancellationTokenSource();
            var options = new ExecutionDataflowBlockOptions
            {
                CancellationToken = cts.Token
            };
            var b1 = new TransformBlock<int, int>(
                x => { Console.WriteLine("TransformBlock 1"); return x * 2; }, options);
            var b2 = new TransformBlock<int, int>(
                x => { Console.WriteLine("TransformBlock 2"); return x + 3; }, options);
            var b3 = new TransformBlock<int, string>(x =>
                {
                    Console.WriteLine("TransformBlock 3");
                    Thread.Sleep(100_000);
                    return x.ToString() + "!";
                }, options);
            var flowCompletion = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
            b1.LinkTo(b2, flowCompletion);
            b2.LinkTo(b3, flowCompletion);
            var flow = DataflowBlock.Encapsulate(b1, b3);

            var result = flow.Post(8);
            flow.Complete();
            cts.Cancel();
        }
    }
}
