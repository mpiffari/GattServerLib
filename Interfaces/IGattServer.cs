using GattServerLib.GattOptions;
using Java.Util;
using Microsoft.Extensions.Logging;

namespace GattServerLib.Interfaces;

public interface IGattServer
{
    Task InitializeAsync(ILogger logger);
    Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null);
    Task StopAdvertisingAsync();
    Task<bool> AddServiceAsync(UUID uuid);
    Task<bool> RemoveServiceAsync(UUID uuid);
}