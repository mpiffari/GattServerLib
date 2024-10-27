using Android.Bluetooth;
using Android.Content;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using GattServerLib.Support;
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

    public Task<bool> AddServiceAsync(IBleService bleService)
    {
        BluetoothGattService androidService = new BluetoothGattService(UUID.FromString(bleService.ServiceUuid.ToString()), GattServiceType.Primary);
        // Add characteristics to the service

        foreach (var charact in bleService.Characteristics)
        {
            var properties = charact.Properties;
            var characteristic = new BluetoothGattCharacteristic(
                UUID.FromString(charact.CharacteristicUuid.ToString()),
                ToGattProperty(properties),
                ToGattPermission(properties));

            if (androidService.Characteristics.FirstOrDefault(x => x.Uuid == characteristic.Uuid) is not null)
            {
                logger.LogWarning(LoggerScope.GATT_S.EventId(),
                    "Service {S} has already a characteristic with UUID {U}",
                    bleService.ServiceUuid.ToString(),
                    charact.CharacteristicUuid.ToString());
                continue;
            }
            
            androidService.Characteristics.Add(characteristic);
        }
        
        if (gattServer is null || !gattServer.AddService(androidService))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    private GattProperty ToGattProperty(BleCharacteristicProperties properties)
    {
        GattProperty result = GattProperty.Read;

        if (properties.HasFlag(BleCharacteristicProperties.Broadcast))
        {
            result |= GattProperty.Broadcast;
        }
        if (properties.HasFlag(BleCharacteristicProperties.Read))
        {
            result |= GattProperty.Read;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.WriteWithoutResponse))
        {
            result |= GattProperty.WriteNoResponse;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.Write))
        {
            result |= GattProperty.Write;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.Indicate))
        {
            result |= GattProperty.Indicate;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.AuthenticatedSignedWrites))
        {
            result |= GattProperty.SignedWrite;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.ExtendedProperties))
        {
            result |= GattProperty.ExtendedProps;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.NotifyEncryptionRequired))
        {
            result |= GattProperty.Notify;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.IndicateEncryptionRequired))
        {
            result |= GattProperty.Indicate;
        }
        
        
        return result;
    }
    
    private GattPermission ToGattPermission(BleCharacteristicProperties properties)
    {
        GattPermission result = GattPermission.Read;

        if (properties.HasFlag(BleCharacteristicProperties.Read))
        {
            result |= GattPermission.Read;
        }
        if (properties.HasFlag(BleCharacteristicProperties.ReadEncrypted))
        {
            result |= GattPermission.ReadEncrypted;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.WriteWithoutResponse) || properties.HasFlag(BleCharacteristicProperties.Write))
        {
            result |= GattPermission.Write;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.AuthenticatedSignedWrites))
        {
            result |= GattPermission.WriteSigned;
        }
        
        return result;
    }
    
    public Task<bool> RemoveServiceAsync(Guid bleServiceUuid)
    {
        var serviceToRemove = gattServer?.Services?.FirstOrDefault(x => x.Uuid?.ToString() == bleServiceUuid.ToString());
        if (serviceToRemove == null)
        {
            return Task.FromResult(false);
        }

        if (gattServer is null || !gattServer.RemoveService(serviceToRemove))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
}