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
    private IOsPeripheralManagerDelegate peripheralManagerDelegate;
    private ILogger logger;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    
    private List<CBService> nativeServices = new();
    
    /// <inheritdoc />
    public Func<string, bool>? OnConnectionStateChanged { get; set; }
    
    /// <inheritdoc />
    public Func<(string cUuid, int offset), (bool, byte[])>? OnRead { get; set; }
    
    /// <inheritdoc />
    public Func<(string cUuid, byte[] valueWritten), (bool isSuccess, bool notificationNeeded, string notificationUuid)>? OnWrite { get; set; }
    
    /// <inheritdoc />
    public Task InitializeAsync(ILogger log)
    {
        logger = log;
        
        peripheralManagerDelegate = new IOsPeripheralManagerDelegate(log);
        peripheralManagerDelegate.OnAdvertisingStarted += error => { };
        peripheralManagerDelegate.OnServiceAdded += (service, error) => { };
        peripheralManagerDelegate.OnStateUpdated += peripheral =>
        {
            if (OnConnectionStateChanged is not null)
            {
                OnConnectionStateChanged(peripheral.State.ToString());
            }
        };
        peripheralManagerDelegate.OnWriteRequestsReceived += (peripheral, request) =>
        {
            if (OnWrite is null)
            {
                log.LogError(LoggerScope.GATT_S.EventId(), "iOSGattServer - OnWriteRequestsReceived failed due null OnWrite Func");
                peripheral.RespondToRequest(request, CBATTError.ReadNotPermitted);
                return;
            }
            
            var cUuid = request.Characteristic.UUID.ToString();
            (bool isSuccess, bool notificationNeeded, string notificationUuid) res = OnWrite((cUuid, request.Value.ToArray()));
            
            if (request.Characteristic.Properties == CBCharacteristicProperties.Write)
            {
                peripheral.RespondToRequest(request, res.isSuccess ? CBATTError.Success : CBATTError.WriteNotPermitted);
                log.LogDebug(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicWriteRequest response ack (isSuccess {S})", res.isSuccess);
            }
            
            // TODO: sent notification
            // var cNotifyUuid = CBUUID.FromString(res.notificationUuid);
            // var c = nativeServices.FirstOrDefault(x => x.UUID == sUuid)?.Characteristics?.FirstOrDefault(x => x.UUID == cUuid);
        };
        
        peripheralManagerDelegate.OnReadRequestReceived += (peripheral, request) =>
        {
            if (OnRead is null)
            {
                log.LogError(LoggerScope.GATT_S.EventId(), "iOSGattServer - OnReadRequestReceived failed due null OnRead Func");
                peripheral.RespondToRequest(request, CBATTError.ReadNotPermitted);
                return;
            }
            
            var cUuid = request.Characteristic.UUID.ToString();
            (bool isSuccess, byte[] data) res = OnRead((cUuid, request.Offset.ToInt32()));
            
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task StopAdvertisingAsync()
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StopAdvertisingAsync iOS");
        peripheralManager.StopAdvertising();
        return Task.FromResult<bool>(true);
    }

    /// <inheritdoc />
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

        if (iosService is null)
        {
            return false;
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
    
    /// <inheritdoc />
    public Task<bool> RemoveServiceAsync(Guid bleServiceUuid)
    {
        // TODO()
        return Task.FromResult(true);
    }
    
    /// <inheritdoc />
    public Task<bool> SendNotification(string cUuid, byte[] value)
    {
        return Task.FromResult(true);
    }
    
    #region Private functions
    
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
        
        if (properties.HasFlag(BleCharacteristicProperties.Notify))
        {
            result |= CBCharacteristicProperties.Notify;
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
    
    #endregion
}