using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Utopia.Planets.Terran.World;

namespace Liquid.plugin
{
    public enum WaterPoolState
    {
        None,
        WithoutTimer
    }

    public enum WaterPoolProcessState
    {
        QueueProcessINIT
    }

    public class WaterPool
    {
        public List<SurroundingIndex> WaterSources = new List<SurroundingIndex>();
        public Queue<FloodingData> FloodData;
        public WaterPoolState GlobalState;
        public WaterPoolProcessState ProcessState;
        public string ID;
        public bool DryingPool;

        public WaterPool(WaterPoolState state = WaterPoolState.None)
        {
            ID = "WaterPool created at : " + DateTime.Now.ToString();
            FloodData = new Queue<FloodingData>();
            GlobalState = state;
            ProcessState = WaterPoolProcessState.QueueProcessINIT;
            DryingPool = false;
        }

        public void SendSourcesInQueue(LandScape landscape)
        {
            for (int i = 0; i < WaterSources.Count; i++)
            {
                FloodData.Enqueue(new FloodingData() { CubeLocation = WaterSources[i].Position, FloodingPower = landscape.Cubes[WaterSources[i].Index].MetaData2 });
            }
            DryingPool = false;
            WaterSources.Clear();
        }

        public override string ToString()
        {
            return ID + " with a Queue of " + FloodData.Count + " items - Drying mode : " + DryingPool.ToString();
        }
    }
}
