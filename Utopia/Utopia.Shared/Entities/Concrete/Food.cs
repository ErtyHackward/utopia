using System.ComponentModel;
using S33M3Resources.Structs;
using Utopia.Shared.Entities.Inventory;

namespace Utopia.Shared.Entities.Concrete
{
    public class Food : Item
    {
        /// <summary>
        /// Amount of energy provide when used
        /// </summary>
        [Category("Food")]
        public int Calories { get; set; }

        public override ushort ClassId
        {
            get { return EntityClassId.Food; }
        }

        public override EntityPosition GetPosition(Interfaces.IDynamicEntity owner)
        {
            var pos = new EntityPosition();

            // allow to put only on top of the entity
            if (owner.EntityState.PickPointNormal.Y != 1)
                return pos;

            pos.Position = new Vector3D(owner.EntityState.PickPoint);
            pos.Valid = true;

            return pos;
        }
    }
}
