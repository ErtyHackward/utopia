using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utopia.Shared.Configuration;
using Utopia.Shared.Entities;
using Utopia.Shared.Entities.Models;

namespace Utopia.Tests
{
    [TestClass]
    public class VoxelModelCreation
    {
        [TestMethod]
        public void CreateModel()
        {
            EntityFactory.InitializeProtobufInheritanceHierarchy();
            var conf =
                WorldConfiguration.LoadFromFile(@"C:\Dev\Utopia\Utopia\Resources\Shared.Resources\Config\Island.realm");

            try
            {
                var model = VoxelModel.GenerateTreeModel(conf.TreeBluePrints[0]);

                model.SaveToFile(@"C:\Dev\Utopia\Setup\Output\Tree.uvm");
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + e.StackTrace);
            }

        }

    }
}
