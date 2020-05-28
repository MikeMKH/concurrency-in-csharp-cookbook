using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
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
        public async void TestAsyncMethodSequenceIsWhatWeExpectAsync()
        {
            var result = await Task.Run(async () => SequenceAsync());
            Assert.Equal(10, result.Result);
            
            async Task<int> SequenceAsync()
            {
                int value = 10;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                value -= 1;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                value += 1;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                return value;
            }
        }
        
        class SharedData
        {
            public int Value { get; set; }
        }
        
        public async void TestSharedDataMightNotBeWhatWeExpectAsync()
        {
            var data = new SharedData { Value = 10 };
            
            Task t1 = ModifyValueAsync(data);
            Task t2 = ModifyValueAsync(data);
            Task t3 = ModifyValueAsync(data);
            
            await Task.WhenAll(t1, t2, t3);
            Assert.Equal(13, data.Value);
            
            async Task ModifyValueAsync(SharedData data)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                data.Value += 1;
            }
        }
        
        class Foo
        {
            private readonly object _increment_mutex = new object();
            
            public int Value { get { return _value; } }
            private int _value = 0;
            
            public void Increment()
            {
                Console.WriteLine($"Before Lock: {_value}");
                lock(_increment_mutex)
                {
                    Console.WriteLine($"Inside Lock: {_value}");
                    _value += 1;
                }
                Console.WriteLine($"After  Lock: {_value}");
            }
        }
        
        [Fact]
        public void TestLockOnlyAllowsOneAtTime()
        {
            var foo = new Foo();
            Enumerable.Range(1, 10).AsParallel().ForAll(x =>
            {
                Console.WriteLine($"START: {x}");
                foo.Increment();
                Console.WriteLine($"END:   {x}");
            });
            Assert.Equal(10, foo.Value);
        }
    }
}
