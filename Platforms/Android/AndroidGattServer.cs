using Android.Bluetooth;
using Android.Content;
using GattServerLib.GattOptions;
using GattServerLib.Interfaces;
using GattServerLib.Support;
using Java.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.ApplicationModel;
using static Android.Manifest;

namespace GattServerLib;

public interface IOnActivityRequestPermissionsResult
{
    void Handle(Activity activity, int requestCode, string[] permissions, Permission[] grantResults);
}

public class AndroidGattServer : IGattServer, IAndroidLifecycle.IOnActivityRequestPermissionsResult,
    IAndroidLifecycle.IOnActivityResult
{
    private BluetoothManager? bluetoothManager;
    private BluetoothGattServer? gattServer;
    private ILogger logger;
    
    private GattServerCallback gattServerCallback;
    
    private TaskCompletionSource<bool> OnAdvertisingStartedTcs = new();
    private TaskCompletionSource<bool> OnServiceAddedTcs = new();
    private TaskCompletionSource<bool> OnStateUpdatedTcs = new();
    private TaskCompletionSource<bool> OnWriteRequestsReceivedTcs = new();
    private TaskCompletionSource<bool> OnReadRequestReceivedTcs = new();
    
    public BleAccessState AdvertisingAccessStatus
    {
        get
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(23))
                return BleAccessState.NotSupported;

            var status = BleAccessState.Available;
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
                status = this.context.Platform.GetCurrentPermissionStatus(Permission.BluetoothAdvertise);

            if (status == BleAccessState.Available)
                status = this.context.Manager.GetAccessState();

            return status;
        }
    }
    
    public BleAccessState GattAccessStatus
    {
        get
        {
            if (!OperatingSystem.IsAndroidVersionAtLeast(23))
                return BleAccessState.NotSupported;

            var status = BleAccessState.Available;
            if (OperatingSystem.IsAndroidVersionAtLeast(31))
                status = GetCurrentPermissionStatus(Permission.BluetoothConnect);

            if (status == BleAccessState.Available)
                status = bluetoothManager.GetAccessState();

            return status;
        }
    }

    private BleAccessState GetCurrentPermissionStatus(string androidPermission)
    {
        var self = ContextCompat.CheckSelfPermission(this.AppContext, androidPermission);
        if (self == Permission.Granted)
            return BleAccessState.Available;

        if (!this.HasRequestedPermission(androidPermission))
            return BleAccessState.Unknown;

        //var showRequest = ActivityCompat.ShouldShowRequestPermissionRationale(this.CurrentActivity!, androidPermission);
        //if (showRequest)
        //    return AccessState.Unknown;

        return BleAccessState.Denied;
    }
    
    public async Task<BleAccessState> RequestAccess(bool advertise = true, bool connect = true)
    {
        if (!advertise && !connect)
            throw new ArgumentException("You must request at least 1 permission");

        if (!OperatingSystem.IsAndroidVersionAtLeast(23))
            return BleAccessState.NotSupported; //throw new InvalidOperationException("BLE Advertiser needs API Level 23+");

        var current = this.context.Manager.GetAccessState();
        if (current != BleAccessState.Available && current != BleAccessState.Unknown)
            return current;

        if (OperatingSystem.IsAndroidVersionAtLeast(31))
        {
            var perms = new List<string>();
            if (advertise)
                perms.Add(Permission.BluetoothAdvertise);

            if (connect)
                perms.Add(Permission.BluetoothConnect);
            
            
            var result = await Platform.RequestPermissions(perms.ToArray());
            if (!result.IsSuccess())
                return BleAccessState.Denied;
        }
        return BleAccessState.Available;
    }
    
    public Task InitializeAsync(ILogger logger)
    {
        bluetoothManager = (BluetoothManager?)Android.App.Application.Context.GetSystemService(Context.BluetoothService);
        gattServerCallback = new GattServerCallback(logger);
        this.logger = logger;
        
        logger.LogDebug("InitializeAsync Android");
        return Task.CompletedTask;
    }

    public Task<bool> StartAdvertisingAsync(BleAdvOptions? options = null)
    {
        gattServer = bluetoothManager?.OpenGattServer(Android.App.Application.Context, gattServerCallback);
        logger.LogDebug("StartAdvertisingAsync Android");
        return Task.FromResult(gattServer is not null);
    }

    public Task StopAdvertisingAsync()
    {
        foreach (var bluetoothGattService in gattServer.Services)
        {
           bluetoothGattService.Dispose();
        }
        gattServer.Services.Clear();
        gattServer.ClearServices();
        gattServer.Dispose();
        return Task.CompletedTask;
    }
    
    public Task<bool> AddServiceAsync(UUID uuid)
    {       
        BluetoothGattService androidService = new BluetoothGattService(uuid, GattServiceType.Primary);
        // Add characteristics to the service.
        if (gattServer is null || !gattServer.AddService(androidService))
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }

    public Task<bool> RemoveServiceAsync(UUID uuid)
    {
        var serviceToRemove = gattServer.Services?.FirstOrDefault(x => x.Uuid == uuid);
        if (serviceToRemove == null)
        {
            return Task.FromResult(false);
        }

        if (!gattServer.RemoveService(serviceToRemove))
        {
            return Task.FromResult(false);
        }
        
        return Task.FromResult(true);
    }
}