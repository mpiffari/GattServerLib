using CoreBluetooth;
using GattServerLib.Interfaces;

namespace GattServerLib;

public class iOSPermissionHandler : IPermissionHandler
{
    private CBCentralManager centralManager;
    
    public iOSPermissionHandler()
    {
        centralManager = new CBCentralManager();
    }
    
    public Task<bool> IsBluetoothEnabledAsync()
    {
        return Task.FromResult(centralManager.State == CBManagerState.PoweredOn);
    }

    public Task RequestBluetoothActivationAsync()
    {
        // iOS does not support programmatically enabling Bluetooth
        return Task.CompletedTask;
    }
    
    public async Task<bool> CheckAndRequestPermissionsAsync()
    {
        var bluetoothStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
        
        if (bluetoothStatus != PermissionStatus.Granted)
        {
            bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
        }

        return bluetoothStatus == PermissionStatus.Granted;
    }
}