using System.ComponentModel;
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
    }
}
