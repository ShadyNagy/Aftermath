using Aftermath.Hooks;
using Castle.DynamicProxy;

namespace Aftermath;

/// <summary>
/// Generates proxies for objects to intercept method calls and execute hooks.
/// </summary>
public class ProxyGenerator
{
	private readonly HookManager _hookManager;
	private readonly IProxyGenerator _proxyGenerator;

	/// <summary>
	/// Initializes a new instance of the <see cref="ProxyGenerator"/> class.
	/// </summary>
	/// <param name="hookManager">The hook manager for executing hooks.</param>
	public ProxyGenerator(HookManager hookManager)
	{
		_hookManager = hookManager ?? throw new ArgumentNullException(nameof(hookManager));
		_proxyGenerator = new Castle.DynamicProxy.ProxyGenerator();
	}

	/// <summary>
	/// Creates a proxy for the specified target that intercepts method calls to execute hooks.
	/// </summary>
	/// <typeparam name="TInterface">The interface type that the proxy will implement.</typeparam>
	/// <param name="target">The target object to proxy.</param>
	/// <returns>A proxy that implements <typeparamref name="TInterface"/> and executes hooks after method calls.</returns>
	public TInterface CreateProxy<TInterface>(TInterface target) where TInterface : class
	{
		if (target == null)
			throw new ArgumentNullException(nameof(target));

		var interceptor = new HookInterceptor(_hookManager);

		// Create a proxy that intercepts calls to the target object
		return _proxyGenerator.CreateInterfaceProxyWithTarget(target, interceptor);
	}

	/// <summary>
	/// Creates a proxy for the specified target type that intercepts method calls to execute hooks.
	/// </summary>
	/// <typeparam name="TInterface">The interface type that the proxy will implement.</typeparam>
	/// <typeparam name="TImplementation">The implementation type that will be instantiated.</typeparam>
	/// <param name="args">Optional constructor arguments for the implementation type.</param>
	/// <returns>A proxy that implements <typeparamref name="TInterface"/> and executes hooks after method calls.</returns>
	public TInterface CreateProxy<TInterface, TImplementation>(params object[] args)
			where TInterface : class
			where TImplementation : class, TInterface
	{
		var target = (TImplementation)Activator.CreateInstance(typeof(TImplementation), args);
		return CreateProxy<TInterface>(target);
	}

	/// <summary>
	/// Creates a proxy for the specified class type.
	/// </summary>
	/// <typeparam name="T">The class type that will be proxied.</typeparam>
	/// <returns>A proxy of type <typeparamref name="T"/> that executes hooks after method calls.</returns>
	public T CreateClassProxy<T>() where T : class
	{
		var interceptor = new HookInterceptor(_hookManager);

		// Use the correct overload that only takes interceptors
		return _proxyGenerator.CreateClassProxy<T>(interceptor);
	}

	/// <summary>
	/// Creates a proxy for the specified class type with constructor arguments.
	/// </summary>
	/// <typeparam name="T">The class type that will be proxied.</typeparam>
	/// <param name="args">Constructor arguments for the class.</param>
	/// <returns>A proxy of type <typeparamref name="T"/> that executes hooks after method calls.</returns>
	public T CreateClassProxyWithConstructorArgs<T>(params object[] args) where T : class
	{
		// Create the instance first with the constructor arguments
		var target = (T)Activator.CreateInstance(typeof(T), args);

		// Then create a proxy with that target
		return CreateClassProxy<T>(target);
	}

	/// <summary>
	/// Creates a proxy for the specified target.
	/// </summary>
	/// <typeparam name="T">The class type that will be proxied.</typeparam>
	/// <param name="target">The target object to proxy.</param>
	/// <returns>A proxy of type <typeparamref name="T"/> that executes hooks after method calls.</returns>
	public T CreateClassProxy<T>(T target) where T : class
	{
		var interceptor = new HookInterceptor(_hookManager);
		return _proxyGenerator.CreateClassProxyWithTarget<T>(target, interceptor);
	}

	/// <summary>
	/// Creates a proxy for the specified interface type and target instance.
	/// </summary>
	/// <param name="interfaceType">The interface type that the proxy will implement.</param>
	/// <param name="target">The target object to proxy.</param>
	/// <returns>A proxy that implements the interface and executes hooks after method calls.</returns>
	public object CreateProxy(Type interfaceType, object target)
	{
		if (interfaceType == null)
			throw new ArgumentNullException(nameof(interfaceType));

		if (target == null)
			throw new ArgumentNullException(nameof(target));

		if (!interfaceType.IsInterface)
			throw new ArgumentException("Type must be an interface", nameof(interfaceType));

		if (!interfaceType.IsAssignableFrom(target.GetType()))
			throw new ArgumentException($"Target does not implement interface {interfaceType.FullName}", nameof(target));

		var interceptor = new HookInterceptor(_hookManager);

		return _proxyGenerator.CreateInterfaceProxyWithTarget(interfaceType, target, interceptor);
	}
}