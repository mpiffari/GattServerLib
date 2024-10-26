using Android.Bluetooth;
using Android.Content;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using Java.Util;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

public class AndroidGattServer : IGattServer
{
    private BluetoothManager? bluetoothManager;
    private BluetoothGattServer? gattServer;
    private ILogger logger;
    
    private GattServerCallback gattServerCallback;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    private TaskCompletionSource<bool> OnStateUpdatedTcs = new();
    private TaskCompletionSource<bool> OnWriteRequestsReceivedTcs = new();
    private TaskCompletionSource<bool> OnReadRequestReceivedTcs = new();
    
    public Task InitializeAsync(ILogger logger)
    {
        bluetoothManager = (BluetoothManager?)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
        gattServerCallback = new GattServerCallback(logger);
        this.logger = logger;
        
        logger.LogDebug("InitializeAsync Android");
        return Task.CompletedTask;
    }

    public Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        gattServer = bluetoothManager?.OpenGattServer(Android.App.Application.Context, gattServerCallback);
        logger.LogDebug("StartAdvertisingAsync Android");
        return Task.FromResult(gattServer is not null);
    }

    public Task StopAdvertisingAsync()
    {
        foreach (var bluetoothGattService in gattServer.Services)
        {
           bluetoothGattService.Dispose();
        }
        gattServer.Services.Clear();
        gattServer.ClearServices();
        gattServer.Dispose();
        return Task.CompletedTask;
    }
    
    public Task<bool> AddServiceAsync(UUID uuid)
    {       
        BluetoothGattService androidService = new BluetoothGattService(uuid, GattServiceType.Primary);
        // Add characteristics to the service.
        if (gattServer is null || !gattServer.AddService(androidService))
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }

    public Task<bool> RemoveServiceAsync(UUID uuid)
    {
        var serviceToRemove = gattServer.Services?.FirstOrDefault(x => x.Uuid == uuid);
        if (serviceToRemove == null)
        {
            return Task.FromResult(false);
        }

        if (!gattServer.RemoveService(serviceToRemove))
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }
}