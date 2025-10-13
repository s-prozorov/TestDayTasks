namespace TASK_1_1.Infrastructure;

public interface ISurfaceLayer<T>
{
    int Width { get; }
    int Height { get; }

    // Получение тайла по координатам
    T GetTileType(int x, int y);

    // Установка тайла по координатам
    void SetTileType(int x, int y, T type);

    // Заполнение всей карты одним типом
    void Fill(T type);

    // Заполнение прямоугольной области
    void FillArea(int startX, int startY, int endX, int endY, T type);

    // Проверка возможности размещения объекта
    bool CanPlaceObject(int startX, int startY, int endX, int endY);

    // Изменение размеров слоя
    void Resize(int newWidth, int newHeight, T defaultType);
}
