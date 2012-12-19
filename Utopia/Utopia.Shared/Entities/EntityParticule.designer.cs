using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Utopia.Shared.Entities
{
    public partial struct EntityParticule
    {
        //Property Grid editing Purpose
        internal class EntityParticuleConverter : ExpandableObjectConverter
        {
            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {   //This method is used to shown information in the PropertyGrid.
                if (destinationType == typeof(string))
                {
                    return ("Particules Informations");
                }
                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                return TypeDescriptor.GetProperties(typeof(EntityParticule), attributes).Sort(new string[] { "ParticuleType", "ParticuleId", "EmitVelocity", "AccelerationForces", "PositionOffset", "ApplyWindForce", "Size", "SizeGrowSpeed", "Color", "ParticuleLifeTime", "EmittedParticuleRate", "EmittedParticulesAmount", "EmitVelocityRandomness", "ParticuleLifeTimeRandomness" });
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override object CreateInstance(ITypeDescriptorContext context, System.Collections.IDictionary propertyValues)
            {
                return new EntityParticule() { ParticuleType = (EntityParticuleType)propertyValues["ParticuleType"],
                                               ParticuleId = (int)propertyValues["ParticuleType"],
                                               EmitVelocity = (Vector3)propertyValues["EmitVelocity"],
                                               AccelerationForces = (Vector3)propertyValues["AccelerationForces"],
                                               PositionOffset = (Vector3)propertyValues["PositionOffset"],
                                               ApplyWindForce = (bool)propertyValues["ApplyWindForce"],
                                               Size = (Vector2)propertyValues["Size"],
                                               SizeGrowSpeed = (float)propertyValues["SizeGrowSpeed"],
                                               Color = (System.Drawing.Color)propertyValues["Color"],
                                               ParticuleLifeTime = (float)propertyValues["ParticuleLifeTime"],
                                               EmittedParticuleRate = (float)propertyValues["EmittedParticuleRate"],
                                               EmittedParticulesAmount = (int)propertyValues["EmittedParticulesAmount"],
                                               EmitVelocityRandomness = (Vector3)propertyValues["EmitVelocityRandomness"],
                                               ParticuleLifeTimeRandomness = (float)propertyValues["ParticuleLifeTimeRandomness"]
                                             };
            }

        }
    }
}
