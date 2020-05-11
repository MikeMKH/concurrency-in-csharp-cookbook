using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Text;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;

namespace Example
{
    public class Examples
    {
        [Fact]
        public async void ExampleDownloadStringTaskAsync()
        {
            // taken from https://resources.oreilly.com/examples/0636920266624/blob/master/ch08.cs#L17-39
            Task<string> DownloadStringTaskAsync(WebClient client, Uri address)
            {
                var tcs = new TaskCompletionSource<string>();
                // The event handler will complete the task and unregister itself.
                DownloadStringCompletedEventHandler handler = null;
                handler = (_, e) =>
                {
                  client.DownloadStringCompleted -= handler;
                  if (e.Cancelled)
                    tcs.TrySetCanceled();
                  else if (e.Error != null)
                    tcs.TrySetException(e.Error);
                  else
                    tcs.TrySetResult(e.Result);
                };
                // Register for the event and *then* start the operation.
                client.DownloadStringCompleted += handler;
                client.DownloadStringAsync(address);
                return tcs.Task;
            }
            
            var url = "http://www.google.com";
            var download = await DownloadStringTaskAsync(new WebClient(), new Uri(url));
            Console.WriteLine($"DownloadStringTaskAsync: {url} got {download.Length} characters");
            Assert.NotNull(download);
        }
        
        [Fact]
        public async void TestParallelForEachAsync()
        {
            var sum = 0;
            await Summer(10);
            Assert.Equal((10 * 11) / 2, sum);
            
            async Task Summer(int top)
            {
                var values = Enumerable.Range(1, top);
                Action<int> summer = x => sum += x;
                await Task.Run(() => Parallel.ForEach(values, summer));
            }
        }
        
        [Fact]
        public async void TestCanGetValuesFromObservableAsync()
        {
            var first = 1;
            var last = 8;
            IObservable<int> source = Observable.Range(first, last);
            
            var a = await source.LastAsync();
            Assert.Equal(last, a);
            
            var b = await source;
            Assert.Equal(last, b);
            
            var c = await source.FirstAsync();
            Assert.Equal(first, c);
            
            var d = await source.ToList();
            Assert.Equal(
              Enumerable.Range(first, last).ToList(), d);
        }
        
        [Fact]
        public async void TestAyncToObservableAsync()
        {
            var url = "http://www.google.com";
            GetPage(new HttpClient(), url)
              .Subscribe(async response => {
                var download = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"DownloadStringTaskAsync: {url} got {download.Length} characters");
                Assert.NotNull(download);
              });
            
            IObservable<HttpResponseMessage> GetPage(HttpClient client, string url)
              => Observable.StartAsync(
                token => client.GetAsync(url, token)
              );
        }
    }
}
