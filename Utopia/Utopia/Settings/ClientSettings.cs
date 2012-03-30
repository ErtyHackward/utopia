using System;
using Utopia.Shared.Config;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;
using S33M3CoreComponents.Inputs.KeyboardHandler;

namespace Utopia.Settings
{
    public class ClientSettings
    {
        public static XmlSettingsManager<ClientConfig> Current;
        public static string TexturePack = "TexturesPacks\\Default\\";
        public static string EffectPack = "EffectsPacks\\Default\\";
    }

    public enum ParamInputMethod
    {
        InputBox,
        CheckBox,
        Slider,
        List
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ParameterAttribute : Attribute
    {
        public string ParamName { get; private set; }
        public string Info { get; private set; }
        public string InfoSuffix { get; private set; }
        public ParamInputMethod InputMethod { get; private set; }
        public int? MinSliderValue { get; private set; }
        public int? MaxSliderValue { get; private set; }
        public List<string> ListValues { get; set; }

        public ParameterAttribute(string paramName,
                                  string info,
                                  string infoSuffix,
                                  ParamInputMethod inputMethod,     
                                  params string[] listValues)
        {
            ParamName = paramName;
            Info = info;
            InfoSuffix = infoSuffix;
            InputMethod = inputMethod;
            MinSliderValue = null;
            MaxSliderValue = null;
            ListValues = new List<string>(listValues);
        }

        public ParameterAttribute(string paramName,
                                  string info,
                                  string infoSuffix,
                                  ParamInputMethod inputMethod,
                                  int minSliderValue,
                                  int maxSliderValue,
                                  params string[] listValues)
        {
            ParamName = paramName;
            Info = info;
            InfoSuffix = infoSuffix;
            InputMethod = inputMethod;
            MinSliderValue = minSliderValue;
            MaxSliderValue = maxSliderValue;
            ListValues = new List<string>(listValues);
        }
    }

    /// <summary>
    /// Game parameters section
    /// </summary>
    [Serializable]
    public class GameParameters
    {
        [ParameterAttribute("Nick Name", "You nick name in the world", null, ParamInputMethod.InputBox)]
        public string NickName { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class GraphicalParameters
    {
        [ParameterAttribute("Visible World Size", "World size in chunk unit between [10 and 32]", " chunk(s)" , ParamInputMethod.Slider, 10, 32)]
        public int WorldSize { get; set; }
        [ParameterAttribute("Cloud's type", "Cloud visualisation type", null, ParamInputMethod.List, "None", "2D", "3D")]
        public string CloudsQuality { get; set; }
        [ParameterAttribute("Light propagation", "Maximum size of light propagation in block unit", " block(s)" ,ParamInputMethod.Slider, 4, 12)]
        public int LightPropagateSteps { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class EngineParameters
    {
        [ParameterAttribute("Allocate more threads", "Allocate more threads to speed up all rendering routine", " thread(s)", ParamInputMethod.Slider, 0, 4)]
        public int AllocatedThreadsModifier { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class SoundParameters
    {
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
        /// Compute the first time the engine is started
        /// </summary>
        public int DefaultAllocatedThreads { get; set; }

        /// <summary>
        /// Game parameters config section
        /// </summary>
        public GameParameters GameParameters { get; set; }

        /// <summary>
        /// Game parameters config section
        /// </summary>
        public SoundParameters SoundParameters { get; set; }

        /// <summary>
        /// Game parameters config section
        /// </summary>
        public EngineParameters EngineParameters { get; set; }

        /// <summary>
        /// World parameters config section
        /// </summary>
        public GraphicalParameters GraphicalParameters { get; set; }

        /// <summary>
        /// Keyboard mapping config section
        /// </summary>
        public KeyboardMapping KeyboardMapping { get; set; }

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
                    EngineParameters = new EngineParameters()
                    {
                        AllocatedThreadsModifier = 0
                    },
                    SoundParameters = new SoundParameters()
                    {
                    },
                    GraphicalParameters = new GraphicalParameters
                    {
                        WorldSize = 32,
                        CloudsQuality = "2D",
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
                    }
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
