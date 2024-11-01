using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

internal class GattServerCallback(ILogger logger) : BluetoothGattServerCallback
{
    #region PeripheralManagerDelegate
    
    public delegate void CharacteristicReadRequestDelegate(BluetoothDevice? device, int requestId, int offset, BluetoothGattCharacteristic? characteristic);
    public delegate void CharacteristicWriteRequestDelegate(BluetoothDevice? device, int requestId, BluetoothGattCharacteristic? characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value);
    public delegate void CharacteristicWriteExecuteDelegate(int requestId, bool execute);
    public delegate void ServiceAddedDelegate(GattStatus status, BluetoothGattService? service);
    
    #endregion
    
    #region PeripheralManagerDelegate events

    public event CharacteristicReadRequestDelegate? OnCharacteristicReadRequestEvent;
    public event CharacteristicWriteRequestDelegate? OnCharacteristicWriteRequestEvent;
    public event CharacteristicWriteExecuteDelegate? OnCharacteristicWriteExecuteEvent;
    public event ServiceAddedDelegate? OnServiceAddedEvent;
    
    #endregion

    public override void OnCharacteristicReadRequest(BluetoothDevice? device, int requestId, int offset, BluetoothGattCharacteristic? characteristic)
    {
        var deviceInfo = $"device {device?.Name ?? "NA"} (addr {device?.Address ?? "NA"})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnCharacteristicReadRequest - {I} requestId {R} offset {O} characteristic {C}", deviceInfo, requestId, offset, characteristic?.Uuid?.ToString());
        OnCharacteristicReadRequestEvent?.Invoke(device, requestId, offset, characteristic);
    }

    public override void OnCharacteristicWriteRequest(BluetoothDevice? device, int requestId, BluetoothGattCharacteristic? characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
    {
        var valueWritten = BitConverter.ToString(value);
        var deviceInfo = $"device {device?.Name ?? "NA"} (addr {device?.Address ?? "NA"})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnCharacteristicWriteRequest value written {W} - {I} requestId {RI} - characteristic {C} - preparedWrite {P} - responseNeeded {RN}", valueWritten, deviceInfo, requestId, characteristic?.Uuid?.ToString(), preparedWrite, responseNeeded);
        OnCharacteristicWriteRequestEvent?.Invoke(device, requestId, characteristic, preparedWrite, responseNeeded, offset, value);
    }
    
    public override void OnExecuteWrite(BluetoothDevice? device, int requestId, bool execute)
    {
        var deviceInfo = $"device {device?.Name ?? "NA"} (addr {device?.Address ?? "NA"})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - {I} OnExecuteWrite - requestId {R} - execute {E}", deviceInfo, requestId, execute);
        OnCharacteristicWriteExecuteEvent?.Invoke(requestId, execute);
    }
    
    public override void OnNotificationSent(BluetoothDevice? device, [GeneratedEnum] GattStatus status)
    {
        var deviceInfo = $"device {device?.Name ?? "NA"} (addr {device?.Address ?? "NA"})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnNotificationSent {I} status {S}", deviceInfo, status.ToString());
    }
    
    public override void OnServiceAdded([GeneratedEnum] GattStatus status, BluetoothGattService? service)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnServiceAdded status {G} service {S}", status.ToString(), service.Uuid.ToString());
        OnServiceAddedEvent?.Invoke(status, service);
    }
    
    public override void OnConnectionStateChange(BluetoothDevice? device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
    {
        var deviceInfo = $"device {device?.Name ?? "NA"} (addr {device?.Address ?? "NA"})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnConnectionStateChange {I} status {S} newState {N}", deviceInfo, status.ToString(), newState.ToString());
    }
    
    public override void OnDescriptorReadRequest(BluetoothDevice? device, int requestId, int offset, BluetoothGattDescriptor? descriptor)
    {
        // TODO: handle delegate
    }


    public override void OnDescriptorWriteRequest(BluetoothDevice? device, int requestId, BluetoothGattDescriptor? descriptor, bool preparedWrite, bool responseNeeded, int offset, byte[]? value)
    {
        // TODO: handle delegate
    }

    public override void OnMtuChanged(BluetoothDevice? device, int mtu)
    {
        // TODO: handle delegate
    }

    public override void OnPhyRead(BluetoothDevice? device, [GeneratedEnum] ScanSettingsPhy txPhy, [GeneratedEnum] ScanSettingsPhy rxPhy, [GeneratedEnum] GattStatus status)
    {
        // TODO: handle delegate
    }

    public override void OnPhyUpdate(BluetoothDevice? device, [GeneratedEnum] ScanSettingsPhy txPhy, [GeneratedEnum] ScanSettingsPhy rxPhy, [GeneratedEnum] GattStatus status)
    {
        // TODO: handle delegate
    }
}