using System.Collections.Generic;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World
{
    /// <summary>
    /// Performs management of stages in world generation process
    /// </summary>
    public class WorldGenerationStagesManager : IEnumerable<IWorldProcessor>
    {
        private readonly List<IWorldProcessor> _processors = new List<IWorldProcessor>();

        /// <summary>
        /// Adds stage processor to processing list
        /// </summary>
        /// <param name="processor"></param>
        public void AddStage(IWorldProcessor processor)
        {
            _processors.Add(processor);
        }

        /// <summary>
        /// Adds multiple processors to processing list
        /// </summary>
        /// <param name="processors"></param>
        public void AddStages(IEnumerable<IWorldProcessor> processors)
        {
            _processors.AddRange(processors);
        }

        /// <summary>
        /// Removes stage processor from processing list
        /// </summary>
        /// <param name="processor"></param>
        public void RemoveStage(IWorldProcessor processor)
        {
            _processors.Remove(processor);
        }

        /// <summary>
        /// Removes all stages from generation list
        /// </summary>
        public void Clear()
        {
            _processors.Clear();
        }

        /// <summary>
        /// Gets count of processing stages currently in generator
        /// </summary>
        public int Count
        {
            get { return _processors.Count; }
        }

        /// <summary>
        /// Gets processing stage by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IWorldProcessor this[int index]
        {
            get { return _processors[index]; }
        }

        /// <summary>
        /// Allows enumeration of stages in generation list
        /// </summary>
        /// <returns></returns>
        public IEnumerator<IWorldProcessor> GetEnumerator()
        {
            return _processors.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _processors.GetEnumerator();
        }
    }
}
