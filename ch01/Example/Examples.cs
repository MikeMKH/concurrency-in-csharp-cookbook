using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

using Xunit;

namespace Example
{
    public class Examples
    {
        async Task<int> DoSomethingAsync()
        {
            int value = 8;
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            value *= 2;
            
            await Task.Delay(TimeSpan.FromSeconds(1));
            
            return value;
        }
        
        [Fact]
        public async void TestDoSomethingAsyncAsync()
        {
            int value = await DoSomethingAsync();
            Assert.Equal(8 * 2, value);
        }
        
        [Fact]
        public void ExampleParallelForEach()
        {
            var values = Enumerable.Range(1, 10);
            Parallel.ForEach(values, x =>  Console.WriteLine($"Parallel.ForEach {x}"));
        }
        
        [Fact]
        public void ExampleParallelInvoke()
        {
            var values = Enumerable.Range(1, 10).ToArray();
            Parallel.Invoke(
                () => Process(values, 0, values.Count() / 2),
                () => Process(values, values.Count() / 2, values.Count())
            );
            
            void Process<T>(T[] arr, int begin, int end)
              => Console.WriteLine($"Parallel.Invoke {begin}:{end}");
        }
        
        [Fact]
        public void ExampleObservableInterval()
        {
            IObservable<DateTimeOffset> values = Observable.Interval(TimeSpan.FromSeconds(1))
            .Timestamp()
            .Where(x => x.Value % 2 == 0)
            .Select(x => x.Timestamp);
            
            values.Subscribe(x => Console.WriteLine($"Observable.Interval {x}"));
        }
        
        [Fact]
        public void ExmpleDataflowAsync()
        {
            var block1 = new TransformBlock<int, int>(
                x => (x == 1) ?  throw new InvalidOperationException("Meh"): x * 2);
            var block2 = new TransformBlock<int, int>(x => x - 2);
            
            block1.LinkTo(block2, new DataflowLinkOptions { PropagateCompletion = true });
            
            block1.Post(1);
            Assert.Throws<AggregateException>(() =>  block2.Completion.Wait());
        }
    }
}
