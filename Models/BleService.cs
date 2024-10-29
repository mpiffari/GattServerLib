using GattServerLib.Interfaces;

namespace GattServerLib.GattOptions;

public class BleService : IBleService
{
    public string ServiceName { get; }
    public Guid ServiceUuid { get; }
    public List<IBleCharacteristic> Characteristics { get; }

    public BleService(string serviceName, Guid serviceUuid)
    {
        ServiceName = serviceName;
        ServiceUuid = serviceUuid;
        Characteristics = new List<IBleCharacteristic>();
    }
    
    public Task AddCharacteristicAsync(IBleCharacteristic characteristic)
    {
        if (!Characteristics.Select(x => x.CharacteristicUuid).Contains(characteristic.CharacteristicUuid))
        {
            Characteristics.Append(characteristic);
        }
        return Task.CompletedTask;
    }

    public Task RemoveCharacteristicAsync(IBleCharacteristic characteristic)
    {
        if (Characteristics.Select(x => x.CharacteristicUuid).Contains(characteristic.CharacteristicUuid))
        {
            Characteristics.ToList().Remove(characteristic);
        }

        return Task.CompletedTask;
    }
}