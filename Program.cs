﻿using Newtonsoft.Json;
using OneShotMG.src.Entities;
using RMXP2WME.Data.Event;
using RMXP2WME.Data.RMXP.Map;
using RMXP2WME.Data.WME.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Tiled;

namespace RMXP2WME
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // TODO: check if in WME directory and add assembly resolve
            // unneeded rn because it's MY development environment

            string inputPath = string.Empty;
            string outputPath = string.Empty;
            ProgramFlags flags = ProgramFlags.None;
            
            Dictionary<int, int> idMap = new Dictionary<int, int>();

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-i":
                        i++;

                        if (i == args.Length)
                        {
                            Console.WriteLine("Input path not specified.");
                            Console.WriteLine();
                            Help();
                            return;
                        }

                        inputPath = Path.GetFullPath(args[i]);
                        break;
                    case "-o":
                        i++;

                        if (i == args.Length)
                        {
                            Console.WriteLine("Output path not specified.");
                            Console.WriteLine();
                            Help();
                            return;
                        }

                        outputPath = Path.GetFullPath(args[i]);
                        break;
                    case "-f":
                        Console.WriteLine("WARNING: Overwrite files specified.");
                        Console.WriteLine("If you like any conflicting files, you've made a grave mistake.");
                        flags |= ProgramFlags.FORCE_EXPORT;
                        break;
                    case "-m":
                        flags |= ProgramFlags.EXPORT_MAP;
                        break;
                    case "-m:r":
                        int from;
                        int to;

                        i++;
                        if (i == args.Length || !int.TryParse(args[i], out from))
                        {
                            Console.WriteLine("Remap values not specified");
                            Console.WriteLine();
                            Help();
                            return;
                        }
                        i++;
                        if (i == args.Length || !int.TryParse(args[i], out to))
                        {
                            Console.WriteLine("Remap values not specified");
                            Console.WriteLine();
                            Help();
                            return;
                        }

                        idMap[from] = to;
                        break;
                    case "-m:rm":
                        flags |= ProgramFlags.REMAP_ROOM_MOVE;
                        break;
                    case "-m:rt":
                        Console.WriteLine("WARNING: Transition remap option selected.");
                        Console.WriteLine("If your room transitions don't use the same variables as OneShot, you're gonna have a bad time!");
                        flags |= ProgramFlags.REMAP_ROOM_TRANSITION;
                        break;
                    default:
                        Console.WriteLine($"Unknown argument: {args[i]}");
                        Console.WriteLine();
                        Help();
                        return;
                }
            }

            if (inputPath == string.Empty)
            {
                Console.WriteLine($"Input not specified.");
                Console.WriteLine();
                Help();
                return;
            }


            if (Directory.Exists(inputPath))
            {
                Console.WriteLine("Directory specified, exporting all files within.");

                if (outputPath == string.Empty)
                    outputPath = Path.Combine(inputPath, "WME Export");
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                foreach (string file in Directory.GetFiles(inputPath))
                    ProcessFile(file, outputPath, idMap, flags);
            }
            else if (File.Exists(inputPath))
            {
                if (outputPath == string.Empty)
                    outputPath = Path.Combine(Path.GetDirectoryName(inputPath), "WME Export");
                if (!Directory.Exists(outputPath))
                    Directory.CreateDirectory(outputPath);

                ProcessFile(inputPath, outputPath, idMap, flags);
            }
            else
            {
                Console.WriteLine($"Specified file does not exist ({inputPath})");
                Console.WriteLine();
                Help();
                return;
            }
        }

        public static void Help()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("RMXP2WME <options>");
            Console.WriteLine();
            Console.WriteLine("-i <path-to-input> - JSON file exported via rmxp_extractor, or a directory containing many JSON files");
            Console.WriteLine("-o <path-to-output> - Location to export WME files to. Exports in the same directory as input if not specified.");
            Console.WriteLine();
            Console.WriteLine("-f - Force overwrites any conflicting files");
            Console.WriteLine();
            Console.WriteLine("-m - Enables map conversion");
            Console.WriteLine("-m:r <from> <to> - Specifies a room ID to change instances of to a new room ID");
            Console.WriteLine("-m:rm  - Remaps room IDs in Room Move event commands");
            Console.WriteLine("-m:rt - Remaps room IDs in room transitions");
            Console.WriteLine();
            Console.WriteLine("-t - Enables tileset conversion"); // TODO: this and next two unimplemented!!
            Console.WriteLine("-t:p - Creates a patched WME tileset file");
            Console.WriteLine("-t:l - Creates a file detailing all tileset changes");
        }

        // returns true if continue, else false
        public static bool ProcessOutputDirectory(string path, string sourceFile, params string[] filesToCreate)
        {
            bool hasConflict = false;
            foreach (string name in filesToCreate)
            {
                string pathToCreate = Path.Combine(path, name);
                if (File.Exists(pathToCreate))
                {
                    hasConflict = true;
                    break;
                }
            }

            if (hasConflict)
            {
                // don't want users to unknowingly overwrite files!!
                Console.WriteLine($"Conflicting file(s) in output directory ({path}) when processing {sourceFile}.");
                bool doContinue = false;
                while (true)
                {
                    Console.WriteLine("Continue? [y/n]");
                    string answer = Console.ReadLine();
                    if (answer.ToLower() == "y" || answer.ToLower() == "n")
                    {
                        doContinue = answer.ToLower() == "y";
                        break;
                    }
                    Console.WriteLine("Invalid input.");
                }

                if (!doContinue)
                    return false;
            }

            return true;
        }
        public static bool ProcessFile(string input, string outputPath, Dictionary<int, int> remap, ProgramFlags flags)
        {
            string name = Path.GetFileName(input);
            string safeName = Path.GetFileNameWithoutExtension(input);

            if (Path.GetExtension(input) != ".json")
            {
                Console.WriteLine($"{name} is not a .JSON file. Skipping ...");
                return false;
            }

            bool forceExport = (flags & ProgramFlags.FORCE_EXPORT) == ProgramFlags.FORCE_EXPORT;
            if ((flags & ProgramFlags.EXPORT_MAP) == ProgramFlags.EXPORT_MAP && 
                TryParseMapJSON(input, remap, (flags & ProgramFlags.REMAP_ROOM_MOVE) == ProgramFlags.REMAP_ROOM_MOVE, 
                (flags & ProgramFlags.REMAP_ROOM_TRANSITION) == ProgramFlags.REMAP_ROOM_TRANSITION, out RMXPMapJson json))
            {
                if (!forceExport && !ProcessOutputDirectory(outputPath, $"events_{safeName}.json", $"{safeName}.tmx", $"music_{safeName}.json"))
                {
                    Console.WriteLine($"Conflicting files for {name}. Skipping ...");
                    return false;
                }

                if (!ProcessAndOutputWMEMap(json, outputPath, safeName, forceExport))
                {
                    Console.WriteLine($"Did not fully export map for {name}");
                    return false;
                }
                return true;
            }
            else
            {
                Console.WriteLine($"{name} did not match any enabled export types. Skipping ...");
                return false;
            }
        }

        public static bool TryParseMapJSON(string inputPath, Dictionary<int, int> remap, bool remapMoveCommands, bool remapTransitions, out RMXPMapJson json)
        {
            try
            {
                // I hope I did this right????
                json = JsonConvert.DeserializeObject<RMXPMapJson>(File.ReadAllText(inputPath));
            }
            catch
            {
                // is this good practice
                json = null;
                return false;
            }

            // handle bad event command paramters that break everything
            foreach (EventFixed ev in json.data.events)
            {
                foreach (EventFixed.Page p in ev.pages)
                {
                    p.list = p.list.Where(x => x.code != 509).ToArray();
                    foreach (EventCommandFixed command in p.list)
                    {
                        if (remapTransitions && command.code == 122 && command.parameters[0].ToString() == "6")
                        {
                            int value = int.Parse(command.parameters[4].ToString());
                            if (remap.ContainsKey(value))
                                command.parameters[4] = remap[value].ToString();
                        }
                        if (command.code == 201 && command.parameters[0].ToString() != "1")
                        {
                            int value = int.Parse(command.parameters[1].ToString());
                            if (remap.ContainsKey(value))
                                command.parameters[1] = remap[value].ToString();
                        }
                        if (command.code == 102)
                            command.parameters[0] = command.parameters[0].ToString();

                        // bad awful terrible stupid bad code
                        if (command.move_route != null)
                        {
                            foreach (MoveCommandFixed movecommand in command.move_route.list)
                            {
                                for (int i = 0; i < movecommand.parameters.Length; i++)
                                {
                                    if (movecommand.parameters[i].GetType() != typeof(string))
                                    {
                                        string stringified = movecommand.parameters[i].ToString();
                                        if (stringified.Contains("pitch"))
                                        {
                                            movecommand.audio_file = JsonConvert.DeserializeObject<AudioFile>(stringified);
                                            List<object> newParameters = movecommand.parameters.ToList();
                                            newParameters.RemoveAt(i);
                                            movecommand.parameters = newParameters.ToArray();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return true;
        }
        // returns true if export was successful, otherwise false
        public static bool ProcessAndOutputWMEMap(RMXPMapJson json, string outputPath, string name, bool forceContinue)
        {
            string eventsName = Path.Combine(outputPath, $"events_{name}.json");
            string tilesName = Path.Combine(outputPath, $"{name}.tmx");
            string musicName = Path.Combine(outputPath, $"music_{name}.json");

            // spitting out events and music is easy; game already has existing data classes for this and said data classes are tiny
            try
            {
                MapEventsFixed events = new MapEventsFixed();
                events.events = json.data.events;
                using (StreamWriter sw = File.CreateText(eventsName))
                    sw.Write(JsonConvert.SerializeObject(events, Formatting.Indented));
                Console.WriteLine($"Successfully created events JSON at {eventsName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred trying to create events JSON:");
                Console.WriteLine(ex.ToString());
                return false;
            }

            // the events JSON not failing but the music JSON failing feels unlikely. but error handling is Cool
            try
            {
                MapMusic music = new MapMusic();
                music.autoplay_bgm = json.data.autoplay_bgm;
                music.bgm = json.data.bgm;
                using (StreamWriter sw = File.CreateText(musicName))
                    sw.Write(JsonConvert.SerializeObject(music, Formatting.Indented));
                Console.WriteLine($"Successfully created music JSON at {musicName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred trying to create music JSON:");
                Console.WriteLine(ex.ToString());
                return false;
            }

            // TMX will have to be constructed from scratch. heavens.
            // most likely point of failure!! you will be seeing this error message a lot
            try
            {
                Map map = new Map();
                map.version = "1.2";
                map.tiledversion = "1.2.4";
                map.orientation = Orientation.orthogonal;
                map.renderorder = RenderOrder.rightdown;
                map.width = json.data.data.xsize;
                map.height = json.data.data.ysize;
                map.tilewidth = 16;
                map.tileheight = 16;
                map.nextobjectid = 1;

                map.properties = new Property[1]
                {
                    new Property()
                    {
                        name = "tileset_id",
                        type = PropertyType.@int,
                        value = json.data.tileset_id.ToString()
                    }
                };

                // I SHOULD add error handling in case oneshot_tilesets.json
                // however, cuz we already check for if the user's running in the correct folder, it shouldn't matter unless something HORRID happens
                TilesetsInfoJsonFull allTilesets =
                    JsonConvert.DeserializeObject<TilesetsInfoJsonFull>(File.ReadAllText(
                        Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), @"gamedata\oneshot_tilesets.json")));

                TilesetInfoJsonFull appropriateTileset = allTilesets.tilesets.First(x => x.id == json.data.tileset_id);

                // first 8 are autotiles, last one is the actual tileset
                map.tileset = new TileSet[9]
                {
                    new TileSet()
                    {
                        firstgid = 1,
                        firstgidSpecified = true,
                        source = $"../autotiles/blank.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 48,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 1 ? $"../autotiles/blank1.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[0] == string.Empty ? "blank1" : appropriateTileset.autotile_names[0])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 96,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 2 ? $"../autotiles/blank2.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[1] == string.Empty ? "blank2" : appropriateTileset.autotile_names[1])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 144,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 3 ? $"../autotiles/blank3.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[2] == string.Empty ? "blank3" : appropriateTileset.autotile_names[2])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 192,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 4 ? $"../autotiles/blank4.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[3] == string.Empty ? "blank4" : appropriateTileset.autotile_names[3])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 240,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 5 ? $"../autotiles/blank5.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[4] == string.Empty ? "blank5" : appropriateTileset.autotile_names[4])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 288,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 6 ? $"../autotiles/blank6.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[5] == string.Empty ? "blank6" : appropriateTileset.autotile_names[5])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 336,
                        firstgidSpecified = true,
                        source = appropriateTileset.autotile_names.Length < 7 ? $"../autotiles/blank7.tsx" :
                            $"../autotiles/{(appropriateTileset.autotile_names[6] == string.Empty ? "blank7" : appropriateTileset.autotile_names[6])}.tsx"
                    },
                    new TileSet()
                    {
                        firstgid = 384,
                        firstgidSpecified = true,
                        source = $"../tilesets/{appropriateTileset.tileset_name}.tsx"
                    }
                };

                map.Items = new Layer[json.data.data.zsize];
                int perLayer = json.data.data.num_of_elements / json.data.data.zsize;
                for (int i = 0; i < json.data.data.zsize; i++)
                {
                    map.Items[i] = new TileLayer()
                    {
                        // though the TMX specifies a layer ID, there's no property for that, so I'm assuming that's automatically done
                        name = $"Tile Layer {i + 1}",
                        width = json.data.data.xsize,
                        height = json.data.data.ysize,
                        data = new Tiled.Data()
                        {
                            compressionSpecified = false,
                            encoding = Encoding.csv,
                            encodingSpecified = true,
                            // skip already processed elements, take new elements, combine into the CSV format
                            Value = string.Join(",", json.data.data.elements.Skip(i * perLayer).Take(perLayer).Select(x => x.ToString()))
                        }
                    };
                }

                // using the same method that the game does to deserialize the .TMX; good luck!
                // hey, I just realized: .TMX probably stands for Tilemap XML or something like that
                using (Stream s = File.Create(tilesName))
                    new XmlSerializer(typeof(Map)).Serialize(s, map);
                Console.WriteLine($"Successfully created tilemap TMX at {tilesName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred trying to create tilemap TMX:");
                Console.WriteLine(ex.ToString());
                return false;
            }

            return true;
        }
    }
}
