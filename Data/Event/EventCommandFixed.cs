using OneShotMG.src.Entities;

namespace RMXP2WME.Event
{
    public class EventCommandFixed
    {
        public int code;
        public int indent;
        public object[] parameters; // literally the one reason that this needs to exist. agony.
        public MoveRouteRMXPFixed move_route;
        public AudioFile audio_file;
    }
}
