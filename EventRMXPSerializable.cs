using OneShotMG.src.Entities;

namespace RMXP2WME
{
    public class EventRMXPSerializable
    {
        public int id;
        public string name = string.Empty;
        public int x;
        public int y;
        public Page[] pages;

        public class Page
        {
            public Event.Page.Condition condition;
            public Event.Page.Graphic graphic;
            public int move_type;
            public int move_speed = 2;
            public int move_frequency = 2;
            public MoveRoute move_route;
            public bool walk_anime;
            public bool step_anime;
            public bool direction_fix;
            public bool through;
            public bool always_on_top;
            public bool always_on_bottom;
            public int trigger;
            public EventCommandRMXPSerializable[] list;
        }
    }
}
