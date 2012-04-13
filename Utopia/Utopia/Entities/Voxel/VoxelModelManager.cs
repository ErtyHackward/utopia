using System;
using System.Collections.Generic;
using Utopia.Network;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using Utopia.Shared.Structs;
using S33M3DXEngine.Main;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Holds all voxel models. Can request a model from the server
    /// </summary>
    public class VoxelModelManager : GameComponent
    {
        private readonly IVoxelModelStorage _storage;
        private readonly ServerComponent _server;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<Md5Hash, VisualVoxelModel> _models = new Dictionary<Md5Hash, VisualVoxelModel>();
        private readonly HashSet<Md5Hash> _pengingModels = new HashSet<Md5Hash>();

        /// <summary>
        /// Occurs when a voxel moded received from the server
        /// </summary>
        public event EventHandler<VoxelModelReceivedEventArgs> VoxelModelReceived;

        private void OnVoxelModelReceived(VoxelModelReceivedEventArgs e)
        {
            var handler = VoxelModelReceived;
            if (handler != null) handler(this, e);
        }


        public VoxelModelManager(IVoxelModelStorage storage, ServerComponent server, VoxelMeshFactory voxelMeshFactory)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            if (server == null) throw new ArgumentNullException("server");
            _storage = storage;
            _server = server;
            _voxelMeshFactory = voxelMeshFactory;

            _server.MessageVoxelModelData += ServerConnectionMessageVoxelModelData;
        }

        public override void Dispose()
        {
            _server.MessageVoxelModelData -= ServerConnectionMessageVoxelModelData;
            base.Dispose();
        }

        void ServerConnectionMessageVoxelModelData(object sender, ProtocolMessageEventArgs<VoxelModelDataMessage> e)
        {
            lock (_syncRoot)
            {
                _models.Add(e.Message.VoxelModel.Hash, new VisualVoxelModel(e.Message.VoxelModel,_voxelMeshFactory));
                _pengingModels.Remove(e.Message.VoxelModel.Hash);
            }

            OnVoxelModelReceived(new VoxelModelReceivedEventArgs { Model = e.Message.VoxelModel });

            _storage.Save(e.Message.VoxelModel);
        }

        /// <summary>
        /// Request a missing model from the server
        /// </summary>
        /// <param name="hash"></param>
        public void RequestModel(Md5Hash hash)
        {
            bool requested;
            lock (_syncRoot)
            {
                if (_models.ContainsKey(hash))
                    return;
                requested = _pengingModels.Add(hash);
            }

            if(requested)
                _server.ServerConnection.SendAsync(new GetVoxelModelsMessage { Md5Hashes = new [] { hash } });
        }

        /// <summary>
        /// Gets a model from manager, if the model not found requests it from the server
        /// </summary>
        /// <param name="hash">md5 hash of the model</param>
        /// <param name="requestIfMissing"></param>
        /// <returns></returns>
        public VisualVoxelModel GetModel(Md5Hash hash, bool requestIfMissing = true)
        {
            lock (_syncRoot)
            {
                VisualVoxelModel model;
                if (_models.TryGetValue(hash, out model))
                    return model;
            }

            if (requestIfMissing)
                RequestModel(hash);

            return null;
        }

        /// <summary>
        /// Adds a new voxel model and saves it
        /// </summary>
        /// <param name="model"></param>
        public void SaveModel(VisualVoxelModel model)
        {
            model.VoxelModel.UpdateHash();

            lock (_syncRoot)
                _models.Add(model.VoxelModel.Hash, model);

            _storage.Save(model.VoxelModel);
        }

        /// <summary>
        /// Removes a voxel model by its hash
        /// </summary>
        /// <param name="hash"></param>
        public void DeleteModel(Md5Hash hash)
        {
            lock (_syncRoot)
            {
                _models.Remove(hash);
            }

            _storage.Delete(hash);
        }

        public IEnumerable<VisualVoxelModel> Enumerate()
        {
            lock (_syncRoot)
            {
                foreach (var pair in _models)
                {
                    yield return pair.Value;
                }
            }
        }

        public override void Initialize()
        {
            // load all models
            lock (_syncRoot)
            {
                foreach (var voxelModel in _storage.Enumerate())
                {
                    _models.Add(voxelModel.Hash, new VisualVoxelModel(voxelModel, _voxelMeshFactory));
                }
            }
        }
    }

    public class VoxelModelReceivedEventArgs : EventArgs
    {
        public VoxelModel Model { get; set; }
    }
}
