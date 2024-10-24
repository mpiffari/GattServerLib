namespace GattServerLib.GattOptions;

public class BleAdvOptions
{
    /// <summary>
    /// Set the local name of the advertisement
    /// </summary>
    public string? LocalName { get; set; }

    /// <summary>
    /// GATT services to advertise
    /// </summary>
    public string[] ServiceUuids { get; set; }
}