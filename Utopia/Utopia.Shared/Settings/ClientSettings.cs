using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Collections.Generic;
using S33M3CoreComponents.Inputs.KeyboardHandler;
using System.IO;
using SharpDX.DXGI;
using S33M3CoreComponents.Config;

namespace Utopia.Shared.Settings
{
    public class ClientSettings
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public static XmlSettingsManager<ClientConfig> Current;

        private static string _texturePack;
        private static string _effectPack;

        public static string TexturePack
        {
            get { return _texturePack ?? @"TexturesPacks\" + Current.Settings.GraphicalParameters.TexturePack + @"\";}
            set { _texturePack = value; }
        }

        public static string EffectPack
        {
            get { return _effectPack ?? @"EffectsPacks\" + Current.Settings.EngineParameters.EffectPack + @"\";}
            set { _effectPack = value; }
        }

        public static string PathRoot { get; set; }

        public static Dictionary<string, List<object>> DynamicLists = new Dictionary<string,List<object>>();

        static ClientSettings()
        {
            //Create the Dynamic list of values

            //List of textures Packs
            try
            {
                PathRoot = "";
                DynamicLists.Add("CLIST_TexturePacks", new List<object>(GetSubFoldersAt(Application.StartupPath + @"\TexturesPacks\")));
                DynamicLists.Add("CLIST_EffectPacks", new List<object>(GetSubFoldersAt(Application.StartupPath + @"\EffectsPacks\")));
            }
            catch (Exception x)
            {
                logger.Error("Unable to enumerate texture/effect packs {0}", x);
            }
        }
        
        private static IEnumerable<string> GetSubFoldersAt(string lootAt)
        {
            if (Directory.Exists(lootAt))
            {
                foreach (var path in Directory.GetDirectories(lootAt))
                {
                    yield return new DirectoryInfo(path).Name;
                }
            }
        }
    }

    public enum ParamInputMethod
    {
        InputBox,
        CheckBox,
        Slider,
        ButtonList
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
        public List<object> ListValues { get; set; }
        public bool NeedRestartAfterChange { get; set; }

        public ParameterAttribute(string paramName,
                                  string info,
                                  string infoSuffix,
                                  ParamInputMethod inputMethod,
                                  bool needRestartAfterChange,
                                  params string[] listValues)
        {
            NeedRestartAfterChange = needRestartAfterChange;
            ParamName = paramName;
            Info = info;
            InfoSuffix = infoSuffix;
            InputMethod = inputMethod;
            MinSliderValue = null;
            MaxSliderValue = null;
            ListValues = new List<object>(listValues);
        }

        public ParameterAttribute(string paramName,
                                  string info,
                                  string infoSuffix,
                                  ParamInputMethod inputMethod,
                                  int minSliderValue,
                                  int maxSliderValue,
                                  bool needRestartAfterChange,
                                  params string[] listValues)
        {
            NeedRestartAfterChange = needRestartAfterChange;
            ParamName = paramName;
            Info = info;
            InfoSuffix = infoSuffix;
            InputMethod = inputMethod;
            MinSliderValue = minSliderValue;
            MaxSliderValue = maxSliderValue;
            ListValues = new List<object>(listValues);
        }
    }

    /// <summary>
    /// Game parameters section
    /// </summary>
    [Serializable]
    public class GameParameters
    {
        [ParameterAttribute("Nick Name", "You nick name in the world", null, ParamInputMethod.InputBox, true)]
        public string NickName { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class GraphicalParameters
    {
        [ParameterAttribute("Visible World Size", "World size in chunk unit between [10 and 50]", " chunk(s)", ParamInputMethod.Slider, 10, 50, true)]
        public int WorldSize { get; set; }
        [ParameterAttribute("Visible World Entity range", "World entities view range in chunk unit between [1 and 20]", " chunk(s)", ParamInputMethod.Slider, 1, 20, false)]
        public int StaticEntityViewSize { get; set; }
        [ParameterAttribute("Landscape fog", "Foggy far away landscape", null, ParamInputMethod.ButtonList, false, "SkyFog", "SimpleFog", "NoFog")]
        public string LandscapeFog { get; set; }
        [ParameterAttribute("Textures pack", "Textures used in-game", null, ParamInputMethod.ButtonList, true, "CLIST_TexturePacks")]
        public string TexturePack { get; set; }
        [ParameterAttribute("Vertical synchronization", "Vertical refresh synchronization", null, ParamInputMethod.CheckBox, false)]
        public bool VSync { get; set; }
        [ParameterAttribute("Multisampling", "MSAA", null, ParamInputMethod.ButtonList, true, "CLIST_MSAA")]
        public SampleDescriptionSetting MSAA { get; set; }
        [ParameterAttribute("Full-screen", "Show game in full screen", null, ParamInputMethod.CheckBox, false)]
        public bool Fullscreen { get; set; }
        [ParameterAttribute("Shadow map", "Direct sun shadows", null, ParamInputMethod.CheckBox, true)]
        public bool ShadowMap { get; set; }
        
        public Point WindowPos { get; set; }
        public Size WindowSize { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class EngineParameters
    {
        [ParameterAttribute("Allocate more threads", "Allocate more threads to speed up all rendering routine", " thread(s)", ParamInputMethod.Slider, 0, 4, false)]
        public int AllocatedThreadsModifier { get; set; }
        [ParameterAttribute("Effects pack", "Effects used in-game", null, ParamInputMethod.ButtonList, true, "CLIST_EffectPacks")]
        public string EffectPack { get; set; }
    }

    /// <summary>
    /// Graphical parameters section
    /// </summary>
    [Serializable]
    public class SoundParameters
    {
        [ParameterAttribute("Global Music Volume", "Set global music volume", "", ParamInputMethod.Slider, 0, 100, false)]
        public int GlobalMusicVolume { get; set; }
        [ParameterAttribute("Global FX Volume", "Set global fx volume", "", ParamInputMethod.Slider, 0, 100, false)]
        public int GlobalFXVolume { get; set; }
    }

    /// <summary>
    /// Keyboard mapping move section
    /// </summary>
    [Serializable]
    public class MoveBindingKeys
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
    /// Keyboard mapping move section
    /// </summary>
    [Serializable]
    public class SystemBindingKeys
    {
        public KeyWithModifier FullScreen;
        public KeyWithModifier LockMouseCursor;
        public KeyWithModifier DebugInfo;
        public KeyWithModifier VSync;
    }

    /// <summary>
    /// Keyboard mapping move section
    /// </summary>
    [Serializable]
    public class GameBindingKeys
    {
        public KeyWithModifier CameraType;
        public KeyWithModifier Chat;
        public KeyWithModifier Use;
        public KeyWithModifier Throw;
        public KeyWithModifier Inventory;
        public KeyWithModifier ToggleInterface;
        public KeyWithModifier Crafting;
        public KeyWithModifier CharSelect;
    }

    /// <summary>
    /// Keyboard mapping section
    /// </summary>
    [Serializable]
    public class KeyboardMapping
    {
        /// <summary>
        /// Move Binding section
        /// </summary>
        public MoveBindingKeys Move { get; set; }

        /// <summary>
        /// Game Binding section
        /// </summary>
        public GameBindingKeys Game { get; set; }

        /// <summary>
        /// System Binding section
        /// </summary>
        public SystemBindingKeys System { get; set; }
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
        /// Token to access api methods
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// SHA1 hash of the password
        /// </summary>
        public string PasswordHash { get; set; }

        /// <summary>
        /// Compute the first time the engine is started
        /// </summary>
        public int DefaultAllocatedThreads { get; set; }

        /// <summary>
        /// Will put a Max value on frame time
        /// Use only if bad performances occurs !! (Bad GPU)
        /// </summary>
        public int FrameLimiter { get; set; }

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
                    EngineParameters = new EngineParameters
                    {
                        AllocatedThreadsModifier = 0,
                        EffectPack = "Default"
                    },
                    SoundParameters = new SoundParameters
                    {
                        GlobalFXVolume = 50,
                        GlobalMusicVolume = 50
                    },
                    GraphicalParameters = new GraphicalParameters
                    {
                        WorldSize = 20,
                        StaticEntityViewSize = 5,
                        TexturePack = "Default",
                        LandscapeFog = "SkyFog",
                        VSync = true,
                        MSAA = new SampleDescriptionSetting { SampleDescription = new SampleDescription(1, 0) }
                    },
                    KeyboardMapping = new KeyboardMapping
                    {
                        Move = new MoveBindingKeys
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
                        Game = new GameBindingKeys
                        {
                            CameraType = new KeyWithModifier { MainKey = Keys.F5, Modifier = Keys.None, Info = "Change Camera type" },
                            Chat = new KeyWithModifier { MainKey = Keys.Enter, Modifier = Keys.None, Info = "Open/Close the chat" },
                            Use = new KeyWithModifier { MainKey = Keys.E, Modifier = Keys.None, Info = "Use" },
                            Throw = new KeyWithModifier { MainKey = Keys.Back, Modifier = Keys.None, Info = "Throw" },
                            Inventory = new KeyWithModifier { MainKey = Keys.I, Modifier = Keys.None, Info = "Inventory" },
                            ToggleInterface = new KeyWithModifier { MainKey = Keys.Z, Modifier = Keys.Alt, Info = "Toggle Interface" },
                            Crafting = new KeyWithModifier { MainKey = Keys.O, Info = "Toggle crafting window" },
                            CharSelect = new KeyWithModifier { MainKey = Keys.N, Info = "Toggle character selection window" },
                        },
                        System = new SystemBindingKeys
                        {
                            VSync = new KeyWithModifier { MainKey = Keys.F8, Modifier = Keys.RControlKey, Info = "Enable/Disable VSync" },
                            LockMouseCursor = new KeyWithModifier { MainKey = Keys.Tab, Modifier = Keys.None, Info = "Locking/Unlocking mouse cursor" },
                            FullScreen = new KeyWithModifier { MainKey = Keys.F11, Modifier = Keys.None, Info = "Fullscreen switcher" },
                            DebugInfo = new KeyWithModifier { MainKey = Keys.F3, Modifier = Keys.None, Info = "Diplay technical informations" },
                        }
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

                config.KeyboardMapping.Move.Up = Keys.X;
                config.KeyboardMapping.Move.Down = Keys.W;
                config.KeyboardMapping.Move.Forward = Keys.Z;
                config.KeyboardMapping.Move.Backward = Keys.S;
                config.KeyboardMapping.Move.StrafeLeft = Keys.Q;
                config.KeyboardMapping.Game.ToggleInterface = new KeyWithModifier { MainKey = Keys.W, Modifier = Keys.Alt, Info = "Toggle Interface" };

                return config;
            }
        }

        public void Initialize()
        {
        }
    }
}
