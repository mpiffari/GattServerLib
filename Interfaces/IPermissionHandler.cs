namespace GattServerLib.Interfaces;

public interface IPermissionHandler
{
    Task<bool> IsBluetoothEnabledAsync();
    Task RequestBluetoothActivationAsync();
    Task<bool> CheckAndRequestPermissionsAsync();
}