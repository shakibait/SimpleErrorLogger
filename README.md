# SimpleErrorLogger
A simple error log class for .net core +2

This is simple error logger for .net core +2 projects

# Features
- Capture http requests
- Handle runtime errors

# Installation
1. Add classes (RequestFilter.cs and ErrorHandler.cs)
2. Add your database context to startup.cs services by:
	```csharp
	public void ConfigureServices(IServiceCollection services)
	{
		.......
		services.AddScoped<IUnitOfWork, UnitOfWork> ();
		.....
	}
    ```
3. Add request filter middleware to startup.cs Configure by:
	```csharp
	public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
		....
		app.UseMiddleware(typeof(RequestFilter));
		....
	}
	```
4. Enjoy


###### Also you can use projects like ElmahCore [from here](https://github.com/ElmahCore/ElmahCore "ElmahCore")
