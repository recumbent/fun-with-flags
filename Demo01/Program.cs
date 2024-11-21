﻿// Fun with Flags Demo 02
using Microsoft.Extensions.Configuration;

IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

Console.WriteLine("Hello, World!");

if (configuration["FeatureManagement:ShinyNewFeature"] == "true")
{
    Console.WriteLine($"The time now is: {DateTime.Now}");
}