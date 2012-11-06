using System;
using System.Collections.Generic;
using System.Linq;
using Utopia.Network;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using Utopia.Shared.Net.Connections;
using Utopia.Shared.Net.Messages;
using S33M3DXEngine.Main;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Holds all voxel models. Can request a model from the server
    /// </summary>
    public class VoxelModelManager : GameComponent
    {
        private bool _initialized;
        private readonly IVoxelModelStorage _storage;
        private readonly ServerComponent _server;
        private readonly VoxelMeshFactory _voxelMeshFactory;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, VisualVoxelModel> _models = new Dictionary<string, VisualVoxelModel>();
        private HashSet<string> _pendingModels = new HashSet<string>();

        /// <summary>
        /// Occurs when a voxel model is available (received from the server, loaded from the storage)
        /// </summary>
        public event EventHandler<VoxelModelReceivedEventArgs> VoxelModelAvailable;

        private void OnVoxelModelAvailable(VoxelModelReceivedEventArgs e)
        {
            var handler = VoxelModelAvailable;
            if (handler != null) handler(this, e);
        }


        public VoxelModelManager(IVoxelModelStorage storage, ServerComponent server, VoxelMeshFactory voxelMeshFactory)
        {
            if (storage == null) throw new ArgumentNullException("storage");
            _storage = storage;
            _server = server;
            _voxelMeshFactory = voxelMeshFactory;

            if (_server != null)
                _server.MessageVoxelModelData += ServerConnectionMessageVoxelModelData;
        }

        public override void BeforeDispose()
        {
            if (_server != null)
                _server.MessageVoxelModelData -= ServerConnectionMessageVoxelModelData;
        }

        void ServerConnectionMessageVoxelModelData(object sender, ProtocolMessageEventArgs<VoxelModelDataMessage> e)
        {
            lock (_syncRoot)
            {
                _models.Add(e.Message.VoxelModel.Name, new VisualVoxelModel(e.Message.VoxelModel,_voxelMeshFactory));
                _pendingModels.Remove(e.Message.VoxelModel.Name);
            }

            OnVoxelModelAvailable(new VoxelModelReceivedEventArgs { Model = e.Message.VoxelModel });

            _storage.Save(e.Message.VoxelModel);
        }

        /// <summary>
        /// Request a missing model from the server
        /// </summary>
        /// <param name="name"></param>
        public void RequestModel(string name)
        {
            bool requested;
            lock (_syncRoot)
            {
                if (_models.ContainsKey(name))
                    return;
                requested = _pendingModels.Add(name);
            }

            // don't request the model before we are initialized or already requested
            if (requested && _initialized && _server != null)
                _server.ServerConnection.SendAsync(new GetVoxelModelsMessage { Names = new [] { name } });
        }

        /// <summary>
        /// Gets a model from manager, if the model not found requests it from the server
        /// </summary>
        /// <param name="name">name of the model</param>
        /// <param name="requestIfMissing"></param>
        /// <returns></returns>
        public VisualVoxelModel GetModel(string name, bool requestIfMissing = true)
        {
            lock (_syncRoot)
            {
                VisualVoxelModel model;
                if (_models.TryGetValue(name, out model))
                    return model;
            }

            if (requestIfMissing)
                RequestModel(name);

            return null;
        }

        /// <summary>
        /// Adds a new voxel model or updates it
        /// </summary>
        /// <param name="model"></param>
        public void SaveModel(VisualVoxelModel model)
        {
            model.VoxelModel.UpdateHash();

            lock (_syncRoot)
            {
                if (_models.ContainsKey(model.VoxelModel.Name))
                {
                    _models.Remove(model.VoxelModel.Name);
                    _storage.Delete(model.VoxelModel.Name);
                }
                _models.Add(model.VoxelModel.Name, model);
                _storage.Save(model.VoxelModel);
            }
        }

        /// <summary>
        /// Removes a voxel model by its name
        /// </summary>
        /// <param name="name"></param>
        public void DeleteModel(string name)
        {
            VisualVoxelModel model;
            if (_models.TryGetValue(name, out model))
            {
                lock (_syncRoot)
                {
                    _models.Remove(name);
                }

                _storage.Delete(model.VoxelModel.Name);
            }
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
                    var vmodel = new VisualVoxelModel(voxelModel, _voxelMeshFactory);
                    vmodel.BuildMesh(); //Build the mesh of all local models
                    _models.Add(voxelModel.Name, vmodel);
                }
            }
            _initialized = true;
            
            // if we have some requested models, remove those that was loaded
            List<string> loadedModels;
            lock (_syncRoot)
            {
                loadedModels = _pendingModels.Where(Contains).ToList();
                loadedModels.ForEach(m => _pendingModels.Remove(m));
            }

            // fire events
            foreach (var loadedModel in loadedModels)
            {
                var model = GetModel(loadedModel, false);
                OnVoxelModelAvailable(new VoxelModelReceivedEventArgs { Model = model.VoxelModel });
            }

            HashSet<string> requestSet;
            
            lock (_syncRoot)
            {
                _pendingModels.ExceptWith(loadedModels);
                requestSet = _pendingModels;
                _pendingModels = new HashSet<string>();
            }

            // request the rest models
            foreach (var absentModel in requestSet)
            {
                RequestModel(absentModel);
            }
        }

        public bool Contains(string name)
        {
            lock (_syncRoot)
            {
                return _models.ContainsKey(name);
            }
        }

        public void Rename(string oldName, string newName)
        {
            lock (_syncRoot)
            {
                VisualVoxelModel model;
                if (!_models.TryGetValue(oldName, out model))
                    throw new InvalidOperationException("No such model to rename");
                model.VoxelModel.Name = newName;
                _storage.Delete(oldName);
                _storage.Save(model.VoxelModel);
                _models.Remove(oldName);
                _models.Add(newName, model);
            }
        }
    }

    public class VoxelModelReceivedEventArgs : EventArgs
    {
        public VoxelModel Model { get; set; }
    }
}
