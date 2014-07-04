using System;
using System.Collections.Generic;
using S33M3Resources.Structs;
using Utopia.Shared.Interfaces;

namespace Utopia.Shared.Server.AStar
{
    /// <summary>
    /// Represents a node for pathfinding algo in 3d
    /// </summary>
    public class AStarNode3D : AStarNode<AStarNode3D>
    {
        public readonly ILandscapeCursor Cursor;

        public AStarNode3D(ILandscapeCursor cursor, AStarNode3D aParent, AStarNode3D aGoalNode, double aCost) : 
            base(aParent, aGoalNode, aCost)
        {
            Cursor = cursor;
        }

        public override double Estimate()
        {
            if (GoalNode == null)
                return double.PositiveInfinity;

            return Vector3I.Distance(Cursor.GlobalPosition, GoalNode.Cursor.GlobalPosition);
        }

        public override void GetSuccessors(List<AStarNode3D> aSuccessors, Func<AStarNode3D, double> costModify)
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
                    AddSuccessor(aSuccessors, new Vector3I(x, 0, y), costModify);
                }
            }
        }

        private void AddSuccessor(List<AStarNode3D> aSuccessors, Vector3I move, Func<AStarNode3D, double> costModify)
        {
            // get landscape cursor of possible position
            var cursor = Cursor.Clone().Move(move);

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
                    CreateNode(aSuccessors, cursor, 1, costModify);
                    return;
                }

                // or jump down?
                if (cursor.PeekProfile(new Vector3I(0, -2, 0)).IsSolidToEntity)
                {
                    CreateNode(aSuccessors, cursor.Move(Vector3I.Down), 2, costModify);
                }
            }
            else if (!cursor.PeekProfile(new Vector3I(0, 2, 0)).IsSolidToEntity)
            {
                // jump up!
                CreateNode(aSuccessors, cursor.Move(Vector3I.Up), 3, costModify);
            }
        }

        private void CreateNode(List<AStarNode3D> aSuccessors, ILandscapeCursor cursor, double relativeCost, Func<AStarNode3D, double> costModify)
        {
            var node = new AStarNode3D(cursor, this, GoalNode, Cost + relativeCost);

            if (costModify != null)
                node.Cost += costModify(node);

            aSuccessors.Add(node);
        }

        public override bool IsSameState(AStarNode3D aNode)
        {
            return aNode.Cursor.GlobalPosition == Cursor.GlobalPosition;
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