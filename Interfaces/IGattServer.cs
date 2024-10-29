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
    
    Func<(string sUuid, string cUuid, int offset), Task<(bool, byte[])>>? onRead { get; set; }
}