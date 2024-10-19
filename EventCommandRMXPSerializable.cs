using OneShotMG.src.Entities;

namespace RMXP2WME
{
    public class EventCommandRMXPSerializable
    {
        public int code;
        public int indent;
        public object[] parameters; // literally the one reason that this needs to exist. agony.
        public MoveRouteRMXPSerializable move_route;
        public AudioFile audio_file;
    }
}
