using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Example
{
    public class Examples
    {
        async IAsyncEnumerable<IEnumerable<int>> GetResultsAsync()
        {
            var values = Enumerable.Range(1, 100);
            for(var i = 0; i < values.Count() / 10; i++)
            {
                var results = values.Skip(i * 10).Take(10);
                yield return results;
            }
        }
        
        [Fact]
        public async void TestAsyncMethodProducesResultsForeachAsync()
        {
            await foreach (var value in GetValuesAsync())
            {
                Assert.True(value > 0);
            }
            
            async IAsyncEnumerable<int> GetValuesAsync()
            {
                await Task.Delay(TimeSpan.FromTicks(10));
                yield return 1;
                await Task.Delay(TimeSpan.FromTicks(10));
                yield return 2;
                await Task.Delay(TimeSpan.FromTicks(10));
                yield return 3;
            } 
        }
        
        [Fact]
        public async void TestWrapResultsInAsyncEnumerableAsync()
        {
            await foreach(var result in GetResultsAsync())
            {
                Assert.Equal(10, result.Count());
            }
        }
        
        [Fact]
        public async void TestWrapResultsInAsyncEnumerableWithDifferentContextAsync()
        {
            await foreach(var result in GetResultsAsync().ConfigureAwait(false))
            {
                Assert.Equal(10, result.Count());
            }
        }
    }
}
