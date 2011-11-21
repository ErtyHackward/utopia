using System;
using System.Collections.Generic;
using System.IO;
using Utopia.Server.Services;
using Utopia.Server.Structs;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Structs;

namespace Utopia.Server.Sample
{
    public class BlueprintRecorderService : Service
    {
        private Server _server;

        private Dictionary<ClientConnection, Blueprint> _blueprints = new Dictionary<ClientConnection, Blueprint>();

        public override string ServiceName
        {
            get { return "Blueprint recorder"; }
        }

        public override void Initialize(Server server)
        {
            _server = server;
            // listen player commands
            _server.CommandsManager.PlayerCommand += CommandsManagerPlayerCommand;

            _server.CommandsManager.RegisterCommand(new BlueprintRecordCommand());
            _server.CommandsManager.RegisterCommand(new BlueprintStopCommand());

        }

        void CommandsManagerPlayerCommand(object sender, Events.PlayerCommandEventArgs e)
        {
            if (e.Command is BlueprintRecordCommand)
            {
                if (e.HaveParameters)
                {
                    if (_blueprints.ContainsKey(e.Connection))
                        _blueprints.Remove(e.Connection);
                    
                    _blueprints.Add(e.Connection, new Blueprint { Name = e.Params[0] });
                    _server.ChatManager.SendMessage(e.Connection, "Blueprint record started", "server");
                }
                else
                {
                    _server.ChatManager.SendMessage(e.Connection, "Error, provide blueprint name", "server");
                }
            }

            if (e.Command is BlueprintStopCommand)
            {
                Blueprint bp;
                if (_blueprints.TryGetValue(e.Connection, out bp))
                {
                    bp.Save(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Utopia\\Blueprints", bp.Name + ".bp"));
                    _blueprints.Remove(e.Connection);
                    _server.ChatManager.SendMessage(e.Connection, "Blueprint saved.", "server");
                }
            }
        }

        public override void Dispose()
        {
            _server.CommandsManager.PlayerCommand -= CommandsManagerPlayerCommand;
        }
    }

    public class Blueprint : IBinaryStorable
    {
        public List<BlueprintAction> Actions { get; private set; }

        public string Name { get; set; }

        public Blueprint()
        {
            Actions = new List<BlueprintAction>();
        }

        public void Normalize()
        {
            var first = Actions[0].Position;

            for (int i = 0; i < Actions.Count; i++)
            {
                Actions[i] = new BlueprintAction { Position = Actions[i].Position - first, CubeId = Actions[i].CubeId };
            }
        }

        public void Save(string fileName)
        {
            using (var sw = new BinaryWriter(File.Open(fileName, FileMode.Create)))
            {
                Save(sw);
                sw.BaseStream.SetLength(sw.BaseStream.Position);
            }
        }

        public void Save(BinaryWriter writer)
        {
            if (!Actions[0].Position.IsZero())
                Normalize();

            writer.Write(Actions.Count);

            for (int i = 0; i < Actions.Count; i++)
            {
                writer.Write(Actions[i].Position);
                writer.Write(Actions[i].CubeId);
            }

        }

        public static Blueprint Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                using (var reader = new BinaryReader(File.OpenRead(fileName)))
                {
                    var bp = new Blueprint();
                    bp.Load(reader);
                    return bp;
                }
            }
            return null;
        }
        
        public void Load(BinaryReader reader)
        {
            Actions.Clear();

            var count = reader.ReadInt32();

            for (int i = 0; i < count; i++)
            {
                var action = new BlueprintAction { Position = reader.ReadVector3I(), CubeId = reader.ReadByte() };
                Actions.Add(action);
            }

        }
    }

    public struct BlueprintAction
    {
        public Vector3I Position;
        public byte CubeId;
    }

    public class BlueprintRecordCommand : IServerCommand
    {
        public string Id
        {
            get { return "bprec"; }
        }
        public string Description
        {
            get { return "Starts recording the blueprint. All blocks modifications will be recordered. Call bpstop to stop recording. Provide blueprint name. \nExample: bprec house1"; }
        }
    }

    public class BlueprintStopCommand : IServerCommand
    {
        public string Id
        {
            get { return "bpstop"; }
        }
        public string Description
        {
            get { return "Stops recording of the blueprint and save all data to the hard disk"; }
        }
    }
}
