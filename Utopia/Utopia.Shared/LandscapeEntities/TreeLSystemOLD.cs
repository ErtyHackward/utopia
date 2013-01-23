using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities
{
    public class TreeLSystemOLD
    {
        #region Private Variables
        //Settings
        private int _iteration;
        private double _angleDegree;
        private int _maxAngleOffset = 5;
        private byte _trunkBlockId;
        private byte _foliageBlockId;

        //Rules
        private string _initialAxiom;
        private Dictionary<char, string> _ruleSet;
        private Dictionary<char, double> _probalities;
        #endregion

        #region Public Properties
        #endregion

        public TreeLSystemOLD(string initialAxiom, Dictionary<char, string> ruleSet, Dictionary<char, double> probabilities, int iteration, int angle, byte trunkBlockId, byte foliageBlockId)
        {
            _angleDegree = angle;
            _iteration = iteration;
            _initialAxiom = initialAxiom;
            _ruleSet = ruleSet;
            _probalities = probabilities;
            _trunkBlockId = trunkBlockId;
            _foliageBlockId = foliageBlockId;
        }

        #region Public Methods
        public IEnumerable<BlockWithPosition> Generate(FastRandom rnd, Vector3I rootPosition)
        {
            List<BlockWithPosition> mesh = new List<BlockWithPosition>();

            string axiom = _initialAxiom;
            Stack<Vector3> _stackPosition = new Stack<Vector3>();
            Stack<Matrix> _stackOrientation = new Stack<Matrix>();

            for (int i = 0; i < _iteration; i++)
            {
                string temp = "";
                foreach (var c in axiom)
                {
                    double rValue = rnd.NextDouble(); //Value between [0;1]
                    if (_ruleSet.ContainsKey(c) && _probalities[c] < rValue)
                    {
                        temp += _ruleSet[c];
                    }
                    else
                    {
                        temp += c;
                    }
                }
                axiom = temp;
            }

            Vector3 position = Vector3.Zero;
            Matrix rotation = Matrix.RotationAxis(new Vector3(0,0,1), (float)Math.PI / 2.0f);

            int angleOffset = rnd.Next(_maxAngleOffset);

            foreach (var c in axiom)
            {
                Matrix tempRotation = Matrix.Identity;

                switch (c)
                {
                    case 'G':
                    case 'F':
                        // Tree trunk
                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X + 1, rootPosition.Y + (int)position.Y, rootPosition.Z + (int)position.Z), BlockId = _trunkBlockId });
                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X - 1, rootPosition.Y + (int)position.Y, rootPosition.Z + (int)position.Z), BlockId = _trunkBlockId });
                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X, rootPosition.Y + (int)position.Y, rootPosition.Z + (int)position.Z + 1), BlockId = _trunkBlockId });
                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X, rootPosition.Y + (int)position.Y, rootPosition.Z + (int)position.Z - 1), BlockId = _trunkBlockId });
                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X, rootPosition.Y + (int)position.Y, rootPosition.Z + (int)position.Z), BlockId = _trunkBlockId });

                        // Generate leaves
                        if (_stackPosition.Count > 1)
                        {
                            int size = 1;

                            for (int x = -size; x <= size; x++)
                            {
                                for (int y = -size; y <= size; y++)
                                {
                                    for (int z = -size; z <= size; z++)
                                    {
                                        if (Math.Abs(x) == size && Math.Abs(y) == size && Math.Abs(z) == size) continue;

                                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X + 1 + x, rootPosition.Y + (int)position.Y + y, rootPosition.Z + (int)position.Z + z), BlockId = _foliageBlockId });
                                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X - 1 + x, rootPosition.Y + (int)position.Y + y, rootPosition.Z + (int)position.Z + z), BlockId = _foliageBlockId });
                                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X + x, rootPosition.Y + (int)position.Y + y, rootPosition.Z + (int)position.Z + 1 + z), BlockId = _foliageBlockId });
                                        mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + (int)position.X + x, rootPosition.Y + (int)position.Y + y, rootPosition.Z + (int)position.Z - 1 + z), BlockId = _foliageBlockId });
                                        //mesh.Add(new BlockWithPosition() { WorldPosition = new Vector3I(rootPosition.X + position.X + x, rootPosition.Y + position.Y + y, rootPosition.Z + position.Z + z), BlockId = _foliageBlockId });
                                    }
                                }
                            }
                        }

                        Vector3 dir = new Vector3(1, 0, 0);
                        Vector3.TransformNormal(ref dir, ref rotation, out dir); //Apply rotation on the direction vector

                        position += dir; //Make the "cursor advance"

                        break;
                    case '[': //Save rotation and position
                        _stackOrientation.Push(rotation);
                        _stackPosition.Push(position);
                        break;
                    case ']': //Restore rotation and position
                        rotation = _stackOrientation.Pop();
                        position = _stackPosition.Pop();
                        break;
                    case '+': //Rotate Z axis
                        tempRotation = Matrix.Identity;
                        tempRotation = Matrix.RotationAxis(new Vector3(0, 0, 1), (float)MathHelper.ToRadians(_angleDegree + angleOffset));
                        rotation *= tempRotation;
                        break;
                    case '-': //Rotate Z axis negatively
                        tempRotation = Matrix.Identity;
                        tempRotation = Matrix.RotationAxis(new Vector3(0, 0, -1), (float)MathHelper.ToRadians(_angleDegree + angleOffset));
                        rotation *= tempRotation;
                        break;
                    case '&': //Rotate Y axis
                        tempRotation = Matrix.Identity;
                        tempRotation = Matrix.RotationAxis(new Vector3(0, 1, 0), (float)MathHelper.ToRadians(_angleDegree + angleOffset));
                        rotation *= tempRotation;
                        break;
                    case '^': //Rotate Y axis negatively
                        tempRotation = Matrix.Identity;
                        tempRotation = Matrix.RotationAxis(new Vector3(0, -1, 0), (float)MathHelper.ToRadians(_angleDegree + angleOffset));
                        rotation *= tempRotation;
                        break;
                    case '*': //Rotate X axis
                        tempRotation = Matrix.Identity;
                        tempRotation = Matrix.RotationAxis(new Vector3(1, 0, 0), (float)MathHelper.ToRadians(_angleDegree));
                        rotation *= tempRotation;
                        break;
                    case '/': //Rotate X axis negatively 
                        tempRotation = Matrix.Identity;
                        tempRotation = Matrix.RotationAxis(new Vector3(-1, 0, 0), (float)MathHelper.ToRadians(_angleDegree));
                        rotation *= tempRotation;
                        break;
                    default:
                        break;
                }
            }

            return mesh;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
