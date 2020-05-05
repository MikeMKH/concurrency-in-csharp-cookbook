using System;
using System.Reactive;
using System.Reactive.Linq;
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
              .Subscribe(_ => Console.WriteLine($"Timer\t:{counter}"));
        }
        
        [Fact]
        public void ExampleTimerCanBeObservableWithReflection()
        {
            var counter = 0;
            var timer = new System.Timers.Timer(interval: 10) { Enabled = true };
            IObservable<EventPattern<object>> ticks =
            Observable.FromEventPattern(timer, nameof(Timer.Elapsed));
            ticks
              .Select(_ => counter++)
              .Subscribe(_ => Console.WriteLine($"Reflect\t:{counter}"));
        }
        
        [Fact]
        public void ExampleBuffer()
        {
            Observable.Interval(TimeSpan.FromMilliseconds(10))
              .Buffer(2)
              .Subscribe(x => Console.WriteLine($"Buffer\t:{x[0]}, {x[1]}"));
        }
        
        [Fact]
        public void ExampleWindow()
        {
            Observable.Interval(TimeSpan.FromMilliseconds(10))
              .Window(2)
              .Subscribe(group => {
                  Console.WriteLine($"Window\t:Starting Group");
                  group.Subscribe(
                      x => Console.WriteLine($"Window\t:{x}"),
                      () => Console.WriteLine($"Window\t:Ending Group"));
              });
        }
    }
}
