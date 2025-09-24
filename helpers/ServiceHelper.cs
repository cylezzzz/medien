using System;

namespace AiMediaGenerator;

public static class ServiceHelper
{
    public static IServiceProvider Services { get; private set; } = default!;

    public static void Initialize(IServiceProvider services)
        => Services = services;

    public static T GetService<T>() where T : notnull
        => (T)Services.GetService(typeof(T))!;
}
