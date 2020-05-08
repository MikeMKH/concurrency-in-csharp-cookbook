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
    }
}
