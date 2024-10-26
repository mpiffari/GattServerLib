using CoreBluetooth;
using Foundation;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

internal class iOSPeripheralManagerDelegate(ILogger logger) : CBPeripheralManagerDelegate
{
    #region PeripheralManagerDelegate
    
    public delegate void OnAdvertisingStartedDelegate(NSError? error);
    public delegate void OnServiceAddedDelegate(CBService service, NSError? error);
    public delegate void OnWriteRequestsReceivedDelegate(CBATTRequest[] requests);
    public delegate void OnReadRequestReceivedDelegate(CBATTRequest request);
    
    #endregion
    
    #region PeripheralManagerDelegate events

    public event OnAdvertisingStartedDelegate? OnAdvertisingStarted;
    public event OnServiceAddedDelegate? OnServiceAdded;
    public event EventHandler<string>? OnStateUpdated;
    public event OnWriteRequestsReceivedDelegate? OnWriteRequestsReceived;
    public event OnReadRequestReceivedDelegate? OnReadRequestReceived;
    
    #endregion
    
    public override void AdvertisingStarted(CBPeripheralManager peripheral, NSError? error)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "AdvertisingStarted");
        OnAdvertisingStarted?.Invoke(error);
    }
    public override void ServiceAdded(CBPeripheralManager peripheral, CBService service, NSError? error)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "ServiceAdded");
        OnServiceAdded?.Invoke(service, error);
    }
    
    public override void StateUpdated(CBPeripheralManager peripheral)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StateUpdated");
        OnStateUpdated?.Invoke(this, "StateUpdated");
    }
    
    public override void WriteRequestsReceived(CBPeripheralManager peripheral, CBATTRequest[] requests)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "WriteRequestsReceived");
        OnWriteRequestsReceived?.Invoke(requests);
    }
    
    public override void ReadRequestReceived(CBPeripheralManager peripheral, CBATTRequest request)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "ReadRequestReceived");
        OnReadRequestReceived?.Invoke(request);
    }
    
    public override void CharacteristicSubscribed(CBPeripheralManager peripheral, CBCentral central, CBCharacteristic characteristic)
    {
        // TODO: handle delegate
    }
       
    public override void CharacteristicUnsubscribed(CBPeripheralManager peripheral, CBCentral central, CBCharacteristic characteristic)
    {
        // TODO: handle delegate
    }
       
    public override void DidOpenL2CapChannel(CBPeripheralManager peripheral, CBL2CapChannel? channel, NSError? error)
    {
        // TODO: handle delegate
    }
       
    public override void DidPublishL2CapChannel(CBPeripheralManager peripheral, ushort psm, NSError? error)
    {
        // TODO: handle delegate
    }
       
    public override void DidUnpublishL2CapChannel(CBPeripheralManager peripheral, ushort psm, NSError? error)
    {
        // TODO: handle delegate
    }
       
    public override void ReadyToUpdateSubscribers(CBPeripheralManager peripheral)
    {
        // TODO: handle delegate
    }
    
    public override void WillRestoreState(CBPeripheralManager peripheral, NSDictionary dict)
    {
        // TODO: handle delegate
    }
}