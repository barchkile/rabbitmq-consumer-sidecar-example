using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RabbitMQ.Client;

namespace RabbitMQ.Consumer;

public static class BasicPropertiesExtensions
{
    private const string RabbitHeaderPrefix = "x-";
    private const string DeathHeaderName = "x-death";

    public static long DeathCount(this IBasicProperties basicProperties)
    {
        var deathsPerExchange = basicProperties.DeathsPerExchange();
        if (deathsPerExchange is null or {Count: 0})
        {
            return 0;
        }

        return Convert.ToInt64(
            deathsPerExchange!.First()
                .GetValueOrDefault("count", 0)
        );
    }

    public static IReadOnlyDictionary<string, string> NonRabbitHeaders(this IBasicProperties basicProperties)
        => basicProperties?.Headers
            ?.Where(header => !header.Key.StartsWith(RabbitHeaderPrefix))
            .ToDictionary(_ => _.Key, _ => GetStringHeaderValue(_.Value));
    
    public static string ToUtf8(this byte[] bytes)
        => bytes == null
            ? null
            : Encoding.UTF8.GetString(bytes);

    private static string GetStringHeaderValue(object headerValue)
        => headerValue switch
        {
            byte[] byteArray => byteArray.ToUtf8(),
            string stringValue => stringValue,
            _ => headerValue?.ToString()
        };
    
    private static List<Dictionary<string, object>> DeathsPerExchange(this IBasicProperties basicProperties)
        => basicProperties?.Headers is { } headers && headers.ContainsKey(DeathHeaderName)
            ? (headers[DeathHeaderName] as List<object>)?.Select(_ => _ as Dictionary<string, object>)
            .Where(_ => _ != null)
            .ToList()
            : null;
}