namespace GattServerLib.Interfaces;

public interface IBleService
{
    string ServiceName { get; }
    Guid ServiceUuid { get; }
    IReadOnlyList<IBleCharacteristic> Characteristics { get; }
    Task AddCharacteristicAsync(IBleCharacteristic service);
    Task RemoveCharacteristicAsync(IBleCharacteristic service);
}