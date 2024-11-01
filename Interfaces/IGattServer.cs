using GattServerLib.GattOptions;
using Microsoft.Extensions.Logging;

namespace GattServerLib.Interfaces;

public interface IGattServer
{
    Task InitializeAsync(ILogger log);
    
    Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null);
    Task StopAdvertisingAsync();
    
    Task<bool> AddServiceAsync(IBleService bleService);
    Task<bool> RemoveServiceAsync(Guid bleServiceUuid);

    Task<bool> SendNotification(string cUuid, byte[] value);
    
    Func<(string cUuid, int offset), (bool, byte[])>? OnRead { get; set; } 
    Func<(string cUuid, byte[] valueWritten), (bool isSuccess, bool notificationNeeded, string notificationUuid)>? OnWrite { get; set; }
}