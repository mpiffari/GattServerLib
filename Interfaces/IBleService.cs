namespace GattServerLib.Interfaces;

public interface IBleService
{
    /// <summary>
    /// 
    /// </summary>
    string ServiceName { get; }
    
    /// <summary>
    /// 
    /// </summary>
    Guid ServiceUuid { get; }
    
    /// <summary>
    /// 
    /// </summary>
    List<IBleCharacteristic> Characteristics { get; }
}