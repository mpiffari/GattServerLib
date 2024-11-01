namespace GattServerLib.Interfaces;

public interface IPermissionHandler
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> IsBluetoothEnabledAsync();
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task RequestBluetoothActivationAsync();
    
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> CheckAndRequestPermissionsAsync();
}