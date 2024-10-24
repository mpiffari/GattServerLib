namespace GattServerLib.GattOptions;

public class BleServiceOptions
{
    public Guid ServiceUuid { get; set; }
    public bool IsPrimary { get; set; } = true;
}