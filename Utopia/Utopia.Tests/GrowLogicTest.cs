using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Concrete;
using Utopia.Shared.Server;
using Utopia.Shared.Server.Managers;
using Utopia.Shared.Structs;

namespace Utopia.Tests
{
    [TestClass]
    public class GrowLogicTest
    {
        [TestMethod]
        public void CheckGrowing()
        {
            var entityGrowingManager = new EntityGrowingManager(null);

            var random = new Random(1);

            var entity = new PlantGrowingEntity();
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromHours(1) });
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromHours(1) });
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromHours(1) });
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromHours(1) });
            entity.LastGrowUpdate = new UtopiaTime() + UtopiaTimeSpan.FromMinutes(1);

            var now = new UtopiaTime() + UtopiaTimeSpan.FromHours(2.5);
            entityGrowingManager.EntityGrowCheck(now, entity, null, random);
            Assert.AreEqual(2, entity.CurrentGrowLevelIndex);
            
            entity = new PlantGrowingEntity();
            entity.GrowingSeasons.Add(UtopiaTime.TimeConfiguration.Seasons[0].Name);
            entity.GrowingSeasons.Add(UtopiaTime.TimeConfiguration.Seasons[2].Name);
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromDays(10) });
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromDays(10) });
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromDays(10) });
            entity.GrowLevels.Add(new GrowLevel { GrowTime = UtopiaTimeSpan.FromDays(10) });
            entity.LastGrowUpdate = new UtopiaTime() + UtopiaTimeSpan.FromMinutes(1);

            now = new UtopiaTime() + UtopiaTimeSpan.FromMinutes(50);
            entityGrowingManager.EntityGrowCheck(now, entity, null, random);
            entity.LastGrowUpdate = now;
            Assert.AreEqual(0, entity.CurrentGrowLevelIndex);

            now = new UtopiaTime() + UtopiaTimeSpan.FromYears(1);
            entityGrowingManager.EntityGrowCheck(now, entity, null, random);
            entity.LastGrowUpdate = now;
            Assert.AreEqual(1, entity.CurrentGrowLevelIndex);

            now = new UtopiaTime() + UtopiaTimeSpan.FromYears(1) + UtopiaTimeSpan.FromMinutes(1);
            entityGrowingManager.EntityGrowCheck(now, entity, null, random);
            entity.LastGrowUpdate = now;
            Assert.AreEqual(2, entity.CurrentGrowLevelIndex);

        }
    }
}
