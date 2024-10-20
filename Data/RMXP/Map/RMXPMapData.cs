using OneShotMG.src.Entities;
using RMXP2WME.Data.Event;

namespace RMXP2WME.Data.RMXP.Map
{
    public class RMXPMapData
    {
        public int tileset_id;
        public int width;
        public bool autoplay_bgm;
        public AudioFile bgm;
        public bool autoplay_bgs;
        public AudioFile bgs;
        public object[] encounter_list;
        public int encounter_step;
        public RMXPTiles data;
        public EventFixed[] events;
    }
}
