using GattServerLib.GattOptions;
using GattServerLib.Support;
using Java.Util;
using Microsoft.Extensions.Logging;

namespace GattServerLib.Interfaces;

public interface IGattServer
{
    Task<BleAccessState> RequestAccess(bool advertise = true, bool connect = true);
    BleAccessState AdvertisingAccessStatus { get; }
    BleAccessState GattAccessStatus { get; }
    
    Task InitializeAsync(ILogger logger);
    Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null);
    Task StopAdvertisingAsync();
    Task<bool> AddServiceAsync(UUID uuid);
    Task<bool> RemoveServiceAsync(UUID uuid);
}