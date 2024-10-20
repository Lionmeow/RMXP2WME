using OneShotMG.src.Map;

namespace RMXP2WME.Data.WME.Map
{
    public class TilesetInfoJsonFull : TilesetInfoJson
    {
        // no clue why WME doesn't use this info normally
        public string name;
        public string tileset_name;
        public string[] autotile_names;
    }
}
