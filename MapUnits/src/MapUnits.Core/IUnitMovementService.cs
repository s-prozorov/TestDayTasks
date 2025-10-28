namespace MapUnits.Core;

public interface IUnitMovementService
{
    void StartMovement(string unitId, float targetX, float targetY);
    List<Unit> GetUnitsInArea(int x, int y, int width, int height);
    List<Movement> GetMovementsInArea(int x, int y, int width, int height);
    Unit? GetUnit(string unitId);
    void AddUnit(Unit unit);
}


