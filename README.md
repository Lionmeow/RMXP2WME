Tool for converting RMXP files to files understandable by WME<br />
Utilizes a modified version of [RMXP-Extractor](https://github.com/Speak2Erase/RMXP-Extractor) to generate JSONs from RMXP binaries<p />
Still WIP, will VERY likely change in the future!<p />

Usage:<br />
```
RMXP2WME <options>

-i <path-to-input> - JSON file exported via rmxp_extractor, or a directory containing many JSON files
-o <path-to-output> - Location to export WME files to. Exports in the same directory as input if not specified.

-f - Force overwrites any conflicting files

-m - Enables map conversion
-m:r <from> <to> - Specifies a room ID to change instances of to a new room ID
-m:rm  - Remaps room IDs in Room Move event commands
-m:rt - Remaps room IDs in room transitions

-t - Enables tileset conversion
-t:p - Creates a patched WME tileset file
-t:l - Creates a file detailing all tileset changes
```
