using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Nito;
using Nito.AsyncEx;

using Xunit;

namespace Example
{
    public class Examples
    {
        class SharedFoo
        {
            static int _value;
            static readonly Lazy<int> SharedValue = new Lazy<int>(() =>
            {
                Console.WriteLine($"Setting SharedValue to {_value + 1}");
                return _value++;
            });
            
            public int UseAndReturnSharedValue()
            {
                int value = SharedValue.Value;
                return value;
            }
        }
        [Fact]
        public void TestUsingSharedValueReturnsSameValue()
        {
            var sut = new SharedFoo();
            var expected = sut.UseAndReturnSharedValue();
            var acutal = sut.UseAndReturnSharedValue();
            Assert.Equal(expected, acutal);
        }
        
        [Fact]
        public async void TestSubscribeWithDeferAsync()
        {
            List<Task<int>> spies = new List<Task<int>>();
            var observable = Observable.Defer(() =>
            {
                var t = GetValueAsync();
                spies.Add(t);
                return t.ToObservable();
            });
            
            observable.Subscribe(x => Assert.Equal(8, x));
            observable.Subscribe(x => Assert.Equal(8, x));
            
            Task.WaitAll(spies.ToArray());
            
            async Task<int> GetValueAsync()
            {
                Console.WriteLine($"START GetValueAsync on {Task.CurrentId}");
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                Console.WriteLine($"END   GetValueAsync on {Task.CurrentId}");
                return 8;
            }
        }
        
        private static AsyncLocal<Guid> _operationId = new AsyncLocal<Guid>();
        
        [Fact]
        public async void TestAsyncLocalCanStoreInformationAsync()
        {
            _operationId.Value = Guid.NewGuid();
            var expected = _operationId.Value;
            
            await DoLongOperation();
            
            async Task DoLongOperation()
            {
                Assert.Equal(expected, _operationId.Value);
                await DoStepOfLongOperation();
            }
            
            async Task DoStepOfLongOperation()
            {
                var expected = _operationId.Value;
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                await Task.Run(() => Assert.Equal(expected, _operationId.Value));
            }
        }
        
        [Fact]
        public async void TestBooleanArgumentHackAsync()
        {
            var foo1 = await FooAsync();
            var foo2 = Foo();
            
            Assert.Equal(foo1, foo2);
            
            async Task<int> FooCore(bool sync = true)
            {
                int result = 8;
                
                if (sync)
                  Thread.Sleep(result);       // synchronous
                else
                  await Task.Delay(result); // asynchronous
                  
                return result;
            }
            
            Task<int> FooAsync() => FooCore(sync: false);
            int Foo() => FooCore(sync: true).GetAwaiter().GetResult();
        }
        
        static TransformBlock<Try<TInput>, Try<TOutput>> RailwayTransform<TInput, TOutput>(
            Func<TInput, TOutput> func) => 
              new TransformBlock<Try<TInput>, Try<TOutput>>(t => t.Map(func));
        
        [Fact]
        public async void ExampleRailTransformAsync()
        {
            var sub1 = RailwayTransform<int, int>(x => x - 1);
            var mul2 = RailwayTransform<int, int>(x => x * 2);
            var add3 = RailwayTransform<int, int>(x => x + 3);
            
            var options = new DataflowLinkOptions { PropagateCompletion = true };
            sub1.LinkTo(mul2, options);
            mul2.LinkTo(add3, options);
            
            sub1.Post(Try.FromValue(5));
            sub1.Post(Try.FromValue(1));
            sub1.Post(Try.FromValue(-1));
            sub1.Complete();
            
            while (await add3.OutputAvailableAsync())
            {
                Try<int> item = await add3.ReceiveAsync();
                if (item.IsValue)
                  Console.WriteLine($"RailwayTransform: value={item.Value}");
                else
                  Console.WriteLine($"RailwayTransform: exception=\"{item.Exception.Message}\"");
            }
        }
        
        [Fact]
        public async void ExampleRailTransformWithExceptionAsync()
        {
            var start = RailwayTransform<int, int>(x => x + 8);
            var error = RailwayTransform<int, int>(
                x => (x % 3 == 1) ? throw new Exception("x % 3 == 1") : x + 7);
            var final = RailwayTransform<int, int>(x => x * 3);
            
            var options = new DataflowLinkOptions { PropagateCompletion = true };
            start.LinkTo(error, options);
            error.LinkTo(final, options);
            
            start.Post(Try.FromValue(5));
            start.Post(Try.FromValue(1)); // should not error
            start.Post(Try.FromValue(-1));
            start.Complete();
            
            while (await final.OutputAvailableAsync())
            {
                Try<int> item = await final.ReceiveAsync();
                if (item.IsValue)
                  Console.WriteLine($"RailwayTransform: value={item.Value}");
                else
                  Console.WriteLine($"RailwayTransform: excpetion=\"{item.Exception.Message}\"");
            }
        }
    }
}
