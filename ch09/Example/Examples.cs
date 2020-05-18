using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Xunit;
using System.Linq;

namespace Example
{
    public class Examples
    {
        [Fact]
        public void TestStackIsLIFO()
        {
            var expected = 8;
            var stack = ImmutableStack<int>.Empty;
            stack = stack.Push(5);
            stack = stack.Push(11);
            stack = stack.Push(expected);
            
            stack.Pop(out int actual);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void TestImmutableStackIsImmutable()
        {
            var s0 = ImmutableStack<int>.Empty;
            var s1 = s0.Push(1);
            var s2 = s1.Push(2);
            var s3 = s2.Push(3);
            
            foreach(var x in s3)
            {
                Console.WriteLine($"ImmutableStack: {x}");
            }
            
            s2.Pop(out int x2);
            s3.Pop(out int x3);
            Assert.NotEqual(x2, x3);
        }
        
        [Fact]
        public void TestImmutableQueueIsFIFO()
        {
            var expected = 1;
            var queue = ImmutableQueue<int>.Empty;
            queue = queue.Enqueue(expected);
            queue = queue.Enqueue(2);
            queue = queue.Enqueue(3);
            
            foreach(var x in queue)
            {
                Console.WriteLine($"ImmutableQueue: {x}");
            }
            
            queue.Dequeue(out int actual);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void TestImmutableListIsImmutable()
        {
            var l0 = ImmutableList<int>.Empty;
            var l1 = l0.Insert(0, 2);
            var l2 = l0.Insert(0, 4);
            var l3 = l0.Insert(0, 6);
            
            foreach (var x in
                     from xs in new[] { l0, l1, l2, l3 }
                     from x in xs
                     select x)
            {
                Console.WriteLine($"ImmutableList: {x}");
            }
            
            Assert.NotEqual(l2[0], l3[0]);
        }
        
        [Fact]
        public void ExampleOrderOfImmutableHashSetIsUnpredictable()
        {
            var set = ImmutableHashSet<int>.Empty;
            foreach(var x in Enumerable.Range(1, 100_000))
            {
                set = set.Add(x);
            }
            
            foreach(var x in set.Take(5))
            {
                Console.WriteLine($"ImmutableHashSet: {x}");
            }
        }
        
        [Fact]
        public void TestImmutableDictionarySetItem()
        {
            var d0 = ImmutableDictionary<string, int>.Empty;
            var d1 = d0.Add("ten", 10);
            var d2 = d1.Add("eight", 8);
            var d3 = d2.Add("five", 5);
            
            foreach (var x in d3)
            {
                Console.WriteLine($"ImmutableDictionary: {x.Key}={x.Value}");
            }
            
            var d4 = d3.SetItem("ten", 1010);
            Assert.NotEqual(d4["ten"], d3["ten"]);
        }
        
        [Fact]
        public void TestConcurrentDictionaryTryGetValue()
        {
            var dictionary = new ConcurrentDictionary<int, string>();
            var expected = "foo";
            var value = dictionary.AddOrUpdate(
                0,
                key => { Console.WriteLine($"ConcurrentDictionary: add {key}"); return expected; },
                (key, old) => { Console.WriteLine($"ConcurrentDictionary: update {key}"); return expected; }
            );
            
            var exists = dictionary.TryGetValue(0, out string actual);
            Assert.True(exists);
            Assert.Equal(expected, actual);
        }
        
        [Fact]
        public void ExampleBlockingCollectionProducerConsumer()
        {
            var col = new BlockingCollection<int>();
            
            producer();
            consumer();
            
            void producer()
            {
                col.Add(8);
                col.Add(11);
                col.Add(3);
                col.CompleteAdding();
            }
            
            void consumer()
            {
                foreach(var x in col.GetConsumingEnumerable())
                {
                    Console.WriteLine($"BlockingCollection: {x}");
                }
            }
        }
        
        [Fact]
        public void TestBlockingCollectionCanBeStack()
        {
            var stack = new BlockingCollection<int>(new ConcurrentStack<int>());
            
            stack.Add(2);
            stack.Add(4);
            stack.Add(6);
            stack.CompleteAdding();
            
            Assert.NotEmpty(stack);
            
            foreach(var x in stack.GetConsumingEnumerable())
            {
                Console.WriteLine($"ConcurrentStack: {x}");
            }
            
            Assert.Empty(stack);
        }
        
        [Fact]
        public async void ExampleChannelProducerConsumerAsync()
        {
            Channel<int> queue = Channel.CreateUnbounded<int>();
            
            ChannelWriter<int> writer = queue.Writer;
            await writer.WriteAsync(1);
            await writer.WriteAsync(2);
            await writer.WriteAsync(3);
            writer.Complete();
            
            ChannelReader<int> reader = queue.Reader;
            await foreach(var x in reader.ReadAllAsync())
            {
                Console.WriteLine($"Channel: {x}");
            }
        }
        
        [Fact]
        public void TestChannelCreateBoundedWaitsUntilItHasRoom()
        {
            Channel<int> queue = Channel.CreateBounded<int>(1);
            ChannelWriter<int> writer = queue.Writer;
            
            var written = writer.TryWrite(8);
            Assert.True(written);
            
            written = writer.TryWrite(11);
            Assert.False(written);
        }

        [Fact]
        public async void TestBufferBlockBoundedCapacityWaitsUntilItHasRoomAsync()
        {
            var queue = new BufferBlock<int>(
                new DataflowBlockOptions { BoundedCapacity = 1 });

            var expected = 8;
            await queue.SendAsync(expected);
            // await queue.SendAsync(11);  // would not return control
            queue.Complete();
            

            var actual = await queue.ReceiveAsync();
            Assert.Equal(expected, actual);
        }

        [Fact]
        public async void TestBoundedChannelFullModeDropOldestKeepsNewestAsync()
        {
            var queue = Channel.CreateBounded<int>(
                new BoundedChannelOptions(1)
                {
                    FullMode = BoundedChannelFullMode.DropOldest
                });
            ChannelWriter<int> writer = queue.Writer;
            ChannelReader<int> reader = queue.Reader;

            var expected = 11;
            await writer.WriteAsync(8);
            await writer.WriteAsync(expected);

            var actual = await reader.ReadAsync();
            Assert.Equal(expected, actual);
        }
    }
}
