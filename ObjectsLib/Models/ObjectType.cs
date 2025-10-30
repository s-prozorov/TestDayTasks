namespace ObjectsLib.Models;

public enum ObjectType : int
{
    /// <summary>
    /// База игрока
    /// </summary>
    PlayerBase = 0,
    
    /// <summary>
    /// Шахта (ресурсный объект)
    /// </summary>
    Mine = 1,
    
    /// <summary>
    /// Временный объект
    /// </summary>
    Temporary = 2
}