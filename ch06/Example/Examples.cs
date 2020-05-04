using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;
using Xunit;

namespace Example
{
    public class Examples
    {
        [Fact]
        public void ExampleTimerCanBeObservable()
        {
            var counter = 0;
            var timer = new System.Timers.Timer(interval: 10) { Enabled = true };
            IObservable<EventPattern<ElapsedEventArgs>> ticks =
            Observable.FromEventPattern<ElapsedEventHandler, ElapsedEventArgs>(
                handler => (s, a) => handler(s, a),
                handler => timer.Elapsed += handler,
                handler => timer.Elapsed -= handler
            );
            ticks
              .Select(_ => counter++)
              .Subscribe(data => Console.WriteLine($"Timer: {counter}"));
        }
    }
}
