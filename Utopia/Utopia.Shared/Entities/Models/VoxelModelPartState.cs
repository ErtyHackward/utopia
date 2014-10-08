using System.IO;
using ProtoBuf;
using SharpDX;
using Utopia.Shared.Tools.BinarySerializer;

namespace Utopia.Shared.Entities.Models
{
    /// <summary>
    /// Contains a layout of a single part of the model and a current active frame
    /// </summary>
    [ProtoContract]
    public class VoxelModelPartState : IBinaryStorable
    {
        private Vector3 _scale;
        private Quaternion _rotation;
        private Vector3 _rotationOffset;
        private Vector3 _translation;

        // cached transformation
        private Matrix? _transform;

        /// <summary>
        /// Current active frame, set byte.MaxValue to hide
        /// </summary>
        [ProtoMember(1)]
        public byte ActiveFrame;
        
        /// <summary>
        /// Frame scale
        /// </summary>
        [ProtoMember(2)]
        public Vector3 Scale
        {
            get { return _scale; }
            set
            {
                _scale = value;
                _transform = null;
            }
        }
        
        /// <summary>
        /// Frame rotation
        /// </summary>
        [ProtoMember(3)]
        public Quaternion Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _transform = null;
            }
        }
        
        /// <summary>
        /// Frame rotation offset, Its the location from where the rotation is applied
        /// </summary>
        [ProtoMember(4)]
        public Vector3 RotationOffset
        {
            get { return _rotationOffset; }
            set
            {
                _rotationOffset = value;
                _transform = null;
            }
        }
        
        /// <summary>
        /// Frame translation
        /// </summary>
        [ProtoMember(5)]
        public Vector3 Translation
        {
            get { return _translation; }
            set {
                _translation = value;
                _transform = null;
            }
        }

        /// <summary>
        /// Optional palm tranformation. Specifies the location of the tool equipped. Only for arm
        /// </summary>
        [ProtoMember(6)]
        public Matrix? PalmTransform;

        /// <summary>
        /// Optional particules for the state
        /// </summary>
        [ProtoMember(7)]
        public StaticEntityParticule Particlules { get; set; }

        /// <summary>
        /// Current part bounding box
        /// </summary>
        public BoundingBox BoundingBox;
        
        public Matrix GetTransformation()
        {
            return _transform.HasValue ? _transform.Value : (_transform = Matrix.Scaling(Scale) * Matrix.Translation(-RotationOffset) * Matrix.RotationQuaternion(Rotation) * Matrix.Translation(RotationOffset) * Matrix.Translation(Translation)).Value;
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
            
            var havePalmTransform = reader.ReadBoolean();
            if (havePalmTransform)
            {
                PalmTransform = reader.ReadMatrix();
            }

        }

        /// <summary>
        /// Calculates values for this state as interpolated between states passed
        /// </summary>
        /// <param name="psFrom"></param>
        /// <param name="psTo"></param>
        /// <param name="step"></param>
        public void Interpolation(VoxelModelPartState psFrom, VoxelModelPartState psTo, float step)
        {
            Vector3.Lerp(ref psFrom._translation, ref psTo._translation, step, out _translation);
            Vector3.Lerp(ref psFrom._rotationOffset, ref psTo._rotationOffset, step, out _rotationOffset);
            Vector3.Lerp(ref psFrom._scale, ref psTo._scale, step, out _scale);
            Quaternion.Slerp(ref psFrom._rotation, ref psTo._rotation, step, out _rotation);
            _transform = null;
        }
    }
}