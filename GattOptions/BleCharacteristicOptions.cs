namespace GattServerLib.GattOptions;

public class BleCharacteristicOptions
{
    public Guid CharacteristicUuid { get; set; }
    public BleCharacteristicProperties Properties { get; set; }
    public byte[] InitialValue { get; set; }
}