using System.Collections.Generic;

namespace Utopia.Server.AStar
{
    public class AStarNode3D : AStarNode<AStarNode3D>
    {
        public AStarNode3D(AStarNode3D aParent, AStarNode3D aGoalNode, double aCost) : base(aParent, aGoalNode, aCost)
        {

        }

        public override void GetSuccessors(List<AStarNode3D> aSuccessors)
        {
            
        }
    }
}