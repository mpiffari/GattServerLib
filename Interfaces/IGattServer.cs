using GattServerLib.GattOptions;
using Microsoft.Extensions.Logging;

namespace GattServerLib.Interfaces;

public interface IGattServer
{ 
    /// <summary>
    /// 
    /// </summary>
    Func<string, bool>? OnConnectionStateChanged { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    Func<(string cUuid, int offset), (bool, byte[])>? OnRead { get; set; } 
    
    /// <summary>
    /// 
    /// </summary>
    Func<(string cUuid, byte[] valueWritten), (bool isSuccess, bool notificationNeeded, string notificationUuid)>? OnWrite { get; set; }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="log"></param>
    /// <returns></returns>
    Task InitializeAsync(ILogger log);
    
    /// <summary>
    /// /
    /// </summary>
    /// <param name="options"></param>
    /// <returns></returns>
    Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null);
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task StopAdvertisingAsync();
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bleService"></param>
    /// <returns></returns>
    Task<bool> AddServiceAsync(IBleService bleService);
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="bleServiceUuid"></param>
    /// <returns></returns>
    Task<bool> RemoveServiceAsync(Guid bleServiceUuid);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cUuid"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    Task<bool> SendNotification(string cUuid, byte[] value);
}