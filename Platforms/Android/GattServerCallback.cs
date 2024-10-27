using Android.Bluetooth;
using Android.Bluetooth.LE;
using Android.Runtime;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

internal class GattServerCallback(ILogger logger) : BluetoothGattServerCallback
{
    #region PeripheralManagerDelegate
    
    public delegate void OnCharacteristicReadRequestDelegate(int requestId, int offset, BluetoothGattCharacteristic characteristic);
    public delegate void OnCharacteristicWriteRequestDelegate(int requestId, BluetoothGattCharacteristic characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value);
    public delegate void OnCharacteristicWriteExecuteDelegate(int requestId, bool execute);
    public delegate void OnServiceAddedDelegate(GattStatus status, BluetoothGattService? service);
    
    #endregion
    
    #region PeripheralManagerDelegate events

    public event OnCharacteristicReadRequestDelegate? OnCharacteristicReadRequestEvent;
    public event OnCharacteristicWriteRequestDelegate? OnCharacteristicWriteRequestEvent;
    public event OnCharacteristicWriteExecuteDelegate? OnCharacteristicWriteExecuteEvent;
    public event OnServiceAddedDelegate? OnServiceAddedEvent;
    
    #endregion

    public override void OnCharacteristicReadRequest(BluetoothDevice? device, int requestId, int offset, BluetoothGattCharacteristic? characteristic)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnCharacteristicReadRequest");
        OnCharacteristicReadRequestEvent?.Invoke(requestId, offset, characteristic);
    }

    public override void OnCharacteristicWriteRequest(BluetoothDevice? device, int requestId, BluetoothGattCharacteristic? characteristic, bool preparedWrite, bool responseNeeded, int offset, byte[] value)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnCharacteristicWriteRequest");
        OnCharacteristicWriteRequestEvent?.Invoke(requestId, characteristic, preparedWrite, responseNeeded, offset, value);
    }
    
    public override void OnExecuteWrite(BluetoothDevice? device, int requestId, bool execute)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnExecuteWrite");
        OnCharacteristicWriteExecuteEvent?.Invoke(requestId, execute);
    }
    
    public override void OnServiceAdded([GeneratedEnum] GattStatus status, BluetoothGattService? service)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "BluetoothGattServerCallback - OnServiceAdded");
        OnServiceAddedEvent?.Invoke(status, service);
    }
    
    public override void OnConnectionStateChange(BluetoothDevice? device, [GeneratedEnum] ProfileState status, [GeneratedEnum] ProfileState newState)
    {
        // TODO: handle delegate
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

    public override void OnNotificationSent(BluetoothDevice? device, [GeneratedEnum] GattStatus status)
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