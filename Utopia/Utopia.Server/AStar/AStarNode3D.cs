using System.Collections.Generic;
using Utopia.Shared.Chunks;
using Utopia.Shared.Structs;

namespace Utopia.Server.AStar
{
    /// <summary>
    /// Represents a node for pathfinding algo in 3d
    /// </summary>
    public class AStarNode3D : AStarNode<AStarNode3D>
    {
        public readonly LandscapeCursor Cursor;

        public AStarNode3D(LandscapeCursor cursor, AStarNode3D aParent, AStarNode3D aGoalNode, double aCost) : 
            base(aParent, aGoalNode, aCost)
        {
            Cursor = cursor;
        }

        public override double Estimate()
        {
            if(GoalNode == null)
                return double.PositiveInfinity;

            return Vector3I.Distance(Cursor.GlobalPosition, GoalNode.Cursor.GlobalPosition);
        }

        public override void GetSuccessors(List<AStarNode3D> aSuccessors)
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
                    AddSuccessor(aSuccessors, new Vector3I(x, 0, y));
                }
            }
        }

        private void AddSuccessor(List<AStarNode3D> aSuccessors, Vector3I move)
        {
            // get landscape cursor of possible position
            var cursor = Cursor.Clone().Move(move);

            // do we have a blocking cube in front of our face?
            if (cursor.IsSolidUp())
                return;

            if (!cursor.IsSolid())
            {
                // simple move?
                if (cursor.IsSolidDown())
                {
                    CreateNode(aSuccessors, cursor, 1);
                    return;
                }

                // or jump down?
                if (cursor.IsSolid(new Vector3I(0, -2, 0)))
                {
                    CreateNode(aSuccessors, cursor.MoveDown(), 2);
                }
            }
            else if (!cursor.IsSolid(new Vector3I(0, 2, 0)))
            {
                // jump up!
                CreateNode(aSuccessors, cursor.MoveUp(), 3);
            }
        }

        private void CreateNode(List<AStarNode3D> aSuccessors, LandscapeCursor cursor, double relativeCost)
        {
            var node = new AStarNode3D(cursor, this, GoalNode, Cost + relativeCost);
            aSuccessors.Add(node);
        }

        public override bool IsSameState(AStarNode3D aNode)
        {
            return aNode.Cursor.GlobalPosition.X == Cursor.GlobalPosition.X && aNode.Cursor.GlobalPosition.Y == Cursor.GlobalPosition.Y && aNode.Cursor.GlobalPosition.Z == Cursor.GlobalPosition.Z;
        }

        public override int GetHashCode()
        {
            return Cursor.GlobalPosition.X + (Cursor.GlobalPosition.Y << 10) + (Cursor.GlobalPosition.Z << 20);
        }

        public override string ToString()
        {
            return string.Format("Node 3d [{0}]", Cursor.GlobalPosition);
        }
    }
}