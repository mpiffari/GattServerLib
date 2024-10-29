using GattServerLib.Interfaces;

namespace GattServerLib.GattOptions;

public class BleCharacteristic : IBleCharacteristic
{
    public string CharateristicName { get; }
    public Guid CharacteristicUuid { get; }
    public BleCharacteristicProperties Properties { get; }
    
    public BleCharacteristic(string charateristicName, Guid uuid, BleCharacteristicProperties properties)
    {
        CharacteristicUuid = uuid;
        CharateristicName = charateristicName;
        Properties = properties;
    }
}