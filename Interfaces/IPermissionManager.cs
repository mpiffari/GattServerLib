namespace GattServerLib.Interfaces;

public interface IPermissionManager
{
    Task<bool> CheckAndRequestPermissionsAsync();
    Task<bool> IsBleSupported();
    Task<bool> IsBluetoothEnabled();
}