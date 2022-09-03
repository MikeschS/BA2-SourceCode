using System;
using System.Collections.Generic;
using System.Text;

namespace BA.Roslyn.CodeBlocks
{
    [MyHandler("iet", MyProperty = 5)]
    public partial class Handler
    {
        public int Data { get; set; }

        public async Task Handle(int request, CancellationToken cancellationToken)
        {
            var handler = new MyHandlerAttribute("tcie") { MyProperty = 5 };
            await handler.Handle(cancellationToken);
            this.Data = handler.MyProperty;
        }
    }

    public partial class Handler
    {
        public async Task PreHandle(int request, CancellationToken cancellationToken)
        {
            var handler = new MyHandlerAttribute("tcie") { MyProperty = 5 };
            await handler.Handle(request, cancellationToken);
            this.Data = handler.MyProperty;
        }
    }

    public class MyHandlerAttribute : Attribute
    {
        private readonly IServiceProvider serviceProvider;

        public MyHandlerAttribute(string data)
        {
            Data = data;
        }

        public int MyProperty { get; set; }
        public string Data { get; }

        public Task Handle(int request, CancellationToken cancellationToken)
        {
            this.MyProperty = request;
            return Task.CompletedTask;
        }
    }
}
