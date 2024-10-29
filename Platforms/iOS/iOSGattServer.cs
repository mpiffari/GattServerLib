using CoreBluetooth;
using CoreFoundation;
using Foundation;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

public class iOSGattServer : IGattServer
{
    private CBPeripheralManager peripheralManager;
    private iOSPeripheralManagerDelegate peripheralManagerDelegate;
    private ILogger logger;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    private TaskCompletionSource<bool> OnStateUpdatedTcs = new();
    private TaskCompletionSource<bool> OnWriteRequestsReceivedTcs = new();
    private TaskCompletionSource<bool> OnReadRequestReceivedTcs = new();
    
    private List<CBService> nativeServices = new();

    public Func<(string sUuid, string cUuid, int offset), Task<(bool, byte[])>>? onRead { get; set; }
    
    public Task InitializeAsync(ILogger logger)
    {
        this.logger = logger;
        
        peripheralManagerDelegate = new iOSPeripheralManagerDelegate(logger);
        peripheralManagerDelegate.OnAdvertisingStarted += error => { };
        peripheralManagerDelegate.OnServiceAdded += (service, error) => { };
        peripheralManagerDelegate.OnStateUpdated += (sender, s) => { };
        peripheralManagerDelegate.OnWriteRequestsReceived += requests => { }; 
        peripheralManagerDelegate.OnReadRequestReceived += async (peripheral, request) =>
        {
            var sUuid = request.Characteristic.Service.UUID.ToString();
            var cUuid = request.Characteristic.UUID.ToString();
            (bool isSuccess, byte[] data) res = await onRead?.Invoke((sUuid, cUuid, request.Offset.ToInt32()));

            if (res.isSuccess)
            {
                request.Value = NSData.FromArray(res.data);
                peripheral.RespondToRequest(request, CBATTError.ReadNotPermitted);
            }
            else
            {
                peripheral.RespondToRequest(request, CBATTError.ReadNotPermitted);
            }
        };
        
        peripheralManager = new CBPeripheralManager(peripheralManagerDelegate, DispatchQueue.MainQueue);
        nativeServices.Clear();
        
        return Task.CompletedTask;
    }

    public async Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StartAdvertisingAsync iOS");
        
        OnAdvertisingStartedTcs = new();
        if (peripheralManager.Advertising)
        {
            return false;
        }
        
        options ??= new BleAdvOptions();
        var opts = new StartAdvertisingOptions();
        if (options.LocalName != null)
        {
            opts.LocalName = options.LocalName;
        }

        if (options.ServiceUuids.Length > 0)
        {
            opts.ServicesUUID = options
                .ServiceUuids
                .Select(CBUUID.FromString)
                .ToArray();
        }
        
        peripheralManager.StartAdvertising(opts);
        var result = await OnAdvertisingStartedTcs.Task;
        return result;
    }

    public Task StopAdvertisingAsync()
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StopAdvertisingAsync iOS");
        peripheralManager.StopAdvertising();
        return Task.FromResult<bool>(true);
    }

    public async Task<bool> AddServiceAsync(IBleService bleService)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "AddServiceAsync iOS");
        OnServiceAddedTcs = new();
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
        
        if (nativeServices is null || nativeServices.Count == 0)
        {
            nativeServices = new List<CBService>();
        }
        nativeServices.Add(iosService);
        
        var result = await OnServiceAddedTcs.Task;
        return result;
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
    
    public Task<bool> RespondToReadRequestAsync(string serviceUuid, string characteristicUuid, byte[] value)
    {
        var sUuid = CBUUID.FromString(serviceUuid);
        var cUuid = CBUUID.FromString(characteristicUuid);

        var c = nativeServices.FirstOrDefault(x => x.UUID == sUuid)?.Characteristics?.FirstOrDefault(x => x.UUID == cUuid);
        if (c is null)
        {
            return Task.FromResult(false);
        }
        else
        {
            c.Value = NSData.FromArray(value);
            return Task.FromResult(true);
        }
    }
}