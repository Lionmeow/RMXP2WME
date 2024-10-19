using OneShotMG.src.Entities;

namespace RMXP2WME
{
    public class RMXPData
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
        public EventRMXPSerializable[] events;
    }
}
