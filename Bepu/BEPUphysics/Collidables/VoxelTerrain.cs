using System;
using BEPUphysics.Collidables.Events;
using BEPUphysics.CollisionShapes;
using BEPUphysics.MathExtensions;

using BEPUphysics.DataStructures;
using BEPUphysics.Materials;
using BEPUphysics.CollisionRuleManagement;
using BEPUphysics.CollisionShapes.ConvexShapes;
using BEPUphysics.ResourceManagement;
using BEPUphysics.CollisionTests.CollisionAlgorithms;

namespace BEPUphysics.Collidables
{
    ///<summary>
    /// Unmoving, collidable voxel terrain
    ///</summary>
    ///<remarks>
    /// The acceleration structure for the mesh is created individually for each
    /// StaticMesh; if you want to create many meshes of the same model, consider using the
    /// InstancedMesh.
    /// </remarks>
    public class VoxelTerrain : Collidable, ISpaceObject, IMaterialOwner
    {

        ///<summary>
        /// Gets or sets the world transform of the mesh.
        ///</summary>
        public AffineTransform WorldTransform { get; set; }

        ///<summary>
        /// Constructs a new static mesh.
        ///</summary>
        public VoxelTerrain(byte[,,] terrain)
        {
            base.Shape = new VoxelTerrainShape() { Voxels = terrain };

            collisionRules.group = CollisionRules.DefaultKinematicCollisionGroup;
            events = new ContactEventManager<VoxelTerrain>(this);

            material = new Material();
            materialChangedDelegate = OnMaterialChanged;
            material.MaterialChanged += materialChangedDelegate;
        }

        ///<summary>
        /// Gets the shape used by the mesh.
        ///</summary>
        public new VoxelTerrainShape Shape
        {
            get
            {
                return (VoxelTerrainShape)shape;
            }
        }

        protected internal ContactEventManager<VoxelTerrain> events;

        ///<summary>
        /// Gets the event manager used by the mesh.
        ///</summary>
        public ContactEventManager<VoxelTerrain> Events
        {
            get
            {
                return events;
            }
        }
        protected internal override IContactEventTriggerer EventTriggerer
        {
            get { return events; }
        }

        internal Material material;
        //NOT thread safe due to material change pair update.
        ///<summary>
        /// Gets or sets the material used by the mesh.
        ///</summary>
        public Material Material
        {
            get
            {
                return material;
            }
            set
            {
                if (material != null)
                    material.MaterialChanged -= materialChangedDelegate;
                material = value;
                if (material != null)
                    material.MaterialChanged += materialChangedDelegate;
                OnMaterialChanged(material);
            }
        }

        protected override void OnShapeChanged(CollisionShape collisionShape)
        {
            UpdateBoundingBox();
        }

        /// <summary>
        /// Updates the bounding box to the current state of the entry.
        /// </summary>
        public override void UpdateBoundingBox()
        {
            boundingBox = new BoundingBox(new Vector3(), new Vector3(256, 128, 256));
        }

        Action<Material> materialChangedDelegate;
        void OnMaterialChanged(Material newMaterial)
        {
            for (int i = 0; i < pairs.Count; i++)
            {
                pairs[i].UpdateMaterialProperties();
            }
        }

        protected internal override bool IsActive
        {
            get { return false; }
        }

        /// <summary>
        /// Tests a ray against the entry.
        /// </summary>
        /// <param name="ray">Ray to test.</param>
        /// <param name="maximumLength">Maximum length, in units of the ray's direction's length, to test.</param>
        /// <param name="rayHit">Hit location of the ray on the entry, if any.</param>
        /// <returns>Whether or not the ray hit the entry.</returns>
        public override bool RayCast(Ray ray, float maximumLength, out RayHit rayHit)
        {
            rayHit = new RayHit();
            var startPoint = ray.Position;
            var step = new Vector3(ray.Direction.X * 0.1f, ray.Direction.Y * 0.1f, ray.Direction.Z * 0.1f);
            for (float i = 0; i < maximumLength; i+= 0.1f)
            {
                startPoint += step;
                if (Shape.Voxels[(int)startPoint.X, (int)startPoint.Y, (int)startPoint.Z] != 0)
                {
                    rayHit.Location = startPoint;
                    rayHit.T = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Casts a convex shape against the collidable.
        /// </summary>
        /// <param name="castShape">Shape to cast.</param>
        /// <param name="startingTransform">Initial transform of the shape.</param>
        /// <param name="sweep">Sweep to apply to the shape.</param>
        /// <param name="hit">Hit data, if any.</param>
        /// <returns>Whether or not the cast hit anything.</returns>
        public override bool ConvexCast(ConvexShape castShape, ref RigidTransform startingTransform, ref Vector3 sweep, out RayHit hit)
        {
            hit = new RayHit();
            BoundingBox boundingBox;
            Toolbox.GetExpandedBoundingBox(ref castShape, ref startingTransform, ref sweep, out boundingBox);
            var tri = Resources.GetTriangle();
            var hitElements = Resources.GetIntList();
            if (Mesh.Tree.GetOverlaps(boundingBox, hitElements))
            {
                hit.T = float.MaxValue;
                for (int i = 0; i < hitElements.Count; i++)
                {
                    mesh.Data.GetTriangle(hitElements[i], out tri.vA, out tri.vB, out tri.vC);
                    Vector3 center;
                    Vector3.Add(ref tri.vA, ref tri.vB, out center);
                    Vector3.Add(ref center, ref tri.vC, out center);
                    Vector3.Multiply(ref center, 1f / 3f, out center);
                    Vector3.Subtract(ref tri.vA, ref center, out tri.vA);
                    Vector3.Subtract(ref tri.vB, ref center, out tri.vB);
                    Vector3.Subtract(ref tri.vC, ref center, out tri.vC);
                    tri.maximumRadius = tri.vA.LengthSquared();
                    float radius = tri.vB.LengthSquared();
                    if (tri.maximumRadius < radius)
                        tri.maximumRadius = radius;
                    radius = tri.vC.LengthSquared();
                    if (tri.maximumRadius < radius)
                        tri.maximumRadius = radius;
                    tri.maximumRadius = (float)Math.Sqrt(tri.maximumRadius);
                    tri.collisionMargin = 0;
                    var triangleTransform = new RigidTransform();
                    triangleTransform.Orientation = Quaternion.Identity;
                    triangleTransform.Position = center;
                    RayHit tempHit;
                    if (MPRToolbox.Sweep(castShape, tri, ref sweep, ref Toolbox.ZeroVector, ref startingTransform, ref triangleTransform, out tempHit) && tempHit.T < hit.T)
                    {
                        hit = tempHit;
                    }
                }
                tri.maximumRadius = 0;
                Resources.GiveBack(tri);
                Resources.GiveBack(hitElements);
                return hit.T != float.MaxValue;
            }
            Resources.GiveBack(tri);
            Resources.GiveBack(hitElements);
            return false;
        }

        ISpace space;
        ISpace ISpaceObject.Space
        {
            get
            {
                return space;
            }
            set
            {
                space = value;
            }
        }
        ///<summary>
        /// Gets the space that owns the mesh.
        ///</summary>
        public ISpace Space
        {
            get
            {
                return space;
            }
        }

        void ISpaceObject.OnAdditionToSpace(ISpace newSpace)
        {
        }

        void ISpaceObject.OnRemovalFromSpace(ISpace oldSpace)
        {
        }
    }
}
