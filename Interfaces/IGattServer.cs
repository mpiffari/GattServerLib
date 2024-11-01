using GattServerLib.GattOptions;
using Microsoft.Extensions.Logging;

namespace GattServerLib.Interfaces;

public interface IGattServer
{
    Task InitializeAsync(ILogger logger);
    Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null);
    Task StopAdvertisingAsync();
    Task<bool> AddServiceAsync(IBleService bleService);
    Task<bool> RemoveServiceAsync(Guid bleServiceUuid);
    
    Func<(string cUuid, int offset), Task<(bool, byte[])>>? OnReadAsync { get; set; }
    Func<(string cUuid, int offset), (bool, byte[])>? OnRead { get; set; }
}