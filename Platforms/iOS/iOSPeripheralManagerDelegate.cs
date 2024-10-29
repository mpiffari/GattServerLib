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
    public delegate void OnWriteRequestsReceivedDelegate(CBATTRequest request);
    public delegate void OnReadRequestReceivedDelegate(CBPeripheralManager peripheral, CBATTRequest request);
    public delegate void OnSubscriptionReceivedDelegate(CBCentral central, CBCharacteristic characteristic);
    public delegate void OnUnsubscriptionReceivedDelegate(CBCentral central, CBCharacteristic characteristic);
    
    #endregion
    
    #region PeripheralManagerDelegate events

    public event OnAdvertisingStartedDelegate? OnAdvertisingStarted;
    public event OnServiceAddedDelegate? OnServiceAdded;
    public event EventHandler<string>? OnStateUpdated;
    public event OnWriteRequestsReceivedDelegate? OnWriteRequestsReceived;
    public event OnReadRequestReceivedDelegate? OnReadRequestReceived;
    public event OnSubscriptionReceivedDelegate? OnSubscriptionReceived;
    public event OnUnsubscriptionReceivedDelegate? OnUnsubscriptionReceived;
    
    #endregion
    
    public override void AdvertisingStarted(CBPeripheralManager peripheral, NSError? error)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "AdvertisingStarted iOS - error {E}", error?.ToString());
        OnAdvertisingStarted?.Invoke(error);
    }
    public override void ServiceAdded(CBPeripheralManager peripheral, CBService service, NSError? error)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "ServiceAdded iOS - service {S} - error {E}", service.UUID.ToString(), error?.ToString());
        OnServiceAdded?.Invoke(service, error);
    }
    
    public override void StateUpdated(CBPeripheralManager peripheral)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "StateUpdated iOS - state {P}", peripheral.State);
        OnStateUpdated?.Invoke(this, "StateUpdated");
    }
    
    public override void WriteRequestsReceived(CBPeripheralManager peripheral, CBATTRequest[] requests)
    {
        foreach (var request in requests)
        {
            var characteristic = request.Characteristic;
            characteristic.Value = request.Value;
            peripheral.RespondToRequest(request, CBATTError.Success);
                
            logger.LogDebug(LoggerScope.GATT_S.EventId(), "WriteRequestsReceived iOS - charact {C} - request (#{R}) {V}",
                characteristic.UUID.ToString(),
                request.Value.Length,
                request.Value.ToList().Select(x => x.ToString("X2")));
            
            // Verify that the request matches a writable characteristic.
            if (request.Characteristic.Properties.HasFlag(CBCharacteristicProperties.Write))
            {
                peripheral.RespondToRequest(request, CBATTError.Success);
            }
            else
            {
                // If the characteristic is not writable, respond with an error
                peripheral.RespondToRequest(request, CBATTError.WriteNotPermitted);
            }
            
            OnWriteRequestsReceived?.Invoke(request);
        }
    }
    
    public override void ReadRequestReceived(CBPeripheralManager peripheral, CBATTRequest request)
    {
        var characteristic = request.Characteristic;
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "ReadRequestReceived iOS - characteristic {R}", characteristic.UUID.ToString());
        OnReadRequestReceived?.Invoke(peripheral, request);
    }
    
    public override void CharacteristicSubscribed(CBPeripheralManager peripheral, CBCentral central, CBCharacteristic characteristic)
    {        
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CharacteristicSubscribed iOS - ");
        OnSubscriptionReceived?.Invoke(central, characteristic);
    }
       
    public override void CharacteristicUnsubscribed(CBPeripheralManager peripheral, CBCentral central, CBCharacteristic characteristic)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "CharacteristicUnsubscribed iOS - ");
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