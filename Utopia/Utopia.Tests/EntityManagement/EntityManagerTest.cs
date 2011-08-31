using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using Utopia.Server.Managers;
using Utopia.Shared.Chunks.Entities.Management;

namespace Utopia.Tests.EntityManagement
{
    [TestClass]
    public class EntityManagerTest
    {
        EntityManager _manager = new EntityManager();

        [TestMethod]
        public void PerfomanceTest1000()
        {
            // test 1000 dynamic entities

            var r = new Random();

            var entitiesCount = 1000;

            Trace.WriteLine(string.Format("Generating {0} entities", entitiesCount));

            Stopwatch sw = Stopwatch.StartNew();

            for (uint i = 0; i < entitiesCount; i++)
            {
                var entity = new WalkingTestEntity();
                entity.EntityId = i + 1;
                while (true)
                {
                    // need some nonzero vector
                    entity.MoveVector = new Vector2(r.Next(-1, 1), r.Next(-1, 1));
                    if (entity.MoveVector.X != 0f || entity.MoveVector.Y != 0f) break; 
                }
                // random entity position
                entity.Position = new Vector3(r.Next(-MapArea.AreaSize.X, MapArea.AreaSize.X), 0, r.Next(-MapArea.AreaSize.Z, MapArea.AreaSize.Z));
                _manager.AddEntity(entity);
            }

            sw.Stop();
            Trace.WriteLine(string.Format("Generated {0} entities at {1} ms", entitiesCount, sw.ElapsedMilliseconds ));

            sw = Stopwatch.StartNew();

            var takes = 10000;

            for (int i = 0; i < takes; i++)
            {
                _manager.Update(DateTime.Now);
            }

            sw.Stop();
            Trace.WriteLine(string.Format("Modulation finished. One cycle takes {0} ms", (double)sw.ElapsedMilliseconds / takes));
#if DEBUG
            Trace.WriteLine(string.Format("Area transitions count: {0}", _manager.entityAreaChangesCount));

            // each entity should listen its area and 8 surrounding (9 total)
            foreach (WalkingTestEntity entity in _manager.EnumerateEntities())
            {
                Assert.AreEqual(entity.AreasListeningCount, 9);
            }
#endif


        }
    }
}
