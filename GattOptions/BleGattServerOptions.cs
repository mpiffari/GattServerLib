using Microsoft.Extensions.Logging;

namespace GattServerLib.GattOptions;

public class BleGattServerOptions
{
    public bool EnableLogging { get; set; } = true;
    public LogLevel MinimumLogLevel { get; set; } = LogLevel.Information;
    public bool AutoRequestPermissions { get; set; } = true;
}