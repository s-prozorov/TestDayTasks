using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TASK_1_1;

struct Tile
{
    public TileEnum Type;

    public bool CanPlaceObject => Type == TileEnum.Plain;
}