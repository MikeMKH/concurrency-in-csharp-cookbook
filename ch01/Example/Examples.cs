using System;
using System.Threading.Tasks;
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
    }
}
