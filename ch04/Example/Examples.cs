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
    }
}
