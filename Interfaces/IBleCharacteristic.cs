using GattServerLib.GattOptions;

namespace GattServerLib.Interfaces;

public interface IBleCharacteristic
{ 
    /// <summary>
    /// 
    /// </summary>
    string CharateristicName { get; }
    
    /// <summary>
    /// 
    /// </summary>
    Guid CharacteristicUuid { get; }
    
    /// <summary>
    /// 
    /// </summary>
    BleCharacteristicProperties Properties { get; }
}