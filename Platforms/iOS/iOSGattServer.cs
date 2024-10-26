using CoreBluetooth;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using Java.Util;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

public class iOSGattServer : IGattServer
{
    private CBPeripheralManager peripheralManager;
    private iOSPeripheralManagerDelegate peripheralManagerDelegate;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    private TaskCompletionSource<bool> OnStateUpdatedTcs = new();
    private TaskCompletionSource<bool> OnWriteRequestsReceivedTcs = new();
    private TaskCompletionSource<bool> OnReadRequestReceivedTcs = new();
    
    public Task InitializeAsync(ILogger logger)
    {
        peripheralManager = new CBPeripheralManager();
        
        peripheralManagerDelegate = new iOSPeripheralManagerDelegate(logger);
        peripheralManagerDelegate.OnAdvertisingStarted += error => { }; 
        peripheralManagerDelegate.OnServiceAdded += (service, error) => 
        peripheralManagerDelegate.OnStateUpdated += (sender, s) => { };
        peripheralManagerDelegate.OnWriteRequestsReceived += requests => { }; 
        peripheralManagerDelegate.OnReadRequestReceived += request => { }; 
        
        return Task.CompletedTask;
    }

    public Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        if (peripheralManager.Advertising)
        {
            return Task.FromResult(false);
        }
        
        options ??= new BleAdvOptions();
        var opts = new StartAdvertisingOptions();
        if (options.LocalName != null)
            opts.LocalName = options.LocalName;

        if (options.ServiceUuids.Length > 0)
        {
            opts.ServicesUUID = options
                .ServiceUuids
                .Select(CBUUID.FromString)
                .ToArray();
        }
        
        peripheralManager.StartAdvertising(opts);
        return Task.FromResult<bool>(true);
    }

    public Task StopAdvertisingAsync()
    {
        peripheralManager.StopAdvertising();
        return Task.FromResult<bool>(true);
    }

    public Task<bool> AddServiceAsync(UUID uuid)
    {
        var iosService = new CBMutableService(CBUUID.FromString(uuid.ToString()), true);
        // Add characteristics to the service.
        peripheralManager.AddService(iosService);
        return Task.FromResult<bool>(true);
    }

    public Task<bool> RemoveServiceAsync(UUID uuid)
    {
        return Task.FromResult<bool>(true);
    }
}