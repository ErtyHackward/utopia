﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Network;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Dynamic;
using Utopia.Shared.World;
using Utopia.Entities;
using Utopia.Worlds.GameClocks;
using Utopia.GUI;
using Utopia.Editor;
using Utopia.Worlds.SkyDomes;
using Utopia.Worlds.Weather;
using Utopia.Worlds.Storage;
using Utopia.Worlds.Cubes;
using Utopia.Worlds.Chunks.ChunkLandscape;
using Utopia.Shared.Chunks;
using Utopia.Worlds.Chunks.ChunkLighting;
using Utopia.Worlds.Chunks.ChunkMesh;
using Utopia.Worlds.Chunks;
using Utopia.Worlds.Chunks.ChunkWrapper;
using Utopia.Shared.Interfaces;
using Utopia.Entities.Renderer.Interfaces;
using Utopia.Worlds.Chunks.ChunkEntityImpacts;
using Utopia.Entities.Managers.Interfaces;
using Utopia.Entities.Managers;
using Utopia.Entities.Voxel;
using Utopia.Effects.Shared;
using S33M3_DXEngine;
using S33M3_CoreComponents.WorldFocus;
using S33M3_CoreComponents.States;
using S33M3_CoreComponents.Cameras.Interfaces;
using S33M3_CoreComponents.Timers;
using S33M3_CoreComponents.Inputs;
using S33M3_CoreComponents.Inputs.Actions;
using S33M3_CoreComponents.GUI;
using System.Windows.Forms;
using S33M3_DXEngine.Main.Interfaces;
using S33M3_CoreComponents.Cameras;
using Utopia.GUI.Inventory;
using Utopia.GUI.Map;

namespace Utopia
{
    public class UtopiaRenderStates
    {
        public D3DEngine engine;
        public ServerComponent server;
        public WorldFocusManager worldFocusManager;
        public WorldParameters worldParameters;
        public VisualWorldParameters visualWorldParameters;
        public GameStatesManager gameStatesManager;
        public ICamera firstPersonCamera;
        public CameraManager<ICameraFocused> cameraManager;
        public TimerManager timerManager;
        public EntityMessageTranslator entityMessageTranslator;
        public ItemMessageTranslator itemMessageTranslator;
        public InputsManager inputsManager;
        public ActionsManager actionsManager;
        public GuiManager guiManager;
        public Screen screen;
        public IconFactory iconFactory;
        public IClock gameClock;
        public InventoryComponent inventoryComponent;
        public ChatComponent chatComponent;
        public MapComponent mapComponent;
        public Hud hud;
        //public EntityEditor entityEditor;
        //public CarvingEditor carvingEditor;
        public IDrawableComponent stars;
        public ISkyDome skydome;
        public IWeather weather;
        public IDrawableComponent clouds;
        public IChunkStorageManager chunkStorageManager;
        public ICubeMeshFactory solidCubeMeshFactory;
        public ICubeMeshFactory liquidCubeMeshFactory;
        public SingleArrayChunkContainer singleArrayChunkContainer;
        public ILandscapeManager landscapeManager;
        public ILightingManager lightingManager;
        public IChunkMeshManager chunkMeshManager;
        public IWorldChunks worldChunks;
        public IChunksWrapper chunksWrapper;
        public WorldGenerator worldGenerator;
        public IWorldProcessorConfig worldProcessorConfig;
        public IPickingRenderer pickingRenderer;
        public IChunkEntityImpactManager chunkEntityImpactManager;
        public IEntityPickingManager entityPickingManager;
        public IDynamicEntityManager dynamicEntityManager;
        public PlayerEntityManager playerEntityManager;
        public PlayerCharacter playerCharacter;
        public IEntitiesRenderer playerEntityRenderer;
        public IEntitiesRenderer defaultEntityRenderer;
        public VoxelMeshFactory voxelMeshFactory;
        public SharedFrameCB sharedFrameCB;
    }
}
