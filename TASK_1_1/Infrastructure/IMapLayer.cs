using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TASK_1_1.Layers;

namespace TASK_1_1.Infrastructure;

public interface IMapLayer<T>
{
    int Width { get; }
    int Height { get; }

    // Получения типа тайла по координатам (x, y)
    T GetTileType(int x, int y);

    // Установки типа тайла по координатам (x, y)
    void SetTileType(int x, int y, T type);

    // Установка или Замены типа тайла для массива ([x, y])
    void ReplaceTiles(T[,] newTiles);

    // Заполнение области типом
    void FillArea(int startX, int startY, int endX, int endY, T type);

    // Проверка возможности размещения объекта
    bool CanPlaceObject(int startX, int startY, int endX, int endY);

    // Дополнительно: Resize
    void Resize(int newWidth, int newHeight, T defaultType);

    // Методы создания из коллекции тайлов (массива, списка и т.д.)
    Surface FromArray(TileEnum[,] source);
}
