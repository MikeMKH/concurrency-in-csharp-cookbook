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
            interface IAsyncInterface<T>
            {
                Task<T> GetAsyncValue();
            }
            
            class MySynchronousImplementation : IAsyncInterface<int>
            {
                private int result;
                public MySynchronousImplementation(int result)
                {
                    this.result = result;
                }
                
                public Task<int> GetAsyncValue()
                  => Task.FromResult(result);
            }
        
        [Fact]
        public async void TestWrapAsyncAsync()
        {   
            var expected = 8;
            IAsyncInterface<int> x = new MySynchronousImplementation(expected);
            var actual = await x.GetAsyncValue();
            
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public async void TestTaskInOrderAsync()
        {
            Task<int> t1 = Task.FromResult(3);
            Task<int> t2 = Task.FromResult(5);
            Task<int> t3 = Task.FromResult(7);
            
            int[] results = await Task.WhenAll(t1, t2, t3);
            Assert.Equal(new[] {3, 5, 7}, results);
        }
        
        [Fact]
        public async void TestProcessTaskAsyncAsync()
        {
            Task<int> t1 = DelayAndReturnAsync(1);
            Task<int> t2 = DelayAndReturnAsync(2);
            Task<int> t3 = DelayAndReturnAsync(1);
            
            Task<int>[] processing = new [] {t1, t2, t3}
              .Select(async t =>
              {
                  var result = await t;
                  Console.WriteLine($"ExampleProcessTaskAsyncAsync: {result}");
                  return result;
              }).ToArray();
              
            int [] results = await Task.WhenAll(processing);
            
            Assert.Equal(new [] {1, 2, 1}, results);
            
            async Task<int> DelayAndReturnAsync(int value)
            {
                await Task.Delay(TimeSpan.FromSeconds(value));
                return value;
            }
        }
    }
}
