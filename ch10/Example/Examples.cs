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
            var task = CancelableMethodAsync("ExampleIssueCancelRequest", cts.Token);
            
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
            Assert.ThrowsAsync<OperationCanceledException>(async () =>
            {
                var task = CancelableMethodAsync(cts.Token);
                cts.Cancel();
            });
            
            async Task<int> CancelableMethodAsync(CancellationToken cancellationToken)
            {
                foreach (var x in Enumerable.Range(1, 100))
                {
                    Thread.Sleep(1);
                    cancellationToken.ThrowIfCancellationRequested();
                }
                return 8;
            }
        }
    }
}
