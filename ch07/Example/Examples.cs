using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Reactive.Testing;
using Nito.AsyncEx;
using Xunit;

namespace Example
{
    public class Examples
    {
        class Foo
        {
            public Task<bool> TrueAsync() => Task.FromResult(true);
            public async void VoidAsync() { }
        }
        
        [Fact]
        public async void TrueAsyncReturnsTrueAsync()
        {
            var foo = new Foo();
            var result = await foo.TrueAsync();
            Assert.True(result);
        }
        
        [Fact]
        public void TrueAsyncReturnsTrueWithAsyncContext()
        {
            AsyncContext.Run(async () =>
            {
                var foo = new Foo();
                var result = await foo.TrueAsync();
                Assert.True(result);
            });
        }
    }
}
