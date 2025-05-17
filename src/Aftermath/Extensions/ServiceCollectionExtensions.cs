using Aftermath.Hooks;
using Microsoft.Extensions.DependencyInjection;

namespace Aftermath.Extensions;

/// <summary>
/// Contains extension methods for setting up Aftermath in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Adds Aftermath services to the specified <see cref="IServiceCollection"/>.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddAftermath(this IServiceCollection services)
	{
		return services.AddAftermath(options => { });
	}

	/// <summary>
	/// Adds Aftermath services to the specified <see cref="IServiceCollection"/> with custom options.
	/// </summary>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="configure">A delegate to configure the <see cref="AftermathOptions"/>.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddAftermath(this IServiceCollection services, Action<AftermathOptions> configure)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		if (configure == null)
			throw new ArgumentNullException(nameof(configure));

		services.AddLogging();

		var options = new AftermathOptions();
		configure(options);
		services.AddSingleton(options);

		services.AddSingleton<HookManager>();

		services.AddSingleton<ProxyGenerator>();

		return services;
	}

	/// <summary>
	/// Registers a singleton service of the type specified in <typeparamref name="TInterface"/>
	/// with a proxied implementation of the type specified in <typeparamref name="TImplementation"/>.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to register.</typeparam>
	/// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookedSingleton<TInterface, TImplementation>(this IServiceCollection services)
			where TInterface : class
			where TImplementation : class, TInterface
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		services.AddSingleton<TImplementation>();
		services.AddSingleton(sp =>
		{
			var implementation = sp.GetRequiredService<TImplementation>();
			var proxyGenerator = sp.GetRequiredService<ProxyGenerator>();
			return proxyGenerator.CreateProxy<TInterface>(implementation);
		});

		return services;
	}

	/// <summary>
	/// Registers a singleton service of the type specified in <typeparamref name="TInterface"/>
	/// with a proxied implementation factory.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to register.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="implementationFactory">The factory that creates the service.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookedSingleton<TInterface>(
			this IServiceCollection services,
			Func<IServiceProvider, TInterface> implementationFactory)
			where TInterface : class
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		if (implementationFactory == null)
			throw new ArgumentNullException(nameof(implementationFactory));

		services.AddSingleton(sp =>
		{
			var implementation = implementationFactory(sp);
			var proxyGenerator = sp.GetRequiredService<ProxyGenerator>();
			return proxyGenerator.CreateProxy(implementation);
		});

		return services;
	}

	/// <summary>
	/// Registers a scoped service of the type specified in <typeparamref name="TInterface"/>
	/// with a proxied implementation of the type specified in <typeparamref name="TImplementation"/>.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to register.</typeparam>
	/// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookedScoped<TInterface, TImplementation>(this IServiceCollection services)
			where TInterface : class
			where TImplementation : class, TInterface
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		services.AddScoped<TImplementation>();
		services.AddScoped(sp =>
		{
			var implementation = sp.GetRequiredService<TImplementation>();
			var proxyGenerator = sp.GetRequiredService<ProxyGenerator>();
			return proxyGenerator.CreateProxy<TInterface>(implementation);
		});

		return services;
	}

	/// <summary>
	/// Registers a scoped service of the type specified in <typeparamref name="TInterface"/>
	/// with a proxied implementation factory.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to register.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="implementationFactory">The factory that creates the service.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookedScoped<TInterface>(
			this IServiceCollection services,
			Func<IServiceProvider, TInterface> implementationFactory)
			where TInterface : class
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		if (implementationFactory == null)
			throw new ArgumentNullException(nameof(implementationFactory));

		services.AddScoped(sp =>
		{
			var implementation = implementationFactory(sp);
			var proxyGenerator = sp.GetRequiredService<ProxyGenerator>();
			return proxyGenerator.CreateProxy(implementation);
		});

		return services;
	}

	/// <summary>
	/// Registers a transient service of the type specified in <typeparamref name="TInterface"/>
	/// with a proxied implementation of the type specified in <typeparamref name="TImplementation"/>.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to register.</typeparam>
	/// <typeparam name="TImplementation">The type of the implementation to use.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookedTransient<TInterface, TImplementation>(this IServiceCollection services)
			where TInterface : class
			where TImplementation : class, TInterface
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		services.AddTransient<TImplementation>();
		services.AddTransient(sp =>
		{
			var implementation = sp.GetRequiredService<TImplementation>();
			var proxyGenerator = sp.GetRequiredService<ProxyGenerator>();
			return proxyGenerator.CreateProxy<TInterface>(implementation);
		});

		return services;
	}

	/// <summary>
	/// Registers a transient service of the type specified in <typeparamref name="TInterface"/>
	/// with a proxied implementation factory.
	/// </summary>
	/// <typeparam name="TInterface">The type of the service to register.</typeparam>
	/// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
	/// <param name="implementationFactory">The factory that creates the service.</param>
	/// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookedTransient<TInterface>(
			this IServiceCollection services,
			Func<IServiceProvider, TInterface> implementationFactory)
			where TInterface : class
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		if (implementationFactory == null)
			throw new ArgumentNullException(nameof(implementationFactory));

		services.AddTransient(sp =>
		{
			var implementation = implementationFactory(sp);
			var proxyGenerator = sp.GetRequiredService<ProxyGenerator>();
			return proxyGenerator.CreateProxy(implementation);
		});

		return services;
	}
}