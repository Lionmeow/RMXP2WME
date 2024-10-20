using System;

namespace RMXP2WME
{
    [Flags]
    public enum ProgramFlags
    {
        None = 0,
        EXPORT_MAP = 1,
        EXPORT_TILESET = 2,
        REMAP_ROOM_MOVE = 4,
        REMAP_ROOM_TRANSITION = 8,
        CREATE_TILESET_PATCH = 16,
        CREATE_TILESET_LOG = 32,
        FORCE_EXPORT = 64,
    }
}
