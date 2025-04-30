namespace pacman.Models
{
    public class ClassicStrategy : IGhostStrategy
    {
        // Create a graph from the board

        // Helper method to check if a position is within bounds
        static private bool IsWithinBounds(Position position, int rows, int cols)
        {
            return position.Y >= 0 && position.Y < rows && position.X >= 0 && position.X < cols;
        }

        static Direction[] directions = { Direction.Left, Direction.Right, Direction.Up, Direction.Down };

        static IEnumerable<Position> GetNeighbors(Board board,Position position)
        {
            foreach (var direction in directions)
            {
                var neighborPosition = GameState.CalculateNewPosition(position, direction);
                if (IsWithinBounds(neighborPosition, board.Height, board.Width) && board[neighborPosition] != TileType.Wall)
                    yield return neighborPosition;
            }
        }

        // Implement the shortest path algorithm (BFS)
        static private List<Position> FindShortestPath(Board board, Position start, Position target)
        {
            var queue = new Queue<Position>();
            var visited = new HashSet<Position>();
            var parentMap = new Dictionary<Position, Position>();

            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == target)
                {
                    // Reconstruct the path
                    var path = new List<Position>();
                    while (current != null)
                    {
                        path.Add(current);
                        parentMap.TryGetValue(current, out current);
                    }
                    path.Reverse();
                    return path;
                }

                foreach (var neighbor in GetNeighbors(board,current))
                {
                    if (!visited.Contains(neighbor))
                    {
                        visited.Add(neighbor);
                        parentMap[neighbor] = current;
                        queue.Enqueue(neighbor);
                    }
                }
            }

            return new List<Position>(); // Return empty if no path found
        }


        enum State
        {
            Scatter,
            Chase,
        }

        State[] currentState = [State.Scatter, State.Scatter, State.Scatter, State.Scatter];
        Position[] targetCorners = null;
        Position[] getTargetCorners(Board board) => 
        [
            new Position(1, 1), // Top-left corner
            new Position(board.Width-2, 1), // Top-right corner
            new Position(1, board.Height-2), // Bottom-left corner
            new Position(board.Width-2, board.Height-2) // Bottom-right corner
        ];

        Direction GetDirection(Position from, Position to)
        {
            if (to.X > from.X) return Direction.Right;
            if (to.X < from.X) return Direction.Left;
            if (to.Y > from.Y) return Direction.Down;
            if (to.Y < from.Y) return Direction.Up;
            return Direction.None; // No movement
        }

        // Implement the classical strategy for ghosts
        public Direction[] GetNextMove(GameState game)
        {
            if(targetCorners == null)
            {
                targetCorners = getTargetCorners(game.Board);
            }   
            var moves = new Direction[game.Ghosts.Length];

            for (int i = 0; i < game.Ghosts.Length; i++)
            {
                var ghost = game.Ghosts[i];
                if (currentState[i] == State.Scatter)
                {
                    var targetCorner = targetCorners[i];
                    var path = FindShortestPath(game.Board, ghost.Position, targetCorner);
                    if (path.Count > 1)
                    {
                        var nextPosition = path[1];
                        moves[i] = GetDirection(ghost.Position, nextPosition);
                        if(nextPosition==targetCorner)
                            currentState[i] = State.Chase; // Switch to chase state when reaching the corner
                    }
                    else
                    {
                        // If no path, choose a random valid direction
                        moves[i] = Direction.None;
                    }
                }
                else
                {
                    // Chase state: move towards Pacman
                    var path = FindShortestPath(game.Board, ghost.Position, game.PacmanPosition);
                    if (path.Count > 1)
                    {
                        var nextPosition = path[1];
                        moves[i] = GetDirection(ghost.Position, nextPosition);
                    }
                    else
                    {
                        // If no path, choose a random valid direction
                        moves[i] = Direction.None;
                    }
                }
            }
            return moves;
        }
        
    }
}
