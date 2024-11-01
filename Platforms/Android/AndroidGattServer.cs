using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Content;
using Android.OS;
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
    private GattAdvertiseCallback gattAdvertiseCallback;

    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();

    public Func<(string cUuid, int offset), (bool, byte[])>? OnRead { get; set; }
    
    public Func<(string cUuid, byte[] valueWritten), (bool isSuccess, bool notificationNeeded, string notificationUuid)>? OnWrite { get; set; }
    
    public Task InitializeAsync(ILogger log)
    {
        logger = log;
        
        bluetoothManager = (BluetoothManager?)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
        gattAdvertiseCallback = new GattAdvertiseCallback(log);
        gattAdvertiseCallback.OnStartSuccessEvent += OnAdvertisingStartedSuccess;
        gattAdvertiseCallback.OnStartFailureEvent += OnAdvertisingStartedFailure;
        
        gattServerCallback = new GattServerCallback(log);
        gattServerCallback.OnServiceAddedEvent += OnServiceAdded;
        gattServerCallback.OnCharacteristicReadRequestEvent += OnCharacteristicReadRequest;
        gattServerCallback.OnCharacteristicWriteRequestEvent += OnCharacteristicWriteRequest;
        
        log.LogDebug("InitializeAsync Android - completed");
        return Task.CompletedTask;
    }

    private void OnCharacteristicWriteRequest(BluetoothDevice? device, int requestId, BluetoothGattCharacteristic? characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
    {
        if (gattServer is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicWriteRequest failed due null gatt server reference");
            return;
        }
        
        if (device is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicWriteRequest failed due null BluetoothDevice");
            gattServer.SendResponse(null, requestId, GattStatus.Failure, offset, value);
            return;
        }
        
        if (characteristic is null || characteristic.Uuid is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicWriteRequest failed due null BluetoothGattCharacteristic");
            gattServer.SendResponse(device, requestId, GattStatus.Failure, offset, value);
            return;
        }
        
        if (OnWrite is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicWriteRequest failed due null OnWrite Func");
            gattServer?.SendResponse(device, requestId, GattStatus.Failure, offset, null);
            return;
        }
        
        var cUuid = characteristic.Uuid.ToString();
        (bool isSuccess, bool notifcationNeeded, string notificationUuid) res = OnWrite((cUuid, value));
        
        if (responseNeeded)
        {
            gattServer.SendResponse(device, requestId, res.isSuccess ? GattStatus.Success : GattStatus.Failure, offset, value);
            logger.LogDebug(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicWriteRequest response ack (isSuccess {S})", res.isSuccess);
        }

        if (res.notifcationNeeded)
        {
            var services = gattServer?.Services?.FirstOrDefault(s => s.Characteristics.Any(c => c.Uuid.ToString() == cUuid));
            var notifyCharact = services?.Characteristics.FirstOrDefault(c => c.Uuid.ToString() == res.notificationUuid);

            if (notifyCharact is not null && (notifyCharact?.Properties.HasFlag(GattProperty.Notify) ?? false))
            {            
                logger.LogDebug(LoggerScope.GATT_S.EventId(), "AndroidGattServer - send notification");
                notifyCharact.SetValue(value);
                try
                {
                    gattServer.NotifyCharacteristicChanged(device, notifyCharact, false);
                }
                catch (Exception e)
                {
                    var a = 10;
                }
            }
            else
            {
                logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - send notification error");
            }
        }
        
        // TODO: store device request reference, in order to sent notifications ASYNC (despite presence or not of any write request from client side)
        // Sent notify
    }

    private void OnCharacteristicReadRequest(BluetoothDevice? device, int requestId, int offset, BluetoothGattCharacteristic? characteristic)
    {
        if (gattServer is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicReadRequest failed due null gatt server reference");
            return;
        }
        
        if (device is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicReadRequest failed due null BluetoothDevice");
            gattServer?.SendResponse(null, requestId, GattStatus.InvalidAttributeLength, offset, null);
            return;
        }
        
        if (characteristic is null || characteristic.Uuid is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicReadRequest failed due null BluetoothGattCharacteristic");
            gattServer?.SendResponse(device, requestId, GattStatus.InvalidAttributeLength, offset, null);
            return;
        }
        
        if (OnRead is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "AndroidGattServer - OnCharacteristicReadRequest failed due null OnRead Func");
            gattServer?.SendResponse(device, requestId, GattStatus.InvalidAttributeLength, offset, null);
            return;
        }
        
        var cUuid = characteristic.Uuid.ToString();
        (bool isSuccess, byte[] data) res = OnRead((cUuid, offset));
        
        if (res.isSuccess)
        {
            gattServer.SendResponse(device, requestId, GattStatus.Success, offset, res.data);
            logger.LogDebug(LoggerScope.GATT_S.EventId(), "Characteristic read response sent successfully");
        }
        else
        {
            gattServer.SendResponse(device, requestId, GattStatus.InvalidAttributeLength, offset, null);
            logger.LogError(LoggerScope.GATT_S.EventId(), "Characteristic read response failed due to null value");
        }
    }

    private void OnServiceAdded(GattStatus status, BluetoothGattService? service)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "OnServiceAdded Android (service {U}) - status {S}", service.Uuid, status.ToString());
        if (status != GattStatus.Success)
        {
            OnServiceAddedTcs.SetResult(false);
        }
        else
        {
            OnServiceAddedTcs.SetResult(true);
        }
    }

    private void OnAdvertisingStartedSuccess(AdvertiseSettings settingsineffect)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StartAdvertisingAsync Android - settingsineffect {S}", settingsineffect.ToString());
        OnAdvertisingStartedTcs.SetResult(true);
    }

    private void OnAdvertisingStartedFailure(AdvertiseFailure errorcode)
    {            
        logger.LogError(LoggerScope.GATT_S.EventId(), "OnAdvertisingStartedFailure - errorcode {E}", errorcode.ToString());
        OnAdvertisingStartedTcs.SetResult(false);
    }

    public async Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StartAdvertisingAsync Android");

        OnAdvertisingStartedTcs = new();
        gattServer = bluetoothManager?.OpenGattServer(Android.App.Application.Context, gattServerCallback);
        if (gattServer is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "StartAdvertisingAsync Android - gattServer is null");
            return false;
        }
        
        var bluetoothAdapter = bluetoothManager?.Adapter;
        var advertiser = bluetoothAdapter?.BluetoothLeAdvertiser;
        if (advertiser == null || !bluetoothAdapter.IsEnabled)
        {            
            logger.LogError(LoggerScope.GATT_S.EventId(), "StartAdvertisingAsync Android - bluetoothAdapter not enabled");
            return false;
        }
        
        /*var settings = new AdvertiseSettings.Builder()
            .SetAdvertiseMode(AdvertiseMode.LowLatency)
            .SetTxPowerLevel(AdvertiseTx.PowerHigh)
            .SetConnectable(true)
            .Build();*/
        var settings = new AdvertiseSettings.Builder()
            .SetAdvertiseMode(AdvertiseMode.Balanced)!
            .SetConnectable(true)
            .Build();

        var data = new AdvertiseData.Builder()
            .SetIncludeDeviceName(true)
            .AddServiceUuid(ParcelUuid.FromString(options.ServiceUuids.FirstOrDefault() ?? "0000180A-0000-1000-8000-00805f9b34fb")) // Device Information Service UUID
            .Build();
        
        advertiser.StartAdvertising(settings, data, gattAdvertiseCallback);
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StartAdvertisingAsync Android - awaiting advertising completion source");
        var result = await OnAdvertisingStartedTcs.Task;
        
        // Set the Bluetooth adapter's device name
        if (options.LocalName is not null)
        {
            bluetoothAdapter.SetName(options.LocalName);  // This sets the device name globally
        }
        
        return result;
    }

    public Task StopAdvertisingAsync()
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StopAdvertisingAsync Android");
        
        foreach (var bluetoothGattService in gattServer.Services)
        {
            bluetoothGattService.Dispose();
        }

        gattServer.Services.Clear();
        gattServer.ClearServices();
        gattServer.Dispose();
        return Task.CompletedTask;
    }

    public async Task<bool> AddServiceAsync(IBleService bleService)
    {
        OnServiceAddedTcs = new();
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "AddServiceAsync Android - service {S}", bleService.ServiceUuid.ToString());
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
            
            logger.LogDebug(LoggerScope.GATT_S.EventId(),
                "Add characteristic with UUID {U} to service {S}",
                charact.CharacteristicUuid.ToString(),
                bleService.ServiceUuid.ToString());
            var res = androidService.AddCharacteristic(characteristic);
            // TODO: handle res
        }
        
        if (gattServer is null)
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "Error on AddService - Android gattServer null");
            return false;
        }
        
        if (!gattServer.AddService(androidService))
        {
            logger.LogError(LoggerScope.GATT_S.EventId(), "Error on AddService - Android");
            return false;
        }
        
        var result = await OnServiceAddedTcs.Task;
        return result;
    }

    private GattProperty ToGattProperty(BleCharacteristicProperties properties)
    {
        GattProperty result = 0;

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
        
        if (properties.HasFlag(BleCharacteristicProperties.Notify))
        {
            result |= GattProperty.Notify;
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
        GattPermission result = 0;

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
    
    public Task<bool> SendNotification(string cUuid, byte[] value)
    {
        // TODO
        /*var services = gattServer?.Services?.FirstOrDefault(s => s.Characteristics.Any(c => c.Uuid.ToString() == cUuid));
        var characteristic = services?.Characteristics.FirstOrDefault(c => c.Uuid.ToString() == cUuid);

        if (characteristic is null)
        {
            return false;
        }

        var setValueResult = characteristic.SetValue(value);
        if (!setValueResult)
        {
            return false;
        }

        gattServer.NotifyCharacteristicChanged(device, characteristic, true);*/
        return Task.FromResult(true);
    }
}