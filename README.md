# Aftermath

[![NuGet](https://img.shields.io/nuget/v/Aftermath.svg)](https://www.nuget.org/packages/Aftermath/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/ShadyNagy/Aftermath/build.yml)](https://github.com/ShadyNagy/Aftermath/actions)
[![License](https://img.shields.io/github/license/ShadyNagy/Aftermath)](https://github.com/ShadyNagy/Aftermath/blob/main/LICENSE)

Aftermath is a powerful .NET library that allows you to declare post-execution hooks for methods without modifying the original method implementation. It implements aspect-oriented programming (AOP) principles to handle cross-cutting concerns in a clean, maintainable way.

## Features

- 🔄 **Non-invasive**: Add behavior to methods without changing their implementation
- 🧩 **Interface-based**: Works with interfaces and dependency injection
- 🔌 **Easy integration**: Seamless integration with ASP.NET Core dependency injection
- ⚡ **Async-aware**: Full support for async methods (Task, Task<T>, ValueTask, ValueTask<T>)
- 🔧 **Highly configurable**: Extensive options for controlling hook behavior
- 🚀 **Performance-conscious**: Skip hooks in release mode, timeout control
- 📊 **Execution context**: Rich information about method execution passed to hooks
- 🪝 **Advanced hook features**: Parameter mapping, return value access, error policies
- 💉 **DI-friendly**: Automatic parameter resolution from services

## Installation

```bash
dotnet add package Aftermath
```

## Quick Start

1. Define your interface with hooks:

```csharp
public interface IUserService : IHookable
{
    [CallAfter(typeof(LoggingService), nameof(LoggingService.LogUserCreation))]
    Task<User> CreateUserAsync(string username, string email);
}
```

2. Register services in your startup:

```csharp
services.AddAftermath();
services.AddHookedScoped<IUserService, UserService>();
```

3. Implement your service normally:

```csharp
public class UserService : IUserService
{
    public virtual async Task<User> CreateUserAsync(string username, string email)
    {
        // Your implementation here
        return new User { Username = username, Email = email };
    }
}
```

4. Create your hook method:

```csharp
public class LoggingService
{
    public void LogUserCreation(string username, User user)
    {
        Console.WriteLine($"User created: {username} with ID {user.Id}");
    }
}
```

## How It Works

Aftermath uses dynamic proxies to intercept method calls and execute hooks after the original method completes. When you call a method on a proxied interface, Aftermath:

1. Executes the original method implementation
2. Captures execution details (parameters, return value, execution time, etc.)
3. Invokes all defined hook methods with the execution context

## Authentication Example

The example in this repository demonstrates how to use Aftermath for cross-cutting concerns in an authentication system:

```csharp
public interface IUserService : IHookable
{
    [CallAfter(typeof(IAuthenticationLogger), nameof(IAuthenticationLogger.LogAuthenticationAttemptAsync))]
    [CallAfter(typeof(SecurityAuditService), nameof(SecurityAuditService.RecordAuthenticationAttempt))]
    [CallAfter(typeof(BruteForceDetector), nameof(BruteForceDetector.CheckForBruteForceAttack))]
    [MapParameter("username", "username")]
    [MapReturnValue("success")]
    Task<bool> AuthenticateAsync(string username, string password);
}
```

In this example:

1. The `AuthenticateAsync` method performs authentication logic
2. After execution, Aftermath automatically:
   - Logs the authentication attempt
   - Records the attempt in a security audit
   - Checks for potential brute force attacks

All without modifying the authentication method itself!

### Running the Example

```csharp
// Configure services
var services = new ServiceCollection();
services.AddAftermath();
services.AddHookedScoped<IUserService, UserService>();
services.AddScoped<IUserManager, UserManager>();
services.AddSingleton<IAuthenticationLogger, AuthenticationLogger>();
services.AddSingleton<SecurityAuditService>();
services.AddSingleton<BruteForceDetector>();

// Build provider and get service
var serviceProvider = services.BuildServiceProvider();
var userService = serviceProvider.GetRequiredService<IUserService>();

// Use the service normally
var result = await userService.AuthenticateAsync("john.doe", "password");
```

## Advanced Features

### Parameter Mapping

Map source method parameters to hook method parameters:

```csharp
[CallAfter(typeof(LoggingService), nameof(LoggingService.LogAction))]
[MapParameter("userId", "user")]
public void UpdateUser(int userId, UserUpdateDto data)
```

### Injecting Custom Values

Inject custom values into hook parameters:

```csharp
[CallAfter(typeof(AuditService), nameof(AuditService.RecordAction))]
[InjectParameter("actionType", "USER_UPDATE")]
public void UpdateUser(int userId, UserUpdateDto data)
```

### Conditional Execution

Execute hooks conditionally:

```csharp
[CallAfter(typeof(NotificationService), nameof(NotificationService.SendPasswordChangeEmail))]
[ExecuteWhen(nameof(ShouldNotifyPasswordChange))]
public void ChangePassword(int userId, string newPassword)

private bool ShouldNotifyPasswordChange(MethodExecutionContext context)
{
    return context.GetParameterValue<bool>("sendNotification");
}
```

### Error Handling

Control how hook errors are handled:

```csharp
// Continue executing other hooks even if this one fails
[CallAfter(typeof(LoggingService), nameof(LoggingService.LogAction), ContinueOnError = true)]

// Stop executing hooks if this one fails
[CallAfter(typeof(CriticalService), nameof(CriticalService.ProcessData), ContinueOnError = false)]
```

### Hook Execution Order

Control the order of hook execution:

```csharp
[CallAfter(typeof(ValidationService), nameof(ValidationService.ValidateResult), Order = 1)]
[CallAfter(typeof(LoggingService), nameof(LoggingService.LogOperation), Order = 2)]
```

### Performance Optimization

Skip hooks in release mode:

```csharp
[SkipHooksInRelease]
public Task<DataResult> HeavyOperationAsync()
```

## Configuration

Customize Aftermath behavior:

```csharp
services.AddAftermath(options =>
{
    // Auto-create instances of hook target types not in DI
    options.AutoCreateInstancesNotInDI = true;
    
    // Auto-resolve parameters from DI when possible
    options.AutoResolveParametersFromDI = true;
    
    // Enable verbose logging
    options.VerboseLogging = true;
    
    // Set hook execution timeout
    options.HookExecutionTimeoutMs = 5000; // 5 seconds
    
    // Set global error policy
    options.GlobalErrorPolicy = HookErrorPolicy.ContinueWithNextHook;
});
```

## Performance Considerations

- Use `[SkipHooksInRelease]` for performance-critical methods
- Set appropriate timeout values
- Consider the overhead of proxying and intercepting
- For very high-performance scenarios, hooks may add non-trivial overhead

## License

MIT License
