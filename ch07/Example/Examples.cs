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
        
        static TransformBlock<int, int> IdentityBlock() => new TransformBlock<int, int>(x => x);
        
        [Fact]
        public async void TestSequenceOfBlocksAsync()
        {
            var block = IdentityBlock();
            
            block.Post(8);
            block.Post(1013);
            block.Complete();
            
            Assert.Equal(8, block.Receive());
            Assert.Equal(1013, block.Receive());
            await block.Completion;
        }
        
        [Fact]
        public void TestSequenceOfBlocksWithExceptionAsync()
        {
            var block = IdentityBlock();
            
            block.Post(8);
            block.Post(1013);
            (block as IDataflowBlock).Fault(new InvalidOperationException());
            
            Assert.ThrowsAsync<AggregateException>(async () => await block.Completion);
        }
        
          public interface IHttpService
          {
              IObservable<string> GetString(string url);
          }
          
          public class Timeout
          {
              private readonly IHttpService _service;
              
              public Timeout(IHttpService service)
              {
                  _service = service;
              }
              
              public IObservable<string> GetStringWithTimeout(string url, IScheduler scheduler = null)
                => _service.GetString(url)
                     .Timeout(TimeSpan.FromSeconds(2), scheduler ?? Scheduler.Default);
          }
          
          public class SuccessHttpServiceStub : IHttpService
          {
              public IScheduler Scheduler { get; set; }
              public TimeSpan Delay { get; set; }
              
              public IObservable<string> GetString(string url)
                => Observable.Return("Success")
                     .Delay(Delay, Scheduler);
          }
          
          [Fact]
          public void TestTimeoutReturnsAfterDelay()
          {
              var scheduler = new TestScheduler();
              var stub = new SuccessHttpServiceStub()
              {
                  Scheduler = scheduler,
                  Delay = TimeSpan.FromMilliseconds(1)
              };
              
              var sut = new Timeout(stub);
              string result = "Failure";
              
              sut.GetStringWithTimeout("www.amazon.com", scheduler)
                .Subscribe(data => result = data);
                
              scheduler.Start();
              Assert.Equal("Success", result);
          }
          
          [Fact]
          public void TestTimeoutFailsAfterLongDelay()
          {
              var scheduler = new TestScheduler();
              var stub = new SuccessHttpServiceStub()
              {
                  Scheduler = scheduler,
                  Delay = TimeSpan.FromHours(5)
              };
              
              var sut = new Timeout(stub);
              Exception result = null;
              
              sut.GetStringWithTimeout("www.amazon.com", scheduler)
                .Subscribe(_ => Assert.True(false), error => result = error);
                
              scheduler.Start();
              Assert.IsType<TimeoutException>(result);
          }
    }
}
