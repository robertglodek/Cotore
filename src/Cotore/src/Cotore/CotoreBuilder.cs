﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace Cotore;

public sealed class CotoreBuilder : ICotoreBuilder
{
    private readonly ConcurrentDictionary<string, bool> _registry = new();
    public IServiceCollection Services { get; init; } = null!;
    public IConfiguration Configuration { get; init; } = null!;

    private CotoreBuilder(IServiceCollection services, IConfiguration configuration)
    {
        Services = services;
        Configuration = configuration;
    }

    public static ICotoreBuilder Create(IServiceCollection services, IConfiguration appConfiguration)
        => new CotoreBuilder(services, appConfiguration);

    public bool TryRegister(string name)
        => _registry.TryAdd(name, true);
}
