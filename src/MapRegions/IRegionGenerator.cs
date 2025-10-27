namespace MapRegions;

public interface IRegionGenerator
{
    ushort[] Generate(int width, int height, int regionCount);
}

