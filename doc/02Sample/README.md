## 1 Hello World
接下来我们看一下最简单的 `ASP.NET Core` MVC 的项目: [Hello World](https://github.com/gaufung/ASPNetCoreDepth/tree/master/src/helloworld)

项目的结构如下 

```tree
+-- Controllers
|   +-- HelloController.cs
+-- Properties
|   +-- launchSettings.json
+-- Views
|   +-- Hello
|       +-- SayHello.cshtml
+-- appsettings.Development.json
+-- appsettings.json
+-- helloworld.csproj
+-- Program.cs
+-- Startup.cs
```

使用 `dotnet run` 命令就会得到如下的结果， 

![](./images/dnrun.png)

打开浏览器，输入 `https://localhost:5001/hello/foo` 就会得到下面的结果

![](./images/browser.png)

## 2 csproj 文件 
首先看一下的 `csproj` 文件是怎样的：

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net5.0</TargetFramework>
  </PropertyGroup>

</Project>
```

其中的 `SDK` 指定为 `Microsoft.NET.Sdk.Web`，`SDK` 是一系列 MSBuild 的 `target` 和关联的 `task`，它们用来编译，打包和发布这个工程项目。在这里是一个 `Web` 应用程序，所以 `Microsoft.NET.Sdk.Web` 包含 `ASP.NET Core` 应用程序所需要的程序集，`build target` 和 `build task`。查看[这个文档](https://docs.microsoft.com/en-us/dotnet/core/project-sdk/overview)了解更多关于 SDK 的内容

## 3 launchSettings.json 文件

`launchSettings.json` 是在程序启动的时候加载的文件
```json
{
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true,
    "iisExpress": {
      "applicationUrl": "http://localhost:13039",
      "sslPort": 44300
    }
  },
  "profiles": {
    "IIS Express": {
      "commandName": "IISExpress",
      "launchBrowser": true,
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    "helloworld": {
      "commandName": "Project",
      "dotnetRunMessages": "true",
      "launchBrowser": true,
      "applicationUrl": "https://localhost:5001;http://localhost:5000",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

`iisSettings` 用来配置 `IIS` 的选项，`profiles` 指定在不同的启动情况下的配置。当我们使用 `dotnet run` 启动的时候，就选择 `helloworld` 这种启动整个服务，如果在 Windows 中，当我们选择 `Ctrl + F5` 启动程序时候，可以选择 `IIS` 启动程序。
但是要注意的时候，这个文件只在本地调试的时候使用。在发布服务的时候，并不会包含这个文件。

## 4 服务注册
`ASP.NET Core` 是一个 Web 服务，它是一种需要长期运行的程序。在 `.NET` 中可以被 `Host` 这个概念概况。一个 `HOST` 主要封装了一个应用程序的资源，比如
- 依赖注入（DI）
- 日志
- 配置
- `IHostService` 的实现

当 `Host` 启动的时候，它会调用注册在服务容器中的每个 `IHostService` 的实现的 `StartAsync` 方法。所以在一个 `Web` 应用程序中，就是启动一个 `HTTP Server`。那么为什么需要一个 `HOST` 来管理一个应用程序呢？答案就是控制程序的启动和优雅的关闭。

```C#
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder => webBuilder.UseStartup<Startup>());
}
```
1. CreateDefaultBuilder
- 设置当前的目录作为 `content root`
- 从不同地方加载 `Host` 的配置
    - 环境变量中以 `DOTNET_` 开头的
    - 命令行参数
- 加载应用程序的配置
    - *appsettings.json*
    - *appsettings.{Environment}.json*
    - 对于开发环境，从 `User Secret` 中加载
    - 环境变量
    - 命令行参数
- 提供 `logger` 实现者
    - Console
    - Debug
    - EventSource
    - EventLog (Window)
- 对于开发环境，开启 `scope validation` 和 `dependency validation`

2. ConfigurationWebHostDefaults
- 加载所有以 `ASPNETCORE_` 开头的环境变量
- 设置 `Kestrel` 作为 web 服务， 并且使用 `Host` 的配置信息来配置 `kestrel` 服务。
- 加载 `filter` 的中间件
- 添加 `header` 的中间件
- 允许 `IIS` 集成

## 5 Startup 文件

在 `ASP.NET Core` 应用程序中，`Startup` 文件是用来注册服务和配置应用程序。`Startup` 类不需要继承某个类或者接口，只要实现包含 `ConfigurationService` 和 `Configure` 两个方法，这两个类各自的功能如下
1. ConfigurationService 用来注册应用程序中各个服务通常使用依赖注入（Dependency Injection, DI) 
2. Configure 用来建立请求的处理的 pipeline。
