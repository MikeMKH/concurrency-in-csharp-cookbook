using System;
using System.Collections.Generic;
using System.Linq;
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
        public async void TestAwaitTaskRunRunsInDifferentThreadAsync()
        {
            var expected = Task.CurrentId;
            var actual = (int?) 0;
            
            await Task.Run(() => {
                actual = Task.CurrentId;
            });
            
            Assert.NotEqual(expected, actual);
        }
        
        [Fact]
        public void TestParallelOptionsUsingConcurrentExclusiveSchedulerPairAsync()
        {
            var values = Enumerable.Range(1, 100);
            var expected = values.Sum();
            
            var schedulerPair = new ConcurrentExclusiveSchedulerPair(
                TaskScheduler.Default, maxConcurrencyLevel: 8);
            var scheduler = schedulerPair.ConcurrentScheduler;
            var options = new ParallelOptions { TaskScheduler = scheduler };
            
            var actual = 0;
            Parallel.ForEach(
                values,
                () => 0,
                (i, _, acc) =>
                {
                    Console.WriteLine(
                        $"Parallel.ForEach: i={i:D3}, acc={acc:D4}, Task.CurrentId={Task.CurrentId}");
                    acc += i;
                    return acc;    
                },
                (acc) => Interlocked.Add(ref actual, acc)
            );
            
            Assert.Equal(expected, actual);
        }
    }
}
