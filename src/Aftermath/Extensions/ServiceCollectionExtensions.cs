using Aftermath.Hooks;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

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

	/// <summary>
	/// Scans the specified assemblies for types that implement IHookable and registers them with the appropriate proxy.
	/// </summary>
	/// <param name="services">The service collection to add services to.</param>
	/// <param name="assemblies">The assemblies to scan for hookable types.</param>
	/// <param name="lifetime">The service lifetime to use for registration.</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookableFromAssemblies(
			this IServiceCollection services,
			IEnumerable<Assembly> assemblies,
			ServiceLifetime lifetime = ServiceLifetime.Scoped)
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		if (assemblies == null)
			throw new ArgumentNullException(nameof(assemblies));

		foreach (var assembly in assemblies)
		{
			var hookableTypes = assembly.GetExportedTypes()
					.Where(t => t.IsClass && !t.IsAbstract && typeof(IHookable).IsAssignableFrom(t))
					.ToList();

			var hookableInterfaces = assembly.GetExportedTypes()
					.Where(t => t.IsInterface && t != typeof(IHookable) && typeof(IHookable).IsAssignableFrom(t))
					.ToList();

			foreach (var hookableType in hookableTypes)
			{
				var interfaces = hookableType.GetInterfaces()
						.Where(i => typeof(IHookable).IsAssignableFrom(i) && i != typeof(IHookable))
						.ToList();

				if (interfaces.Count > 0)
				{
					foreach (var interfaceType in interfaces)
					{
						RegisterHookableServiceWithInterface(services, hookableType, interfaceType, lifetime);
					}
				}
				else
				{
					RegisterHookableServiceWithImpl(services, hookableType, lifetime);
				}
			}

			foreach (var hookableInterface in hookableInterfaces)
			{
				// Find all types that implement this interface
				var implementingTypes = assembly.GetExportedTypes()
						.Where(t => t.IsClass && !t.IsAbstract && hookableInterface.IsAssignableFrom(t))
						.ToList();

				// If there's exactly one implementation, register it
				if (implementingTypes.Count == 1)
				{
					RegisterHookableServiceWithInterface(services, implementingTypes[0], hookableInterface, lifetime);
				}
			}
		}

		return services;
	}

	/// <summary>
	/// Registers all services that implement IHookable from the calling assembly.
	/// </summary>
	/// <param name="services">The service collection to add services to.</param>
	/// <param name="lifetime">The service lifetime to use for registration.</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookable(
			this IServiceCollection services,
			ServiceLifetime lifetime = ServiceLifetime.Scoped)
	{
		return AddHookableFromAssemblies(services, new[] { Assembly.GetCallingAssembly() }, lifetime);
	}

	/// <summary>
	/// Registers a service that implements IHookable with the appropriate proxy.
	/// </summary>
	/// <typeparam name="TService">The service type to register.</typeparam>
	/// <param name="services">The service collection to add services to.</param>
	/// <param name="lifetime">The service lifetime to use for registration.</param>
	/// <returns>The service collection so that additional calls can be chained.</returns>
	public static IServiceCollection AddHookable<TService>(
			this IServiceCollection services,
			ServiceLifetime lifetime = ServiceLifetime.Scoped)
			where TService : class, IHookable
	{
		if (services == null)
			throw new ArgumentNullException(nameof(services));

		var serviceType = typeof(TService);

		if (serviceType.IsInterface)
		{
			var implementingTypes = AppDomain.CurrentDomain.GetAssemblies()
					.SelectMany(a => {
						try { return a.GetExportedTypes(); }
						catch { return Type.EmptyTypes; }
					})
					.Where(t => t.IsClass && !t.IsAbstract && serviceType.IsAssignableFrom(t))
					.ToList();

			if (implementingTypes.Count == 1)
			{
				RegisterHookableServiceWithInterface(services, implementingTypes[0], serviceType, lifetime);
			}
			else if (implementingTypes.Count == 0)
			{
				throw new InvalidOperationException($"No implementation found for interface {serviceType.FullName}");
			}
			else
			{
				throw new InvalidOperationException($"Multiple implementations found for interface {serviceType.FullName}");
			}
		}
		else
		{
			RegisterHookableServiceWithImpl(services, serviceType, lifetime);
		}

		return services;
	}

	private static void RegisterHookableServiceWithInterface(
			IServiceCollection services,
			Type implementationType,
			Type interfaceType,
			ServiceLifetime lifetime)
	{
		if (services.Any(sd => sd.ServiceType == interfaceType))
			return;

		var implServiceDescriptor = new ServiceDescriptor(
				implementationType,
				implementationType,
				lifetime);
		services.Add(implServiceDescriptor);

		var interfaceServiceDescriptor = new ServiceDescriptor(
				interfaceType,
				provider =>
				{
					var implementation = provider.GetRequiredService(implementationType);
					var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
					var proxiedService = proxyGenerator.CreateProxy(interfaceType, implementation);
					return proxiedService;
				},
				lifetime);
		services.Add(interfaceServiceDescriptor);
	}

	private static void RegisterHookableServiceWithImpl(
			IServiceCollection services,
			Type implementationType,
			ServiceLifetime lifetime)
	{
		if (services.Any(sd => sd.ServiceType == implementationType))
			return;

		var serviceDescriptor = new ServiceDescriptor(
				implementationType,
				provider =>
				{
					var proxyGenerator = provider.GetRequiredService<ProxyGenerator>();
					var proxiedService = proxyGenerator.CreateClassProxy(implementationType);
					return proxiedService;
				},
				lifetime);
		services.Add(serviceDescriptor);
	}
}