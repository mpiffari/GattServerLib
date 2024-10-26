using GattServerLib.GattOptions;

namespace GattServerLib.Interfaces;

public interface IBleCharacteristic
{
    Guid CharacteristicUuid { get; }
    BleCharacteristicProperties Properties { get; }
    Task RespondToReadRequestAsync(byte[] value);
    Task RespondToWriteRequestAsync(bool success);
}