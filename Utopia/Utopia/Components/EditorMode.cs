namespace Utopia.Components
{
    public enum EditorMode
    {
        /// <summary>
        /// Represents a simple model view
        /// </summary>
        MainView,
        /// <summary>
        /// Shows whole model and allows to change layout of the parts
        /// </summary>
        ModelLayout,
        /// <summary>
        /// Shows only one frame of the part and allows voxel edition
        /// </summary>
        FrameEdit
    }
}