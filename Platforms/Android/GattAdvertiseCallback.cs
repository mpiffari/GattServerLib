using Android.Bluetooth.LE;
using GattServerLib.Support;
using Microsoft.Extensions.Logging;

namespace GattServerLib;

public class GattAdvertiseCallback(ILogger logger) : AdvertiseCallback
{
    #region AdvertiseCallback
    
    public delegate void StartSuccessDelegate(AdvertiseSettings? settingsInEffect);
    public delegate void StartFailureDelegate(AdvertiseFailure errorCode);
    
    #endregion
    
    #region AdvertiseCallback events
    
    public event StartSuccessDelegate? OnStartSuccessEvent;
    public event StartFailureDelegate? OnStartFailureEvent;
    
    #endregion
    
    /// <inheritdoc />
    public override void OnStartSuccess(AdvertiseSettings? settingsInEffect)
    {        
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "GattAdvertiseCallback - OnStartSuccess {S}", settingsInEffect?.ToString());
        OnStartSuccessEvent?.Invoke(settingsInEffect);
    }

    /// <inheritdoc />
    public override void OnStartFailure(AdvertiseFailure errorCode)
    {
        logger.LogDebug(LoggerScope.GATT_S.EventId(), "GattAdvertiseCallback - OnStartFailure {S}", errorCode.ToString()); 
        OnStartFailureEvent?.Invoke(errorCode);
    }
}