using GattServerLib.GattOptions;

namespace GattServerLib.Interfaces;

public interface IBleCharacteristic
{ 
    string CharateristicName { get; }
    Guid CharacteristicUuid { get; }
    BleCharacteristicProperties Properties { get; }
}