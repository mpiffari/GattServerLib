using GattServerLib.Interfaces;

namespace GattServerLib.GattOptions;

public class BleService : IBleService
{
    public string ServiceName { get; }
    public Guid ServiceUuid { get; }
    public List<IBleCharacteristic> Characteristics { get; }

    public BleService(string serviceName, Guid serviceUuid, List<IBleCharacteristic> characteristics)
    {
        ServiceName = serviceName;
        ServiceUuid = serviceUuid;
        Characteristics = characteristics;
    }
}