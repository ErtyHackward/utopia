using ProtoBuf;

namespace Utopia.Shared.Entities.Dynamic
{
    /// <summary>
    /// Base class for tool-specific data containers
    /// We need to use this class to support data serialization of custom states
    /// </summary>
    [ProtoContract]
    [ProtoInclude(100, typeof(GodHandToolState))]
    public class ToolState
    {

    }
}