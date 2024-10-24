using GattServerLib.GattOptions;

namespace GattServerLib.Interfaces;

public interface IGattServer
{
    Task InitializeAsync();
    Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null);
    Task StopAdvertisingAsync();
    Task AddServiceAsync(IBleService service);
    Task RemoveServiceAsync(IBleService service);
    event EventHandler<BleDeviceConnectionEventArgs> DeviceConnected;
    event EventHandler<BleDeviceConnectionEventArgs> DeviceDisconnected;
    event EventHandler<BleCharacteristicWriteRequest> OnWriteRequest;
    event EventHandler<BleCharacteristicReadRequest> OnReadRequest;
}