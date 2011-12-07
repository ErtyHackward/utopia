namespace S33M3Engines.D3D
{
    public interface IGameComponent
    {
        string Name { get; }

        string InitStep { get; }
        int InitVal { get; }
        bool IsInitialized { get; }

        void Initialize();
        void LoadContent();
        void UnloadContent();
    }
}