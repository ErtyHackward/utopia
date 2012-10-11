using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.World.Processors.Utopia.Biomes
{
    public partial class BiomeTrees
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        #region Public Methods
        public void Save(System.IO.BinaryWriter writer)
        {
            writer.Write(_small);
            writer.Write(_medium);
            writer.Write(_big);
            writer.Write(_cactus);
            writer.Write(TreePerChunks.Min);
            writer.Write(TreePerChunks.Max);
        }

        public void Load(System.IO.BinaryReader reader)
        {
            _small = reader.ReadInt32();
            _medium = reader.ReadInt32();
            _big = reader.ReadInt32();
            _cactus = reader.ReadInt32();
            TreePerChunks = new RangeI(reader.ReadInt32(), reader.ReadInt32());
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
