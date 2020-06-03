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
            
            int value1 = 0, value2 = 0;
            observable.Subscribe(x => value1 = x);
            observable.Subscribe(x => value2 = x);
            
            Task.WaitAll(spies.ToArray());
            Assert.Equal(value1, value2);
            
            async Task<int> GetValueAsync()
            {
                Console.WriteLine($"START GetValueAsync on {Task.CurrentId}");
                await Task.Delay(TimeSpan.FromMilliseconds(100));
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
    }
}
