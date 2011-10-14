using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Utopia.Shared.Chunks.Entities.Inventory;
using Utopia.Shared.Chunks.Entities.Inventory.Tools;
using Utopia.Shared.Structs;

namespace Utopia.Tests
{
    public class Shovel : BlockRemover
    {
        public override Shared.Chunks.Entities.Interfaces.IToolImpact Use(bool runOnServer = false)
        {
            throw new NotImplementedException();
        }

        public override void Rollback(Shared.Chunks.Entities.Interfaces.IToolImpact impact)
        {
            throw new NotImplementedException();
        }

        public override int MaxStackSize
        {
            get { return 1; }
        }

        public override ushort ClassId
        {
            get { throw new NotImplementedException(); }
        }
    }

    public class GoldCoin : SpriteItem
    {
        public GoldCoin()
        {
            MaxStackSize = 1000;
        }

        public override ushort ClassId
        {
            get { throw new NotImplementedException(); }
        }

        public override string DisplayName
        {
            get { throw new NotImplementedException(); }
        }
    }

    [TestClass]
    public class InventoryTest
    {
        [TestMethod]
        public void InventoryContainerTest()
        {
            var inventory = new SlotContainer<ContainedSlot>(new Vector2I(5, 8));

            // first lets put there something

            Assert.IsTrue(inventory.PutItem(new Shovel())); // puts item on first available slot (0,0)

            // ok, now try to use PutItem with slot parameter, this allow to put on desired cell

            // creating new slot
            var slot = new ContainedSlot
                           {
                               // desired position
                               GridPosition = new Vector2I(0, 0),
                               ItemsCount = 1,
                               Item = new Shovel()
                           };

            // unable to put another unstackable shovel to the same slot (occupied by the first Shovel)
            Assert.IsFalse(inventory.PutItem(slot));

            slot.GridPosition = new Vector2I(0, 1);

            // no problem to put item on another empty slot
            Assert.IsTrue(inventory.PutItem(slot));

            // check persistance
            Assert.AreEqual(2, inventory.Count());

            var slotTaken = inventory.PeekSlot(new Vector2I(0, 0));
            Assert.IsTrue(slotTaken.Item is Shovel);
            Assert.AreEqual(1, slotTaken.ItemsCount);

            slotTaken = inventory.PeekSlot(new Vector2I(0, 1));
            Assert.IsTrue(slotTaken.Item is Shovel);
            Assert.AreEqual(1, slotTaken.ItemsCount);

            // now lets play with stacks

            // add 900 gold coins
            inventory.PutItem(new ContainedSlot
                                  {
                                      Item = new GoldCoin(),
                                      GridPosition = new Vector2I(1, 0),
                                      ItemsCount = 900
                                  });

            // check it
            slotTaken = inventory.PeekSlot(new Vector2I(1, 0));
            Assert.IsTrue(slotTaken.Item is GoldCoin);
            Assert.AreEqual(900, slotTaken.ItemsCount);

            // cannot take 500 shovels, we have only 1
            slotTaken = inventory.TakeSlot(new ContainedSlot
                                               {
                                                   GridPosition = new Vector2I(0, 0),
                                                   ItemsCount = 500
                                               });
            Assert.AreEqual(null, slotTaken);

            // ok to take 500 gold coins (400 will left)
            slotTaken = inventory.TakeSlot(new ContainedSlot
                                               {
                                                   GridPosition = new Vector2I(1, 0),
                                                   ItemsCount = 500
                                               });

            Assert.AreEqual(500, slotTaken.ItemsCount);
            Assert.IsTrue(slotTaken.Item is GoldCoin);

            // check
            slotTaken = inventory.PeekSlot(new Vector2I(1, 0));
            Assert.AreEqual(400, slotTaken.ItemsCount);
            Assert.IsTrue(slotTaken.Item is GoldCoin);
        }

        [TestMethod]
        public void ExchangeTest()
        {
            #region init

            var inventory = new SlotContainer<ContainedSlot>(new Vector2I(5, 8));

            inventory.PutItem(new Shovel()); // puts item on first available slot (0,0)

            ContainedSlot slot = new ContainedSlot
                                     {
                                         // desired position
                                         GridPosition = new Vector2I(1, 1),
                                         ItemsCount = 2,
                                         Item = new GoldCoin()
                                     };

            inventory.PutItem(slot);

            #endregion

            var from = new Vector2I(0, 0);                
            var to = new Vector2I(1, 1);                
            

            //initial state
            Assert.IsTrue(inventory.PeekSlot(from).Item is Shovel);
            Assert.IsTrue(inventory.PeekSlot(to).Item is GoldCoin);
            
            ContainedSlot hand = inventory.TakeSlot(new ContainedSlot(from,1));
            //got the shovel in my hand
            Assert.IsNull(inventory.PeekSlot(new Vector2I(0, 0)));
            
            //move my hand to the destination position
            hand.GridPosition = to;
            
            //put down the shovel on 1,1 and get the goldcoin
            ContainedSlot result;
            inventory.PutItemExchange(hand, out result);

            Assert.IsTrue(result.Item is GoldCoin);
            Assert.IsTrue(inventory.PeekSlot(new Vector2I(1, 1)).Item is Shovel);

            hand = result; //now i have the goldCoin in my hand
            
            //move back my hand to the original position
            hand.GridPosition = from;
            //empty my hand, put the goldcoin
            inventory.PutItem(hand);

            //final state :yay I switched two items ! 
            Assert.IsTrue(inventory.PeekSlot(from).Item is GoldCoin);
            Assert.AreEqual(2,inventory.PeekSlot(from).ItemsCount);

            Assert.IsTrue(inventory.PeekSlot(to).Item is Shovel);
            
            
        }
    }
}