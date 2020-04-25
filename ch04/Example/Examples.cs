using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Example
{
    public class Examples
    {
        [Fact]
        public void TestParallelForEach()
        {
            var values = Enumerable.Range(0, 100).ToArray();
            
            Parallel.ForEach(values, x => {
                // long processing
                values[x]++;
            });
            
            Assert.Equal(Enumerable.Range(1, 100).ToArray(), values);
        }
        
        [Fact]
        public void TestParallelForEachCanStop()
        {
            var values = Enumerable.Range(0, 100).ToArray();
            
            Parallel.ForEach(values, (x, state) => {
                if (x > 10) state.Stop();
                
                values[x]++;
            });
            
            var dups = values.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            dups.ToList().ForEach(x => Console.WriteLine($"Parallel.ForEach stopped on {x}"));
            
            Assert.True(dups.Count() > 1);
        }
        
        [Fact]
        public void TestParallelForEachCanBeCancelled()
        {
            var values = Enumerable.Range(0, 100).ToArray();
            var source = new CancellationTokenSource();
            
            source.CancelAfter(100);
            Parallel.ForEach(values, new ParallelOptions { CancellationToken = source.Token }, (x, state) => {
                values[x]++;
            });
            
            // there should be zero duplicates
            var dups = values.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            dups.ToList().ForEach(x => Console.WriteLine($"Parallel.ForEach cancelled on {x}"));
            
            Assert.Equal(0, dups.Count());
        }
        
        [Fact]
        public void TestParallelForEachCanBeSumWithLocal()
        {
            var values = Enumerable.Range(0, 1_000);
            
            var mutex = new object();
            var result = 0;
            Parallel.ForEach(
                source: values,
                localInit: () => 0,
                body: (x, state, localValue) => localValue += x,
                localFinally: localValue => {
                    lock (mutex)
                    {
                        Console.WriteLine($"Parallel.ForEach local value {localValue}");
                        result += localValue;
                    }
                });
            
            Assert.Equal(values.Sum(), result);
        }
        
        [Fact]
        public void TestAsParallelSum()
        {
            var values = Enumerable.Range(0, 10_000);
            
            Assert.Equal(
                 values.Sum(), values.AsParallel().Sum());
            Assert.Equal(
                 values.Sum(), values.AsParallel().Aggregate(seed: 0, (m, x) => m += x));
        }
        
        [Fact]
        public void TestAsParallelAsOrderedPerservesOrder()
        {
            var values = Enumerable.Range(0, 100);
            
            Assert.NotEqual(
                 values.Select(x => x * 2), values.AsParallel().Select(x => x * 2));
            Assert.Equal(
                 values.Select(x => x * 2), values.AsParallel().Select(x => x * 2).OrderBy(x => x));
            Assert.Equal(
                 values.Select(x => x * 2), values.AsParallel().AsOrdered().Select(x => x * 2));
        }
    }
}
