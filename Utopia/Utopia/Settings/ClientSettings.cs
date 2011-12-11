using System;
using Utopia.Shared.Config;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Utopia.Settings
{
    public class ClientSettings
    {
        public static XmlSettingsManager<ClientConfig> Current;
        public static string TexturePack = "TexturesPacks\\Default\\";
        public static string EffectPack = "EffectsPacks\\Default\\";
    }

    /// <summary>
    /// Game parameters section
    /// </summary>
    [Serializable]
    public class ServersList
    {
        [XmlElement("Servers")]
        public List<ServerSetting> Servers = new List<ServerSetting>();
    }

    [Serializable]
    public class ServerSetting
    {
        public string IPAddress { get; set; }
        public string ServerName { get; set; }
        public string DefaultUser { get; set; }
        public string TexturePack { get; set; }
        public string EffectPack { get; set; }

        public override string ToString()
        {
            return ServerName + " [" + IPAddress + "]";
        }

        public ServerSetting()
        {
            TexturePack = "Default";
            EffectPack = "Default";
        }

        public string ID { get { return ServerName + IPAddress + DefaultUser; } }

    }

    /// <summary>
    /// Game parameters section
    /// </summary>
    [Serializable]
    public class GameParameters
    {
        public string NickName { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class GraphicalParameters
    {
        public int WorldSize { get; set; }
        public int CloudsQuality { get; set; }
        public int AllocatedThreadsModifier { get; set; }
        public int LightPropagateSteps { get; set; }
    }

    /// <summary>
    /// Keyboard mapping move section
    /// </summary>
    [Serializable]
    public class MoveParameters
    {
        public KeyWithModifier Forward;
        public KeyWithModifier Backward;
        public KeyWithModifier StrafeLeft;
        public KeyWithModifier StrafeRight;
        public KeyWithModifier Down;
        public KeyWithModifier Up;
        public KeyWithModifier Jump;
        /// <summary>
        /// Change the movement Mode (From Fly &lt;=&gt; Walk)
        /// </summary>
        public KeyWithModifier Mode;
        /// <summary>
        /// Hold On to run in walk Movemode
        /// </summary>
        public KeyWithModifier Run;
    }

    /// <summary>
    /// Keyboard mapping section
    /// </summary>
    [Serializable]
    public class KeyboardMapping
    {
        /// <summary>
        /// Move section
        /// </summary>
        public MoveParameters Move { get; set; }

        public KeyWithModifier FullScreen;
        public KeyWithModifier LockMouseCursor;

        /// <summary>
        /// Enter graphical debug mode
        /// </summary>
        public KeyWithModifier DebugMode;

        /// <summary>
        /// Diplay technical informations
        /// </summary>
        public KeyWithModifier DebugInfo;
        public KeyWithModifier VSync;
        public KeyWithModifier Console;
        /// <summary>
        /// Open/close game chat
        /// </summary>
        public KeyWithModifier Chat;

        /// <summary>
        /// Stop/start World Clock
        /// </summary>
        public KeyWithModifier FreezeTime;

        /// <summary>
        /// use picked entity
        /// </summary>
        public KeyWithModifier Use;

        /// <summary>
        /// Throw equipped tool(s)
        /// </summary>
        public KeyWithModifier Throw;
        
        /// <summary>
        /// Open/close inventory
        /// </summary>
        public KeyWithModifier Inventory;

        /// <summary>
        /// Open/close the map
        /// </summary>
        public KeyWithModifier Map;
    }

    /// <summary>
    /// Main settings model for client
    /// </summary>
    [XmlRoot("ClientConfig")]
    [Serializable]
    public class ClientConfig : IConfigClass
    {
        /// <summary>
        /// Last user email as login
        /// </summary>
        public string Login { get; set; }

        /// <summary>
        /// Game parameters config section
        /// </summary>
        public GameParameters GameParameters { get; set; }

        /// <summary>
        /// World parameters config section
        /// </summary>
        public GraphicalParameters GraphicalParameters { get; set; }

        /// <summary>
        /// Keyboard mapping config section
        /// </summary>
        public KeyboardMapping KeyboardMapping { get; set; }

        /// <summary>
        /// Server Lists
        /// </summary>
        public ServersList ServersList { get; set; }

        /// <summary>
        /// Gets default configuration for qwerty keyboard type
        /// </summary>
        public static ClientConfig DefaultQwerty
        {
            get
            {
                return new ClientConfig
                {
                    GameParameters = new GameParameters
                    {
                        NickName = "Utopia Guest"
                    },
                    GraphicalParameters = new GraphicalParameters
                    {
                        WorldSize = 32,
                        CloudsQuality = 1,
                        AllocatedThreadsModifier = 0,
                        LightPropagateSteps = 8
                    },
                    KeyboardMapping = new KeyboardMapping
                    {
                        Move = new MoveParameters
                        {
                            Up = Keys.Z,
                            Down = Keys.X,
                            Forward = Keys.W,
                            Backward = Keys.S,
                            StrafeLeft = Keys.A,
                            StrafeRight = Keys.D,
                            Jump = Keys.Space,
                            Run = Keys.LShiftKey,
                            Mode = Keys.F
                        },
                        Console = Keys.Oemtilde,
                        VSync = new KeyWithModifier { MainKey = Keys.F8, Modifier = Keys.RControlKey, Info = "Enable/Disable VSync" },
                        LockMouseCursor = new KeyWithModifier { MainKey = Keys.Tab, Modifier = Keys.None, Info = "Locking/Unlocking mouse cursor" },
                        FullScreen = new KeyWithModifier { MainKey = Keys.F11, Modifier = Keys.None, Info = "Fullscreen switcher" },
                        DebugMode = new KeyWithModifier { MainKey = Keys.F2, Modifier = Keys.None, Info = "Enter graphical debug mode" },
                        DebugInfo = new KeyWithModifier { MainKey = Keys.F3, Modifier = Keys.None, Info = "Diplay technical informations" },
                        FreezeTime = new KeyWithModifier { MainKey = Keys.F10, Modifier = Keys.None, Info = "Stop World Clock" },
                        Chat = new KeyWithModifier { MainKey = Keys.Enter, Modifier = Keys.None, Info = "Open/Close the chat" },
                        Use = new KeyWithModifier { MainKey = Keys.E, Modifier = Keys.None, Info = "Use" },
                        Throw = new KeyWithModifier { MainKey = Keys.Back, Modifier = Keys.None, Info = "Throw" },
                        Inventory = new KeyWithModifier { MainKey = Keys.I, Modifier = Keys.None, Info = "Inventory" },
                        Map = new KeyWithModifier { MainKey = Keys.M, Modifier = Keys.None, Info = "Open/close the map" }
                    },
                    ServersList = new ServersList()
                };
            }
        }

        /// <summary>
        /// Gets default configuration for azerty keyboard type
        /// </summary>
        public static ClientConfig DefaultAzerty
        {
            get
            {
                var config = DefaultQwerty;

                config.KeyboardMapping.Console = Keys.F4;
                config.KeyboardMapping.Move.Up = Keys.X;
                config.KeyboardMapping.Move.Down = Keys.W;
                config.KeyboardMapping.Move.Forward = Keys.Z;
                config.KeyboardMapping.Move.Backward = Keys.S;
                config.KeyboardMapping.Move.StrafeLeft = Keys.Q;

                return config;
            }
        }

        public void Initialize()
        {
        }
    }
}
