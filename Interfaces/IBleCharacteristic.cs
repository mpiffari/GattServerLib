namespace GattServerLib.Interfaces;

public interface IBleCharacteristic
{
    Guid CharacteristicUuid { get; }
    BleCharacteristicProperties Properties { get; }
    event EventHandler<BleCharacteristicReadRequestEventArgs> ReadRequested;
    event EventHandler<BleCharacteristicWriteRequestEventArgs> WriteRequested;
    Task RespondToReadRequestAsync(byte[] value);
    Task RespondToWriteRequestAsync(bool success);
}