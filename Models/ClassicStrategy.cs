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

        static IEnumerable<Position> GetNeighbors(Board<TileType> board,Position position)
        {
            foreach (var direction in directions)
            {
                var neighborPosition = GameState.CalculateNewPosition(position, direction);
                if (IsWithinBounds(neighborPosition, board.Height, board.Width) && board[neighborPosition] != TileType.Wall)
                    yield return neighborPosition;
            }
        }


        // Implement the shortest path algorithm (BFS)
        static private Direction FindDirInShortestPath(Board<TileType> board, Position start, Position target)
        {
            var queue = new Queue<Position>();
            var visited = new HashSet<Position>();
            var parentMap = new Dictionary<Position, Position>();
            Direction direction = Direction.None;


            queue.Enqueue(start);
            visited.Add(start);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();

                if (current == target)
                {
                    // Reconstruct the path
                    for (; ; )
                    {
                        parentMap.TryGetValue(current, out Position parent);
                        if (parent == null) break; // No parent found, exit the loop
                        direction = GetDirection(parent, current);
                        current = parent;
                    }
                }
                else
                    foreach (var neighbor in GetNeighbors(board, current))
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            parentMap[neighbor] = current;
                            queue.Enqueue(neighbor);
                        }
                    }
            }

            return direction;
        }


        enum State
        {
            Scatter,
            Chase,
        }

        State[] currentState = [State.Scatter, State.Scatter, State.Scatter, State.Scatter];
        Position[] targetCorners = null;
        Position[] getTargetCorners(Board<TileType> board) => 
        [
            new Position(1, 1), // Top-left corner
            new Position(board.Width-2, 1), // Top-right corner
            new Position(1, board.Height-2), // Bottom-left corner
            new Position(board.Width-2, board.Height-2) // Bottom-right corner
        ];

        static Direction GetDirection(Position from, Position to)
        {
            if (to.X > from.X) return Direction.Right;
            if (to.X < from.X) return Direction.Left;
            if (to.Y > from.Y) return Direction.Down;
            if (to.Y < from.Y) return Direction.Up;
            return Direction.None; // No movement
        }


        private Dictionary<(Position start, Position target), Direction> dirCache = new Dictionary<(Position start, Position target), Direction>();

        Direction GetNextShortestPathDir(Board<TileType> board, Position start, Position target)
        {
            if (!dirCache.TryGetValue((start, target), out Direction dir))
            {
                dir = FindDirInShortestPath(board, start, target);
                dirCache[(start, target)] = dir;
            }
            return dir;
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
                    var dir = GetNextShortestPathDir(game.Board, ghost.Position, targetCorner);
                    moves[i]= dir;
                    if (dir != Direction.None)
                    {
                        var nextPosition = GameState.CalculateNewPosition(ghost.Position, dir);
                        if(nextPosition==targetCorner)
                            currentState[i] = State.Chase; // Switch to chase state when reaching the corner
                    }
                }
                else
                {
                    // Chase state: move towards Pacman
                    moves[i] = GetNextShortestPathDir(game.Board, ghost.Position, game.PacmanPosition);
                }
            }
            return moves;
        }
        
    }
}
