using Android.Bluetooth;
using Android.Content;
using GattServerLib.Interfaces;

namespace GattServerLib;

public class AndroidPermissionHandler : IPermissionHandler
{
    public Task<bool> IsBluetoothEnabledAsync()
    {
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        return Task.FromResult(adapter?.IsEnabled ?? false);
    }

    public Task RequestBluetoothActivationAsync()
    {
        BluetoothAdapter adapter = BluetoothAdapter.DefaultAdapter;
        if (adapter == null || adapter.IsEnabled)
        {
            return Task.CompletedTask;
        }

        Intent enableBtIntent = new Intent(BluetoothAdapter.ActionRequestEnable);
        enableBtIntent.SetFlags(ActivityFlags.NewTask);
        Android.App.Application.Context.StartActivity(enableBtIntent);
        return Task.CompletedTask;
    }
    
    public async Task<bool> CheckAndRequestPermissionsAsync()
    {
        var bluetoothStatus = await Permissions.CheckStatusAsync<Permissions.Bluetooth>();
        var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        if (bluetoothStatus != PermissionStatus.Granted || locationStatus != PermissionStatus.Granted)
        {
            bluetoothStatus = await Permissions.RequestAsync<Permissions.Bluetooth>();
            locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        return bluetoothStatus == PermissionStatus.Granted && locationStatus == PermissionStatus.Granted;
    }
}