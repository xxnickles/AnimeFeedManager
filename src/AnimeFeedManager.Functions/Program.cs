﻿using AnimeFeedManager.DI;
using AnimeFeedManager.Functions.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SendGrid.Extensions.DependencyInjection;

static SendGridConfiguration GetSendGridConfiguration()
{
    var defaultFromEmail = Environment.GetEnvironmentVariable("FromEmail") ?? "test@test.com";
    var defaultFromName = Environment.GetEnvironmentVariable("FromName") ?? "Test";
    bool.TryParse(Environment.GetEnvironmentVariable("Sandbox"), out var sandbox);
    return new SendGridConfiguration(defaultFromEmail, defaultFromName, sandbox);
}

var storageConnection = Environment.GetEnvironmentVariable("AzureWebJobsStorage") ?? string.Empty;
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        s.AddHttpClient();
        s.RegisterStorage(storageConnection);
        s.RegisterPuppeteer();
        s.RegisterAppServices();
        s.RegisterApplicationServices();
        s.AddSendGrid(options => options.ApiKey = Environment.GetEnvironmentVariable("SendGridKey"));
        s.AddSingleton<ISendGridConfiguration>(GetSendGridConfiguration());
    })
    .Build();

await host.RunAsync();



