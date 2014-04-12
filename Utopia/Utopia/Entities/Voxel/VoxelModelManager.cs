using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Ninject;
using Utopia.Shared.Entities.Models;
using Utopia.Shared.Interfaces;
using S33M3DXEngine.Main;

namespace Utopia.Entities.Voxel
{
    /// <summary>
    /// Holds all voxel models. Can request a model from the server
    /// </summary>
    public class VoxelModelManager : GameComponent
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        private bool _initialized;
        private readonly object _syncRoot = new object();
        private readonly Dictionary<string, VisualVoxelModel> _models = new Dictionary<string, VisualVoxelModel>();
        private HashSet<string> _pendingModels = new HashSet<string>();
        private readonly ConcurrentQueue<VoxelModel> _receivedModels = new ConcurrentQueue<VoxelModel>();

        /// <summary>
        /// Occurs when a voxel model is available (received from the server, loaded from the storage)
        /// </summary>
        public event EventHandler<VoxelModelReceivedEventArgs> VoxelModelAvailable;

        private void OnVoxelModelAvailable(VoxelModelReceivedEventArgs e)
        {
            var handler = VoxelModelAvailable;
            if (handler != null) handler(this, e);
        }

        [Inject]
        public IVoxelModelStorage VoxelModelStorage { get; set; }

        [Inject]
        public VoxelMeshFactory VoxelMeshFactory { get; set; }

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
            if (requested && _initialized)
            {
                logger.Info("Downloading model: {0}", name);
                var req = WebRequest.Create(string.Format("http://utopiarealms.com/models/{0}/download", Uri.EscapeDataString(name)));
                req.BeginGetResponse(ModelReceived, req);
            }
            
        }

        private void ModelReceived(IAsyncResult ar)
        {
            var req = (HttpWebRequest)ar.AsyncState;

            try
            {
                var response = req.EndGetResponse(ar);

                using (var stream = response.GetResponseStream())
                {
                    var voxelModel = VoxelModel.LoadFromStream(stream);

                    lock (_syncRoot)
                    {
                        if (_models.ContainsKey(voxelModel.Name))
                            _models.Remove(voxelModel.Name);

                        _models.Add(voxelModel.Name, new VisualVoxelModel(voxelModel, VoxelMeshFactory));
                        _pendingModels.Remove(voxelModel.Name);
                    }

                    _receivedModels.Enqueue(voxelModel);
                    VoxelModelStorage.Save(voxelModel);
                }

                response.Close();
            }
            catch (Exception x)
            {
                logger.Error("Unable to download the model: {0}", x.Message);
            }
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
                    VoxelModelStorage.Delete(model.VoxelModel.Name);
                }
                _models.Add(model.VoxelModel.Name, model);
                VoxelModelStorage.Save(model.VoxelModel);
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

                VoxelModelStorage.Delete(model.VoxelModel.Name);
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
                foreach (var voxelModel in VoxelModelStorage.Enumerate())
                {
                    var vmodel = new VisualVoxelModel(voxelModel, VoxelMeshFactory);
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

        public override void FTSUpdate(GameTime timeSpent)
        {
            if (_receivedModels.Count > 0)
            {
                VoxelModel model;
                while (_receivedModels.TryDequeue(out model))
                {
                    OnVoxelModelAvailable(new VoxelModelReceivedEventArgs { Model = model });
                }
            }

            base.FTSUpdate(timeSpent);
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
                VoxelModelStorage.Delete(oldName);
                VoxelModelStorage.Save(model.VoxelModel);
                _models.Remove(oldName);
                _models.Add(newName, model);
            }
        }

        public VisualVoxelModel GetModelByHash(string hash)
        {
            lock (_syncRoot)
            {
                var model = _models.Select(p => p.Value).FirstOrDefault(m => m.VoxelModel.Hash.ToString() == hash);

                if (model == null)
                {
                    RequestModel(hash);
                }

                return model;
            }
        }
    }

    public class VoxelModelReceivedEventArgs : EventArgs
    {
        public VoxelModel Model { get; set; }
    }
}
