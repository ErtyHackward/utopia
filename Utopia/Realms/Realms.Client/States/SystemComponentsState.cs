using System.Collections.Generic;
using System.Linq;
using Realms.Client.Components.GUI;
using S33M3CoreComponents.States;
using Ninject;
using S33M3CoreComponents.GUI;
using S33M3CoreComponents.Inputs;
using S33M3CoreComponents.Debug;
using Ninject.Parameters;
using S33M3DXEngine;
using Utopia.Entities.Voxel;
using Utopia.Shared.Settings;
using Utopia.Shared.Interfaces;
using Utopia.Entities;
using System.IO;
using S33M3CoreComponents.Config;
using Utopia.Components;
using Utopia.Sounds;
using Utopia.Shared.GraphicManagers;

namespace Realms.Client.States
{
    public class SystemComponentsState : GameState
    {
        #region Private variables
        private IKernel _iocContainer;
        #endregion

        #region Public variables/Properties
        public override string Name
        {
            get { return "SystemComponents"; }
        }
        #endregion

        public SystemComponentsState(GameStatesManager stateManager, IKernel iocContainer)
            :base(stateManager)
        {
            _iocContainer = iocContainer;
            AllowMouseCaptureChange = false;
        }

        #region Public methods
        public override void Initialize(SharpDX.Direct3D11.DeviceContext context)
        {
            var guiManager = _iocContainer.Get<GuiManager>();
            var inputManager = _iocContainer.Get<InputsManager>();
            var generalSoundManager = _iocContainer.Get<GeneralSoundManager>();
            var watermark = _iocContainer.Get<VersionWatermark>();

            DebugComponent debugComponent = null;
            if (Program.ShowDebug) debugComponent = _iocContainer.Get<DebugComponent>(new ConstructorArgument("withDisplayInfoActivated", true));

            //Init Common GUI Menu resources
            var commonResources = _iocContainer.Get<SandboxCommonResources>();
            commonResources.LoadFontAndMenuImages(_iocContainer.Get<D3DEngine>());

            //Init MSAA list
            InitMSAASystemList();

            //Init RuntimeVariables
            var vars = _iocContainer.Get<RealmRuntimeVariables>();
            vars.ApplicationDataPath = XmlSettingsManager.GetFilePath("", SettingsStorage.ApplicationData);

            //"Late Binding" of IVoxelModelStorage, must be done after vars is initialized
            _iocContainer.Bind<IVoxelModelStorage>().To<ModelSQLiteStorage>().InSingletonScope().WithConstructorArgument("fileName", Path.Combine(vars.ApplicationDataPath, "Common", "models.db"));

            var storage = (ModelSQLiteStorage)_iocContainer.Get<IVoxelModelStorage>();
            storage.ImportFromPath("Models");

            AddComponent(watermark);
            AddComponent(debugComponent);
            AddComponent(guiManager);
            AddComponent(inputManager);
            AddComponent(generalSoundManager);
            base.Initialize(context);
        }

        public override void OnEnabled(GameState previousState)
        {
            //Activate the mouse Pooling
            _iocContainer.Get<InputsManager>().MouseManager.IsRunning = true;
        }
        #endregion

        #region Private methods
        private void InitMSAASystemList()
        {
            //Insert the Engine MSAA mode list in the ClientSettings, it needs the Engine created object
            List<SampleDescriptionSetting> sortedCollection = new List<SampleDescriptionSetting>();
            List<object> sampleCollection = new List<object>();
            foreach (var item in D3DEngine.MSAAList)
            {
                sortedCollection.Add(new SampleDescriptionSetting() { SampleDescription = item });
            }
            sampleCollection.AddRange(sortedCollection.OrderBy(x => x.QualityWeight));
            ClientSettings.DynamicLists.Add("CLIST_MSAA", sampleCollection);
        }
        #endregion
    }
}
