using CoreBluetooth;
using Foundation;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

internal class IOsPeripheralManagerDelegate(ILogger logger) : CBPeripheralManagerDelegate
{
    #region PeripheralManagerDelegate
    
    public delegate void AdvertisingStartedDelegate(NSError? error);
    public delegate void ServiceAddedDelegate(CBService service, NSError? error);
    public delegate void WriteRequestsReceivedDelegate(CBPeripheralManager peripheral, CBATTRequest request);
    public delegate void ReadRequestReceivedDelegate(CBPeripheralManager peripheral, CBATTRequest request);
    public delegate void SubscriptionReceivedDelegate(CBCentral central, CBCharacteristic characteristic);
    public delegate void UnsubscriptionReceivedDelegate(CBCentral central, CBCharacteristic characteristic);
    
    #endregion
    
    #region PeripheralManagerDelegate events

    public event AdvertisingStartedDelegate? OnAdvertisingStarted;
    public event ServiceAddedDelegate? OnServiceAdded;
    public event EventHandler<string>? OnStateUpdated;
    public event WriteRequestsReceivedDelegate? OnWriteRequestsReceived;
    public event ReadRequestReceivedDelegate? OnReadRequestReceived;
    public event SubscriptionReceivedDelegate? OnSubscriptionReceived;
    public event UnsubscriptionReceivedDelegate? OnUnsubscriptionReceived;
    
    #endregion
    
    public override void AdvertisingStarted(CBPeripheralManager peripheral, NSError? error)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - AdvertisingStarted iOS - error {E}", error?.ToString());
        OnAdvertisingStarted?.Invoke(error);
    }
    public override void ServiceAdded(CBPeripheralManager peripheral, CBService service, NSError? error)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - ServiceAdded iOS - service {S} - error {E}", service.UUID.ToString(), error?.ToString());
        OnServiceAdded?.Invoke(service, error);
    }
    
    public override void StateUpdated(CBPeripheralManager peripheral)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - StateUpdated iOS - state {P}", peripheral.State);
        OnStateUpdated?.Invoke(this, "StateUpdated");
    }
    
    public override void WriteRequestsReceived(CBPeripheralManager peripheral, CBATTRequest[] requests)
    {
        foreach (var request in requests)
        {        
            var deviceInfo = $"{request.Central.Description} (addr {request.Central.Identifier})";
            var characteristic = request.Characteristic;
            characteristic.Value = request.Value;
            peripheral.RespondToRequest(request, CBATTError.Success);
                
            logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - WriteRequestsReceived iOS - {I} charact {C} - request (#{R}) {V}",
                deviceInfo,
                characteristic.UUID.ToString(),
                request.Value.Length,
                request.Value.ToList().Select(x => x.ToString("X2")));
            
            OnWriteRequestsReceived?.Invoke(peripheral, request);
        }
    }
    
    public override void ReadRequestReceived(CBPeripheralManager peripheral, CBATTRequest request)
    {
        var characteristic = request.Characteristic;
        var deviceInfo = $"{request.Central.Description} (addr {request.Central.Identifier})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - ReadRequestReceived iOS - {I} characteristic {R}", deviceInfo, characteristic.UUID.ToString());
        OnReadRequestReceived?.Invoke(peripheral, request);
    }
    
    public override void CharacteristicSubscribed(CBPeripheralManager peripheral, CBCentral central, CBCharacteristic characteristic)
    {        
        var deviceInfo = $"{central.Description} (addr {central.Identifier})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - CharacteristicSubscribed iOS - central {C} charact {CC}", central.DebugDescription, characteristic.UUID.ToString());
        OnSubscriptionReceived?.Invoke(central, characteristic);
    }
       
    public override void CharacteristicUnsubscribed(CBPeripheralManager peripheral, CBCentral central, CBCharacteristic characteristic)
    {
        var deviceInfo = $"{central.Description} (addr {central.Identifier})";
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CBPeripheralManagerDelegate - CharacteristicUnsubscribed iOS - central {C} charact {CC}", central.DebugDescription, characteristic.UUID.ToString());
        OnUnsubscriptionReceived?.Invoke(central, characteristic);
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