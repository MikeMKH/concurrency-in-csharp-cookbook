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
                  // console order is 1, 1, 2
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
        
        [Fact]
        public async void ExampleReturnOnDifferentContextAsync()
        {
            Console.WriteLine($"{Task.CurrentId} Before: ConfigureAwait(false)");
            await Task.Delay(TimeSpan.FromTicks(10)).ConfigureAwait(false);
            Console.WriteLine($"{Task.CurrentId} After: ConfigureAwait(false)");
        }
        
        [Fact]
        public async void ExampleReturnOnSameContextAsync()
        {
            Console.WriteLine($"{Task.CurrentId} Before: ConfigureAwait(true)");
            await Task.Delay(TimeSpan.FromTicks(10)).ConfigureAwait(true);
            Console.WriteLine($"{Task.CurrentId} After: ConfigureAwait(true)");
        }
        
        [Fact]
        public async void TestWrapValueTaskAsync()
        {
            ValueTask<int> MethodAsync() => new ValueTask<int>(8);
            
            await WrapperAsync();
            
            async Task WrapperAsync()
            {
                ValueTask<int> t = MethodAsync();
                // other stuff
                int result = await t;
                Assert.Equal(8, result);
            }
        }
        
        [Fact]
        public async void TestValueTaskAsTaskAsync()
        {
            ValueTask<int> MethodAsync() => new ValueTask<int>(8);
            
            await WrapperAsync();
            
            async Task WrapperAsync()
            {
                Task<int> t = MethodAsync().AsTask();
                // other stuff
                int result = await t;
                Assert.Equal(8, result);
            }
        }
        
        [Fact]
        public async void TestConsumeValueTaskMultipleTimesAsync()
        {
            ValueTask<int> MethodAsync() => new ValueTask<int>(8);
            
            await ConsumeMultipleTimesAsync();
            
            async Task ConsumeMultipleTimesAsync()
            {
                Task<int> t1 = MethodAsync().AsTask();
                Task<int> t2 = MethodAsync().AsTask();
                // other stuff
                int x1 = await t1;
                int x2 = await t2;
                Assert.Equal(16, x1 + x2);
            }
        }
    }
}
