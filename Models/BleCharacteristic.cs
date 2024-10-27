using GattServerLib.Interfaces;

namespace GattServerLib.GattOptions;

public class BleCharacteristic : IBleCharacteristic
{
    public string CharateristicName { get; }
    public Guid CharacteristicUuid { get; }
    public BleCharacteristicProperties Properties { get; }
    
    public BleCharacteristic(string charateristicName, BleCharacteristicProperties properties)
    {
        CharateristicName = charateristicName;
        Properties = properties;
    }
    
    public Task RespondToReadRequestAsync(byte[] value)
    {
        throw new NotImplementedException();
    }

    public Task RespondToWriteRequestAsync(bool success)
    {
        throw new NotImplementedException();
    }
}