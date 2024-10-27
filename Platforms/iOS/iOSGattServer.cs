using CoreBluetooth;
using Foundation;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using GattServerLib.Support;
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

    public Task<bool> AddServiceAsync(IBleService bleService)
    {
        var iosService = new CBMutableService(CBUUID.FromString(bleService.ServiceUuid.ToString()), true);
        // Add characteristics to the service
        
        foreach (var charact in bleService.Characteristics)
        {
            var properties = charact.Properties;
            iosService?.Characteristics?.Append(new CBMutableCharacteristic(
                CBUUID.FromString(charact.CharacteristicUuid.ToString()),
                ToGattProperty(properties),
                null,
                ToGattPermission(properties)));
        }
        
        peripheralManager.AddService(iosService);
        return Task.FromResult<bool>(true);
    }
    
    private CBCharacteristicProperties ToGattProperty(BleCharacteristicProperties properties)
    {
        CBCharacteristicProperties result = CBCharacteristicProperties.Read;

        if (properties.HasFlag(BleCharacteristicProperties.Broadcast))
        {
            result |= CBCharacteristicProperties.Broadcast;
        }
        if (properties.HasFlag(BleCharacteristicProperties.Read))
        {
            result |= CBCharacteristicProperties.Read;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.WriteWithoutResponse))
        {
            result |= CBCharacteristicProperties.WriteWithoutResponse;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.Write))
        {
            result |= CBCharacteristicProperties.Write;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.Indicate))
        {
            result |= CBCharacteristicProperties.Indicate;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.AuthenticatedSignedWrites))
        {
            result |= CBCharacteristicProperties.AuthenticatedSignedWrites;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.ExtendedProperties))
        {
            result |= CBCharacteristicProperties.ExtendedProperties;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.NotifyEncryptionRequired))
        {
            result |= CBCharacteristicProperties.Notify;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.IndicateEncryptionRequired))
        {
            result |= CBCharacteristicProperties.Indicate;
        }
        
        
        return result;
    }
    
    private CBAttributePermissions ToGattPermission(BleCharacteristicProperties properties)
    {
        CBAttributePermissions result = CBAttributePermissions.Readable;

        if (properties.HasFlag(BleCharacteristicProperties.Read))
        {
            result |= CBAttributePermissions.Readable;
        }
        if (properties.HasFlag(BleCharacteristicProperties.ReadEncrypted))
        {
            result |= CBAttributePermissions.ReadEncryptionRequired;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.WriteWithoutResponse) || properties.HasFlag(BleCharacteristicProperties.Write))
        {
            result |= CBAttributePermissions.Writeable;
        }
        
        if (properties.HasFlag(BleCharacteristicProperties.AuthenticatedSignedWrites))
        {
            result |= CBAttributePermissions.WriteEncryptionRequired;
        }
        
        return result;
    }

    public Task<bool> RemoveServiceAsync(Guid bleServiceUuid)
    {
        // TODO()
        return Task.FromResult<bool>(true);
    }
}