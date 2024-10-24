using Android.Bluetooth;
using Android.Content;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using Java.Util;

namespace GattServerLib;

public class AndroidBleGattServer : IGattServer
{
    private BluetoothManager _bluetoothManager;
    private BluetoothGattServer _gattServer;
    
    private GattServerCallback gattServerCallback;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    private TaskCompletionSource<bool> OnStateUpdatedTcs = new();
    private TaskCompletionSource<bool> OnWriteRequestsReceivedTcs = new();
    private TaskCompletionSource<bool> OnReadRequestReceivedTcs = new();
    
    public Task InitializeAsync()
    {
        _bluetoothManager = (BluetoothManager)Application.Context.GetSystemService(Context.BluetoothService);

        gattServerCallback = new GattServerCallback();
        
    }

    public Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        _gattServer = _bluetoothManager.OpenGattServer(Application.Context, new GattServerCallback());
    }

    public Task StopAdvertisingAsync()
    {
        _gattServer?.Close();
    }

    public Task AddServiceAsync(IBleService service)
    {       
        BluetoothGattService androidService = new BluetoothGattService(UUID.FromString(service.ServiceUuid.ToString()), GattServiceType.Primary);
        // Add characteristics to the service.
        _gattServer.AddService(androidService);
    }

    public Task RemoveServiceAsync(IBleService service)
    {
    }

    public event EventHandler<BleDeviceConnectionEventArgs>? DeviceConnected;
    public event EventHandler<BleDeviceConnectionEventArgs>? DeviceDisconnected;
    public event EventHandler<BleCharacteristicWriteRequest>? OnWriteRequest;
    public event EventHandler<BleCharacteristicReadRequest>? OnReadRequest;
}