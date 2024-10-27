using Android.Bluetooth.LE;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

public class GattAdvertiseCallback(ILogger logger) : AdvertiseCallback
{
    #region AdvertiseCallback
    
    public delegate void OnStartSuccessDelegate(AdvertiseSettings settingsInEffect);
    public delegate void OnStartFailureDelegate(AdvertiseFailure errorCode);
    
    #endregion
    
    #region AdvertiseCallback events
    
    public event OnStartSuccessDelegate? OnStartSuccessEvent;
    public event OnStartFailureDelegate? OnStartFailureEvent;
    
    #endregion
    
    public override void OnStartSuccess(AdvertiseSettings settingsInEffect)
    {        
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "GattAdvertiseCallback - OnStartSuccess {S}", settingsInEffect.ToString());
        OnStartSuccessEvent?.Invoke(settingsInEffect);
    }

    public override void OnStartFailure(AdvertiseFailure errorCode)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "GattAdvertiseCallback - OnStartFailure {S}", errorCode.ToString()); 
        OnStartFailureEvent?.Invoke(errorCode);
    }
}