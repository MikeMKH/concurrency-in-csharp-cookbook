using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Nito.AsyncEx;
using Xunit;

namespace Example
{
    public class Examples
    {
        interface IAsyncFoo
        {
            Task<int> CountBytesAsync(HttpClient client, string url);
        }
        
        class AsyncFoo : IAsyncFoo
        {
            public async Task<int> CountBytesAsync(HttpClient client, string url)
            {
                var bytes = await client.GetByteArrayAsync(url);
                return bytes.Length;
            }
        }
        
        class FooStub : IAsyncFoo
        {
            public Task<int> CountBytesAsync(HttpClient client, string url)
              => Task.FromResult(42);
        }
        
        [Fact]
        public void TestFooStubReturnsTheAnswerToEverything()
        {
            IAsyncFoo sut = new FooStub();
            var t = sut.CountBytesAsync(new HttpClient(), "http://www.slayer.net");
            Assert.Equal(42, t.Result);
        }
        
        [Fact]
        public void TestAyncFooReturnsNumberOfBytes()
        {
            IAsyncFoo sut = new AsyncFoo();
            var url = "http://www.google.com/";
            var t = sut.CountBytesAsync(new HttpClient(), url);
            var actual = t.Result;
            
            Console.WriteLine($"{url} returned {actual} bytes");
            Assert.True(actual > 1);
        }
        
        [Fact]
        public async void TestAsyncAsMethodAsync()
        {
            var result = await GetDatAsync();
            Assert.Equal(8, result);
            
            async Task<int> GetDatAsync()
            {
                await Task.Delay(TimeSpan.FromMilliseconds(10));
                return 8;
            }
        }
        
        public class Bar
        {
            public AsyncLazy<int> Data
            {
                get { return _data; }
            }
            
            private AsyncLazy<int> _data =
              new AsyncLazy<int>(async () =>
              {
                  await Task.Delay(TimeSpan.FromMilliseconds(10));
                  return 8;
              });
        }
        
        [Fact]
        public async void TestAsyncLazyPropertyAsync()
        {
            var sut = new Bar();
            var result = await sut.Data;
            Assert.Equal(8, result);
        }
        
        public class Quax : IAsyncDisposable
        {
            public async ValueTask DisposeAsync()
            {
                Console.WriteLine("DisposeAsync: DisposeAsync");
                await Task.Delay(TimeSpan.FromMilliseconds(10));
            }
        }
        
        [Fact]
        public async void ExampleAsyncDisposable()
        {
            await using (var sut = new Quax())
            {
                Console.WriteLine("using: Quax START");
                Console.WriteLine("using: Quax END");
            }
        }
    }
}
