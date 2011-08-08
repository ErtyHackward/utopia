using System;
using System.Windows.Forms;
using Utopia.Shared.Config;

namespace Utopia
{
    /// <summary>
    /// World parameters section
    /// </summary>
    [Serializable]
    public class WorldParameters
    {
        public int WorldSize { get; set; }
        public int CloudsClayers { get; set; }
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
        /// Stop/start World Clock
        /// </summary>
        public KeyWithModifier FreezeTime;
    }

    /// <summary>
    /// Main settings model for client
    /// </summary>
    [Serializable]
    public class ClientConfig
    {
        /// <summary>
        /// World parameters config section
        /// </summary>
        public WorldParameters WorldParameters { get; set; }

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
                    WorldParameters = new WorldParameters
                    {
                        WorldSize = 32,
                        CloudsClayers = 1
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
                            Run = Keys.Shift,
                            Mode = Keys.M
                        },
                        Console = Keys.Oemtilde,
                        VSync = new KeyWithModifier { MainKey = Keys.F8, Modifier = Keys.RControlKey },
                        LockMouseCursor = Keys.Tab,
                        FullScreen = Keys.F11,
                        DebugMode = Keys.F2,
                        DebugInfo = Keys.F3,
                        FreezeTime = Keys.F10
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
                config.KeyboardMapping.Move.Up = Keys.W;
                config.KeyboardMapping.Move.Forward = Keys.Z;
                config.KeyboardMapping.Move.StrafeLeft = Keys.Q;

                return config;
            }
        }
    }
}
