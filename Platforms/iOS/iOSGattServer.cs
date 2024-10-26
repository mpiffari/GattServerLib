using CoreBluetooth;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;

namespace GattServerLib;

public class iOSGattServer : IGattServer
{
    private CBPeripheralManager peripheralManager;
    private iOSPeripheralManagerDelegate peripheralManagerDelegate;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    private TaskCompletionSource<bool> OnStateUpdatedTcs = new();
    private TaskCompletionSource<bool> OnWriteRequestsReceivedTcs = new();
    private TaskCompletionSource<bool> OnReadRequestReceivedTcs = new();
    
    public Task InitializeAsync()
    {
        peripheralManager = new CBPeripheralManager();
        
        peripheralManagerDelegate = new iOSPeripheralManagerDelegate();
        peripheralManagerDelegate.OnAdvertisingStarted += error => { }; 
        peripheralManagerDelegate.OnServiceAdded += (service, error) => 
        peripheralManagerDelegate.OnStateUpdated += (sender, s) => { };
        peripheralManagerDelegate.OnWriteRequestsReceived += requests => { }; 
        peripheralManagerDelegate.OnReadRequestReceived += request => { }; 
        
        return Task.CompletedTask;
    }

    public Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        if (peripheralManager.Advertising)
        {
            return Task.FromResult(false);e
        }
        
        options ??= new BleAdvOptions();
        var opts = new StartAdvertisingOptions();
        if (options.LocalName != null)
            opts.LocalName = options.LocalName;

        if (options.ServiceUuids.Length > 0)
        {
            opts.ServicesUUID = options
                .ServiceUuids
                .Select(CBUUID.FromString)
                .ToArray();
        }
        
        peripheralManager.StartAdvertising(opts);
    }

    public Task StopAdvertisingAsync()
    {
        peripheralManager.StopAdvertising();
    }

    public Task AddServiceAsync(IBleService service)
    {
        var iosService = new CBMutableService(CBUUID.FromString(service.ServiceUuid.ToString()), true);
        // Add characteristics to the service.
        peripheralManager.AddService(iosService);
    }

    public Task RemoveServiceAsync(IBleService service)
    {
        throw new NotImplementedException();
    }

    public event EventHandler<BleDeviceConnectionEventArgs>? DeviceConnected;
    public event EventHandler<BleDeviceConnectionEventArgs>? DeviceDisconnected;
    public event EventHandler<BleCharacteristicWriteRequest>? OnWriteRequest;
    public event EventHandler<BleCharacteristicReadRequest>? OnReadRequest;
}