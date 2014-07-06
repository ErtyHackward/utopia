using System.Collections.Generic;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Server.AStar
{
    /// <summary>
    /// Represents a node for pathfinding algo in 3d
    /// </summary>
    public class AStarNodeFunc3D : AStarNodeFunc<AStarNodeFunc3D>
    {
        public readonly ILandscapeCursor Cursor;

        public AStarNodeFunc3D(ILandscapeCursor cursor, AStarNodeFunc3D aParent, double aCost)
        {
            Parent = aParent;
            Cursor = cursor;
            Cost = aCost;
        }
        
        public static void GetSuccessors(AStarNodeFunc3D from, List<AStarNodeFunc3D> aSuccessors)
        {
            // we need to find all possible moves from this point
            // frequently there is only 8 possible moves, and only ladders can offer to go up and down
            // as far we have not yet ladders just check all 8 directions

            for (int x = -1; x < 2; x++)
            {
                for (int y = -1; y < 2; y++)
                {
                    if (x == 0 && y == 0)
                        continue;
                    AddSuccessor(from, aSuccessors, new Vector3I(x, 0, y));
                }
            }
        }

        private static void AddSuccessor(AStarNodeFunc3D from, List<AStarNodeFunc3D> aSuccessors, Vector3I move)
        {
            // get landscape cursor of possible position
            var cursor = from.Cursor.Clone().Move(move);

            // do we have a blocking cube in front of our face?
            if (cursor.PeekProfile(Vector3I.Up).IsSolidToEntity)
                return;

            // diagonal move requires blocks to be empty
            if (move.x != 0 && move.z != 0)
            {
                if (cursor.PeekProfile(new Vector3I(move.X, 0, 0)).IsSolidToEntity)
                    return;
                if (cursor.PeekProfile(new Vector3I(move.X, 1, 0)).IsSolidToEntity)
                    return;
                if (cursor.PeekProfile(new Vector3I(0, 0, move.Z)).IsSolidToEntity)
                    return;
                if (cursor.PeekProfile(new Vector3I(0, 1, move.Z)).IsSolidToEntity)
                    return;
            }

            if (!cursor.PeekProfile().IsSolidToEntity)
            {
                // simple move?
                if (cursor.PeekProfile(Vector3I.Down).IsSolidToEntity)
                {
                    CreateNode(from, aSuccessors, cursor, 0.2f);
                    return;
                }

                // or jump down?
                if (cursor.PeekProfile(new Vector3I(0, -2, 0)).IsSolidToEntity)
                {
                    CreateNode(from, aSuccessors, cursor.Move(Vector3I.Down), 0.4f);
                }
            }
            else if (!cursor.PeekProfile(new Vector3I(0, 2, 0)).IsSolidToEntity)
            {
                // jump up!
                CreateNode(from, aSuccessors, cursor.Move(Vector3I.Up), 0.6f);
            }
        }

        private static void CreateNode(AStarNodeFunc3D from, List<AStarNodeFunc3D> aSuccessors, ILandscapeCursor cursor, double relativeCost)
        {
            var node = new AStarNodeFunc3D(cursor, from, from.Cost + relativeCost);
            aSuccessors.Add(node);
        }

        public override bool IsSameState(AStarNodeFunc3D aNode)
        {
            return aNode.Cursor.GlobalPosition == Cursor.GlobalPosition;
        }

        public override int GetHashCode()
        {
            return Cursor.GlobalPosition.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("Node 3d [{0}]", Cursor.GlobalPosition);
        }
    }
}