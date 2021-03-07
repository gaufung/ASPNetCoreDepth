## 1 ASP.NET Core 框架

对于框架（Framework) 和类库（Library）有很多人将它们两者混为一谈，这两者有本质上的区别。假设我们现在有一个 `Web` 的 `MVC` 的类库

```C#
public static class MvcLib
{
    public static Task ListenAsync(URi address);
    public static Task<Request> ReceiveAsync();
    public static Task<Controller> CreateControllerAsync(Request request);
    public static Task<View> ExecuteControllerAsync(Controller controller);
    public static Task RenderViewAsync(View view);
}
```

`MvcLib` 包含了一个 MVC Web 服务器包含的五个主要步骤
1. 侦听一个地址
2. 接受一个请求
3. 创建 `Controller` 来处理这个请求
4. `Controller` 处理这个请求
5. 渲染请求处理的结果 `View`

那么当我们 Web 应用程序使用这个 `Library` 的时候，步骤如下 

```C#
class Program
{
    static async Task Main()
    {
        while(true)
        {
            var address = new Uri("https://127.0.0.1:8080");
            await MvcLib.ListenAsync(address);
            while(true)
            {
                var request = await MvcLib.ReceiveAsync();
                var controller = await MvcLib.CreateControllerAsync(request);
                var view = await MvcLib.ExecuteControllerAsync(controller);
                await MvcLib.RenderViewAsync(view);
            }
        }
    }
}
```

只需要使用这个 `MvcLib` 就可以构建一个 Web 应用程序，但是这样做的，几乎每个程序都需要这么写一遍，为何不讲这个 `MvcLib` 在抽象成一个框架 `MvcFramework`。在这个框架中，应用程序只需要定制化 `Controller` 和  `View`，并且让这个框架能够正确地找到响应的 `Controller` 和 `View` 就可以实现不同的业务需求。

既然我们已经将这个程序的控制权交出去了，那么对于程序运行过程中需要的服务，比如访问数据库，访问其他程序等，就需要框架自行帮我们找到。所以我们要预先将这些服务注册到这个框架中，通常由一个服务容器来承载这个定制化的服务，注册的过程叫做依赖注入(Dependency Injection，DI)。

对于一个框架最重要的一点是有拓展点，比如说框架默认只是 `Http 1.1` 协议，但是如果应用程序想要支持 `Http 2.0`， 那么框架的使用者可以进行扩展。最好的实现就是采用虚方法定义各个组件。

```C#
public class MvcEngine
{
    public async Task StartAsync(Uri address)
    {
        while(true)
        {
            var request = await ReceiveAsync();
            var controller = await CreateControllerAsync(request);
            var view = await ExecuteControllerAsync(controller);
            await RenderAsync(view);
        }
    }
    
    protected virtual Task ListenAsync(URi address);
    protected virtual Task<Request> ReceiveAsync();
    protected virtual Task<Controller> CreateControllerAsync(Request request);
    protected virtual Task<View> ExecuteControllerAsync(Controller controller);
    protected virtual Task RenderViewAsync(View view);
}
```

这样应用程序可以继承这个 `MvcEngine` 方法，并且 override 其中的部分方法。这种实现方式还是有点问题，`MvcEngine` 类包含了全部组件，不符合我们的单一责任原则。因此我们可以使用工厂方法将它们继续拆分。
```C#
public interface IWebListener
{
    Task ListenAsync(Uri address);
    Task<HttpContext> ReceiveAsync();
}

public interface IControllerActivator
{
    Task<Controller> CreateControllerAsync(HttpContext httpContext);
    Task ReleaseAsync(Controller controller);
}

public interface IControllerExecutor
{
    Task<View> ExecuteAsync(Controller controller, HttpContext httpContext);
}

public interface IViewRender
{
    Task RendAsync(View view, HttpContext httpContext);
}
```

有了这四个接口，我们可以在 `MvcEngine` 中定义四个方法来获取它们

```C#
public class MvcEngine
{
    public async Task StartAsync(Uri address)
    {
        var listener =  GetWebListener();
        var activator = GetControllerActivator();
        var executor = GetControllerExecutor();
        var render = GetViewRender();
        await listener.ListenAsync(address);
        while(true)
        {
            var httpContext = await listener.ReceiveAsync();
            var controller = await activator.CreateControllerAsync(httpContext);
            var view = await executor.ExecuteAsync(controller, httpContext);
            await render.RendAsync(view, httpContext);
            await activator.ReleaseAsync(controller);
        }
    }
    
    protected virtual IWebListener GetWebListener();
    protected virtual IControllerActivator GetControllerActivator();
    protected virtual IControllerExecutor GetControllerExecutor();
    protected virtual IViewRender GetViewRender();
}
```

但是我们还想继续讲所有的工厂方法放入同一个工厂中，那么可以用抽象方法进一步抽象

```C#
public interface IMvcEngineFactory
{
    IWebListener GetWebListener();
    IControllerActivator GetControllerActivator();
    IControllerExecutor GetControllerExecutor();
    IViewRender GetViewRender();
}

public class MvcEngineFactory : IMvcEngineFactory
{
    public virtual IWebListener GetWebListener();
    public virtual IControllerActivator GetControllerActivator();
    public virtual IControllerExecutor GetControllerExecutor();
    public virtual IViewRender GetViewRender();
}

public class MvcEngine
{
    public IMvcEngineFactory MvcEngineFactory {get;}
    public MvcEngine(IMvcEngineFactory engineFactory=null)
     => MvcEngineFactory = engineFactory ?? new MvcEngineFactory();

    public async Task StartAsync(Uri address)
    {
        var listener =  MvcEngineFactory.GetWebListener();
        var activator = MvcEngineFactory.GetControllerActivator();
        var executor = MvcEngineFactory.GetControllerExecutor();
        var render = MvcEngineFactory.GetViewRender();
        await listener.ListenAsync(address);
        while(true)
        {
            var httpContext = await listener.ReceiveAsync();
            var controller = await activator.CreateControllerAsync(httpContext);
            var view = await executor.ExecuteAsync(controller, httpContext);
            await render.RendAsync(view, httpContext);
            await activator.ReleaseAsync(controller);
        }
    }
}
```

那么 Web 应用程序中如何拓展这个框架呢？假设我们有一个 `IControllerActivator` 的实现 `SingletonControllerActivator`

```C#
public class FoobarEngineFactory : MvcEngineFactory
{
    public override IControllerActivator GetControllerActivator()
    {
        return new SingletonControllerActivator();
    }
}

public class App
{
    static async Task Main()
    {
        var address = new Uri("http://127.0.0.1.8080");
        var engine = new MvcEngine(new FoobarEngineFactory());
        await engine.StartAsync(address);
    }
}
```