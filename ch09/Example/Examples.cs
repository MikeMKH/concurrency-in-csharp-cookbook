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
    }
}
