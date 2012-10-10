using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class BiomeTrees : IBinaryStorable
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public void Save(System.IO.BinaryWriter writer)
        {
            throw new NotImplementedException();
        }

        public void Load(System.IO.BinaryReader reader)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Private Methods
        #endregion


        //Property Grid editing Purpose
        internal class BiomeTreesTypeConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Tree distributions");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(BiomeTrees), attributes).Sort(new string[] { "Small", "Medium", "Big", "Cactus" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
