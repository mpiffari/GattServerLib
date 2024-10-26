using System.ComponentModel;
using Microsoft.Extensions.Logging;

namespace GattServerLib.Support;

internal enum LoggerScope
{
    [Description("BLE")]
    GATT_S
}

internal static class LoggerExtensions
{
    internal static bool IsEnabled(this LogLevel logLevel)
    {
        // Select which levels keep active
        switch ( logLevel )
        {
            case LogLevel.Trace:
                return false;
            case LogLevel.Critical:
                return true;
            case LogLevel.Debug:
                return true;
            case LogLevel.Error:
                return true;
            case LogLevel.Information:
                return true;
            case LogLevel.None:
                return true;
            case LogLevel.Warning:
                return true;
            default:
                return false;
        };
    }

    internal static bool IsEnabled(this LoggerScope scope)
    {
        // Select which category keep active
        switch ( scope )
        {
            case LoggerScope.GATT_S:
                return true;
            
            default:
                return false;
        };
    }

    internal static EventId EventId(this LoggerScope scope)
    {
        return new EventId((int)scope, scope.ToString());
    }

    internal static LoggerScope? Scope(this EventId eventId)
    {
        if (eventId.Name == LoggerScope.GATT_S.ToString())
        {
            return LoggerScope.GATT_S;
        }

        return null;
    }
}