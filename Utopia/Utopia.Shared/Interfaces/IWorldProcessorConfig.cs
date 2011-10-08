using System;

namespace Utopia.Shared.Interfaces
{
    //Wrapper arround a specific set of IWorldProcessor : Will be use for IoC container
    public interface IWorldProcessorConfig : IDisposable
    {
        IWorldProcessor[] WorldProcessors { get; }
    }
}
