namespace GattServerLib.Interfaces;

public interface IBleService
{
    string ServiceName { get; }
    Guid ServiceUuid { get; }
    List<IBleCharacteristic> Characteristics { get; }
    Task AddCharacteristicAsync(IBleCharacteristic characteristic);
    Task RemoveCharacteristicAsync(IBleCharacteristic characteristic);
}