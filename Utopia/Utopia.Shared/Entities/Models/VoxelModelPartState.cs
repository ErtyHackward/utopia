using System.IO;
using SharpDX;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a layout of a single part of the model and a current active frame
    /// </summary>
    public class VoxelModelPartState : IBinaryStorable
    {
        internal Matrix? Transform;
        /// <summary>
        /// Current active frame
        /// </summary>
        public byte ActiveFrame;
        
        /// <summary>
        /// Frame scale
        /// </summary>
        public Vector3 Scale;

        /// <summary>
        /// Frame rotation
        /// </summary>
        public Quaternion Rotation;

        /// <summary>
        /// Frame rotation offset
        /// </summary>
        public Vector3 RotationOffset;

        /// <summary>
        /// Frame translation
        /// </summary>
        public Vector3 Translation;

        /// <summary>
        /// Optional palm tranformation. Specifies the location of the tool equipped. Only for arm
        /// </summary>
        public Matrix? PalmTransform;

        /// <summary>
        /// Current part bounding box
        /// </summary>
        public BoundingBox BoundingBox;

        public Matrix GetTransformation()
        {
            return Transform.HasValue ? Transform.Value : (Transform = Matrix.Scaling(Scale) * Matrix.Translation(-RotationOffset) * Matrix.RotationQuaternion(Rotation) * Matrix.Translation(RotationOffset) * Matrix.Translation(Translation)).Value;
        }

        public VoxelModelPartState()
        {
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;
            Translation = Vector3.Zero;
            RotationOffset = Vector3.Zero;
        }

        public VoxelModelPartState(VoxelModelPartState copyFrom)
        {
            ActiveFrame = copyFrom.ActiveFrame;
            Scale = copyFrom.Scale;
            Rotation = copyFrom.Rotation;
            Translation = copyFrom.Translation;
            RotationOffset = copyFrom.RotationOffset;
            BoundingBox = copyFrom.BoundingBox;
        }
        
        public void Save(BinaryWriter writer)
        {
            writer.Write(ActiveFrame);
            
            writer.Write(Scale);
            writer.Write(Rotation);
            writer.Write(Translation);
            writer.Write(RotationOffset);
            
            writer.Write(PalmTransform.HasValue);
            if (PalmTransform.HasValue)
            {
                writer.Write(PalmTransform.Value);
            }
        }
        
        public void Load(BinaryReader reader)
        {
            ActiveFrame = reader.ReadByte();

            Scale = reader.ReadVector3();
            Rotation = reader.ReadQuaternion();
            Translation = reader.ReadVector3();
            RotationOffset = reader.ReadVector3();
            

            bool havePalmTransform = reader.ReadBoolean();
            if (havePalmTransform)
            {
                PalmTransform = reader.ReadMatrix();
            }

        }
    }
}