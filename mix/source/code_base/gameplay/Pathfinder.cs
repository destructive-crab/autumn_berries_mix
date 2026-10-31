using System.Numerics;
using destruct.Debug;
using mix.code_base;

namespace autumn_berries_mix
{
    //todo rewrite this whole thing
    public static class Pathfinder
    {
        private static Vec2I currentTarget;
        private static bool currentlyPerformingPathfinding = false;

        private class PathNode : IEquatable<PathNode>
        {
            public PathNode(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }
            
            public PathNode PreviousPathNode { get; private set; }

            public float F => G + H;
            
            public float G { get; private set; } = 0;
            public float H { get; private set; } = 0;
            public int X { get; private set; }
            public int Y { get; private set; }

            public void SetCosts(float g, float h)
            {
                G = g; 
                H = h;
            }
            
            public void SetPreviousPoint(PathNode startingPathNode) => PreviousPathNode = startingPathNode;
            
            public void SetXY(Vec2I point)
            {
                X = point.X;
                Y = point.Y;
            }

            public bool Equals(PathNode node)
            {
                if (node == null)
                    return false;
                
                return node.X == this.X && node.Y == this.Y;
            }
        }
        
        private sealed class PointComparer : IComparer<PathNode>
        {
            public int Compare(PathNode x, PathNode y)
            {
                if (x.F > y.F) return 1;
                else if (x.F < y.F) return -1;
                return 0;
            }
        }

        private static float DefineGCost(PathNode startPathNode, PathNode endPathNode)
        {
            return Vector2.Distance(new Vector2(startPathNode.X, startPathNode.Y), new Vector2(endPathNode.X, endPathNode.Y));
        }
        
        private static int Heuristic(PathNode first, PathNode second) => (int)(MathF.Abs(first.X - second.X) + MathF.Abs(first.Y - second.Y));
        private static bool IsCollider(GameMap map, Vec2I position)
        {
            return map.CellHasEntityWith<BlockCell>(position);
        }
        
        private static List<PathNode> GetNeighbourPoints(GameMap map, PathNode pathNode, List<PathNode> ignoredPoints)
        {
            List<PathNode> neighbourPoints = new List<PathNode>();

            var cellsAround = map.GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<BlockCell>(pathNode.X, pathNode.Y);
            
            foreach (Vec2I p in cellsAround)
                neighbourPoints.Add(new (p.X, p.Y));
            
            return neighbourPoints;
        }
        
        public static List<Vec2I> FindPath(GameMap map, Vec2I start, Vec2I end)
        {
            if (currentlyPerformingPathfinding)
            {
                LGR.PERR("Cannot perform pathfinding until current operation will be completed");
                return null;
            }
            
            currentTarget = end;
            List<PathNode> nextPoints = new List<PathNode>();

            List<PathNode> visitedPoints = new List<PathNode>();
            
            PathNode startPathNode = new PathNode(start.X, start.Y);
            PathNode endPathNode = new PathNode(end.X, end.Y);

            PathNode currentPathNode = startPathNode;

            int maxIterations = map.LayerHeight * map.LayerWidth * 2;
            int iteration = 0;
            while (iteration < maxIterations)
            {
                iteration++;
                
                if (currentPathNode.X == endPathNode.X && currentPathNode.Y == endPathNode.Y)
                    return RestorePath(currentPathNode);

                List<PathNode> neighbourPoints = GetNeighbourPoints(map, currentPathNode, visitedPoints);

                foreach (PathNode point in neighbourPoints)
                {
                    point.SetCosts(currentPathNode.G + DefineGCost(currentPathNode, point), Heuristic(point, endPathNode));
                    point.SetPreviousPoint(currentPathNode);
                    nextPoints.Add(point);

                    visitedPoints.Add(point);
                }
                
                nextPoints.Sort(new PointComparer());

                currentPathNode = nextPoints[0];
                nextPoints.Remove(currentPathNode);
            }

            return null;
        }
        private static List<Vec2I> RestorePath(PathNode endPathNode)
        {
            PathNode current = endPathNode;
            List<Vec2I> path = new List<Vec2I>();

            do
            {
                path.Add(new Vec2I(current.X, current.Y));
                current = current.PreviousPathNode;
            } while ((current.PreviousPathNode != null));

            path.Reverse();

            return path;
        }
    }
}