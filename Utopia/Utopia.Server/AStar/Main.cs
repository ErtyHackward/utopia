using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Utopia.Server.AStar
{
    /// <summary>
	/// Test class for doing A* pathfinding on a 2D map.
	/// </summary>
	class MainClass
	{
		#region Test Maps

        static int[,] Map = {
            { 1,-1, 1, 1, 1,-1, 1, 1, 1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, 1, 1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, 1, 1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, 1, 1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, -1, 1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, -1, 1, -1 },
            { 1,-1, 1,-1, 1,-1, 1, -1, 1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, -1, -1, 1 },
            { 1,-1, 1,-1, 1,-1, 1, 2, -1, 1 },
            { 1, 1, 1,-1, 1, 1, 2, 3, -1, 1 }
        };
        //static int[,] Map = {
        //    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1,-1, 1 },
        //    { 1,-1,-1,-1,-1,-1,-1,-1,-1, 1 },
        //    { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
        //};
//		static int[,] Map = {
//			{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
//			{ 1, 1, 1, 1, 1, 2, 1, 1, 1, 1 },
//			{ 1, 1, 1, 1, 2, 3, 2, 1, 1, 1 },
//			{ 1, 1, 1, 2, 3, 4, 3, 2, 1, 1 },
//			{ 1, 1, 2, 3, 4, 5, 4, 3, 2, 1 },
//			{ 1, 1, 1, 2, 3, 4, 3, 2, 1, 1 },
//			{ 1, 1, 1, 1, 2, 3, 2, 1, 1, 1 },
//			{ 1, 1, 1, 1, 1, 2, 1, 1, 1, 1 },
//			{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 },
//			{ 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 }
//		};

		#endregion

		#region Public Methods

		/// <summary>
		/// Gets movement cost from the 2-dimensional map
		/// </summary>
		/// <param name="x">X-coordinate</param>
		/// <param name="y">Y-coordinate</param>
		/// <returns>Returns movement cost at the specified point in the map</returns>
		static public int GetMap(int x,int y)
		{
			if((x < 0) || (x > 9))
				return(-1);
			if((y < 0) || (y > 9))
				return(-1);
			return(Map[y,x]);
		}

		/// <summary>
		/// Prints the solution
		/// </summary>
		/// <param name="ASolution">The list that holds the solution</param>
		static public void PrintSolution(List<AStarNode2D> ASolution)
		{
			for(int j=0;j<10;j++) 
			{
				for(int i=0;i<10;i++) 
				{
					bool solution = false;
					foreach(AStarNode2D n in ASolution) 
					{
						AStarNode2D tmp = new AStarNode2D(null,null,0,i,j);
						solution = n.IsSameState(tmp);
						if(solution)
							break;
					}
					if(solution)
						Console.Write("o ");
					else
						if(MainClass.GetMap(i,j) == -1)
						Console.Write("# ");
					else
						Console.Write(". ");
				}
				Console.WriteLine("");
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			Console.WriteLine("Starting...");

            //var astar = new AStar<AStarNode2D>(new Comparer2d());
            var astar = new AStar<AStarNode2D>();

            AStarNode2D GoalNode = new AStarNode2D(null, null, 0, 9, 9);
            AStarNode2D StartNode = new AStarNode2D(null, GoalNode, 0, 0, 0);
			StartNode.GoalNode = GoalNode;
            var sw = Stopwatch.StartNew();
			astar.FindPath(StartNode,GoalNode);
            sw.Stop();
			PrintSolution(astar.Solution);

            Console.WriteLine(sw.ElapsedMilliseconds);

			Console.ReadLine();
		}

		#endregion
	}

    public class Comparer2d : IComparer<AStarNode2D>
    {
        public int Compare(AStarNode2D x, AStarNode2D y)
        {
            if (x == null && y == null)
                return 0;
            if (x == null)
                return -1;
            if (y == null)
                return 1;

            if (x.X == y.X && x.Y == y.Y)
                return 0;

            var cmp = x.TotalCost.CompareTo(y.TotalCost);

            if(cmp != 0)
                return cmp;

            cmp = x.X.CompareTo(y.X);
            if(cmp != 0) return cmp;

            return x.Y.CompareTo(y.Y);
        }
    }
}
