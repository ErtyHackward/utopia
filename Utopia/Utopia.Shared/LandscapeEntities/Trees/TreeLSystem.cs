using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using S33M3CoreComponents.Maths;
using S33M3Resources.Structs;
using SharpDX;
using Utopia.Shared.Structs.Landscape;

namespace Utopia.Shared.LandscapeEntities.Trees
{
    public class TreeLSystem
    {
        #region Private Variables
        #endregion

        #region Public Properties
        #endregion

        public TreeLSystem()
        {
        }

        #region Public Methods
        public List<BlockWithPosition> Generate(FastRandom rnd, Vector3I WPos, TreeTemplate treeType)
        {
            Dictionary<Vector3I, BlockWithPosition> meshDico = new Dictionary<Vector3I, BlockWithPosition>();

            //randomize tree growth level, minimum=2
            int iterations = treeType.Iteration;
            if (treeType.IterationRndLevel > 0) iterations -= rnd.Next(treeType.IterationRndLevel);
            if (iterations < 2) iterations = 2; //Minimum 2, in order to have something correct

            double angleRad = MathHelper.ToRadians(treeType.Angle);
            double angleOffsetRad = MathHelper.ToRadians(rnd.NextDouble());

            //initialize rotation matrix, position and stacks for branches
            Matrix rotation = Matrix.RotationAxis(new Vector3(0, 0, 1), (float)Math.PI / 2.0f);
            Vector3 position = Vector3.Zero;
            Stack<Matrix> stackOrientation = new Stack<Matrix>();
            Stack<Vector3> stackPosition = new Stack<Vector3>();

            //Generate axiom ====================================================================
            string axiom = treeType.Axiom;

            for (int i = 0; i < iterations; i++)
            {
                string temp = "";
                foreach (char axiomChar in axiom)
                {
                    switch (axiomChar)
                    {
                        case 'A':
                            temp += treeType.Rules_a.Rule;
                            break;
                        case 'B':
                            temp += treeType.Rules_b.Rule;
                            break;
                        case 'C':
                            temp += treeType.Rules_c.Rule;
                            break;
                        case 'D':
                            temp += treeType.Rules_d.Rule;
                            break;
                        case 'a':
                            if (treeType.Rules_a.Prob >= rnd.NextDouble())
                                temp += treeType.Rules_a.Rule;
                            break;
                        case 'b':
                            if (treeType.Rules_b.Prob >= rnd.NextDouble())
                                temp += treeType.Rules_b.Rule;
                            break;
                        case 'c':
                            if (treeType.Rules_c.Prob >= rnd.NextDouble())
                                temp += treeType.Rules_c.Rule;
                            break;
                        case 'd':
                            if (treeType.Rules_d.Prob >= rnd.NextDouble())
                                temp += treeType.Rules_d.Rule;
                            break;
                        default:
                            temp += axiomChar;
                            break;
                    }
                }
                axiom = temp;
            }

            /* build tree out of generated axiom

	        Key for Special L-System Symbols used in Axioms

            G  - move forward one unit with the pen up
            F  - move forward one unit with the pen down drawing trunks and branches
            f  - move forward one unit with the pen down drawing leaves
            A  - replace with rules set A
            B  - replace with rules set B
            C  - replace with rules set C
            D  - replace with rules set D
            a  - replace with rules set A, chance 90%
            b  - replace with rules set B, chance 80%
            c  - replace with rules set C, chance 70%
            d  - replace with rules set D, chance 60%
            +  - yaw the turtle right by angle degrees
            -  - yaw the turtle left by angle degrees
            &  - pitch the turtle down by angle degrees
            ^  - pitch the turtle up by angle degrees
            /  - roll the turtle to the right by angle degrees
            *  - roll the turtle to the left by angle degrees
            [  - save in stack current state info
            ]  - recover from stack state info

            */

            int x, y, z;
            BlockWithPosition block;
            foreach(char axiomChar in axiom)
            {
                Matrix tempRotation = Matrix.Identity;
                Vector3 dir;

                switch (axiomChar)
                {
                    case 'G':
                        dir = new Vector3(1, 0, 0);
                        dir = Vector3.TransformNormal(dir, rotation);
                        position += dir;
                        break;
                    case 'F':

                        //Single Trunk => Always added
                        block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z) };
                        if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);

                        //Handling other trunk type
                        if (stackOrientation.Count == 0 || (stackOrientation.Count > 0 && !treeType.SmallBranches == false))
                        {
                            switch (treeType.TrunkType)
                            {
                                case TrunkType.Double:
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + 1, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z + 1) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + 1, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z + 1) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    break;
                                case TrunkType.Crossed:
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + 1, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X - 1, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z + 1) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    block = new BlockWithPosition() { isMandatory = true, BlockId = treeType.TrunkBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z - 1) };
                                    if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    break;
                                default:
                                    break;
                            }
                        }

                        //Create foliage "around" trunk, only for "Sub" branch, not the main trunk
                        if (stackOrientation.Count >= treeType.FoliageGenerationStart)
                        {
                            int foliageSize = 1;
                            for (x = -foliageSize; x <= foliageSize; x++)
                            {
                                for (y = -foliageSize; y <= foliageSize; y++)
                                {
                                    for (z = -foliageSize; z <= foliageSize; z++)
                                    {
                                        //Create only foliage outer form (Not inside)
                                        if (Math.Abs(x) == foliageSize && Math.Abs(y) == foliageSize && Math.Abs(z) == foliageSize) continue;

                                        block = new BlockWithPosition() { BlockId = treeType.FoliageBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + x + 1, WPos.Y + (int)position.Y + y, WPos.Z + (int)position.Z + z) };
                                        if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                        block = new BlockWithPosition() { BlockId = treeType.FoliageBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + x - 1, WPos.Y + (int)position.Y + y, WPos.Z + (int)position.Z + z) };
                                        if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                        block = new BlockWithPosition() { BlockId = treeType.FoliageBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + x, WPos.Y + (int)position.Y + y, WPos.Z + (int)position.Z + z + 1) };
                                        if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                        block = new BlockWithPosition() { BlockId = treeType.FoliageBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X + x, WPos.Y + (int)position.Y + y, WPos.Z + (int)position.Z + z - 1) };
                                        if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                                    }
                                }
                            }
                        }

                        dir = new Vector3(1, 0, 0);
                        Vector3.TransformNormal(ref dir, ref rotation, out dir);
                        position += dir;
                        break;

                    case 'f':
                        block = new BlockWithPosition() { BlockId = treeType.FoliageBlock, WorldPosition = new Vector3I(WPos.X + (int)position.X, WPos.Y + (int)position.Y, WPos.Z + (int)position.Z) };
                        if (!meshDico.ContainsKey(block.WorldPosition)) meshDico.Add(block.WorldPosition, block);
                        dir = new Vector3(1, 0, 0);
                        dir = Vector3.TransformNormal(dir, rotation);
                        position += dir;
                        break;
                    // Move commands
                    case '[': //Save states
                        stackOrientation.Push(rotation);
                        stackPosition.Push(position);
                        break;
                    case ']': //Load back states
                        rotation = stackOrientation.Pop();
                        position = stackPosition.Pop();
                        break;
                    case '+':
                        tempRotation = Matrix.RotationAxis(new Vector3(0, 0, 1),(float)(angleRad + angleOffsetRad));
                        rotation = tempRotation * rotation;
                        break;
                    case '-':
                        tempRotation = Matrix.RotationAxis(new Vector3(0, 0, -1), (float)(angleRad + angleOffsetRad));
                        rotation = tempRotation * rotation;
                        break;
                    case '&':
                        tempRotation = Matrix.RotationAxis(new Vector3(0, 1, 0), (float)(angleRad + angleOffsetRad));
                        rotation = tempRotation * rotation;
                        break;
                    case '^':
                        tempRotation = Matrix.RotationAxis(new Vector3(0, -1, 0), (float)(angleRad + angleOffsetRad));
                        rotation = tempRotation * rotation;
                        break;
                    case '*':
                        tempRotation = Matrix.RotationAxis(new Vector3(1, 0, 0), (float)(angleRad));
                        rotation = tempRotation * rotation;
                        break;
                    case '/':
                        tempRotation = Matrix.RotationAxis(new Vector3(-1, 0, 0), (float)(angleRad));
                        rotation = tempRotation * rotation;
                        break;
                    default:
                        break;
                }
            }

            return meshDico.Values.ToList();
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
