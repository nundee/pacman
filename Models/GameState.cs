using System;
using System.Collections.Generic;
using System.Threading;

namespace pacman.Models
{
    public enum Direction
    {
        Up,
        Down,
        Left,
        Right,
        None
    }

    public enum TileType
    {
        Empty,
        Wall,
        Dot,
        PowerPellet,
        PacMan,
        // ghosts
        Blinky,
        Pinky,
        Inky,
        Clyde,
    }

    public record Position(int X, int Y);

    public class Board
    {
        public TileType[,] Tiles { get; set; }
        public int Width => Tiles.GetLength(1);
        public int Height => Tiles.GetLength(0);


        public Board(int width, int height)
        {
            Tiles = new TileType[height, width];
        }

        public TileType this[int row, int col] 
        {
            get => Tiles[row,col];
            set => Tiles[row,col] = value;
        }
        public TileType this[Position pos] 
        {
            get => Tiles[pos.Y,pos.X];
            set => Tiles[pos.Y,pos.X] = value;
        }
    }

    public class Ghost
    {
        public TileType Name { get; set; }

        public Position Position { get; set; }
        public Direction Direction { get; set; }
    }

    public class GameState : IDisposable
    {

        public readonly int TimerInterval = 150;

        public Board Board { get; private set; }
        Board FoodMap = null;
        public Position PacmanPosition { get; set; }
        public Ghost[] Ghosts { get; set; }
        public int Score { get; set; }
        public bool IsGameOver { get; set; }
        public bool IsStarted { get; private set; } = false;
        public bool IsPoweredUp { get; set; }
        public int RemainingDots { get; private set; }

        Queue<Direction> directionQueue = new Queue<Direction>();
        Direction currentDirection = Direction.None;

        PeriodicTimer? timer=null;
        Task gameLoop;

        public event Action Updated;

        //IGhostStrategy ghostStrategy = new RandomGhostStrategy(); // Default strategy
        IGhostStrategy ghostStrategy = new ClassicStrategy(); // Default strategy

        public Direction CurrentDirection => currentDirection;
        public void PushMove(Direction direction)
        {
            if (direction != Direction.None && direction != currentDirection && directionQueue.Count < 50)
                directionQueue.Enqueue(direction);
        }

        public GameState()
        {            
            InitializeGame();
        }

        private void InitializeGame()
        {
            //Board = new TileType[28, 31]; // Standard Pacman board size
            //FoodMap = new TileType[28, 31];
            Score = 0;
            IsGameOver = false;
            IsPoweredUp = false;
            LoadBoard(KnownBoards.layout1);
        }

        static TileType asTile(char c) => c switch
        {
            'w' => TileType.Wall,
            '.' => TileType.Dot,
            'o' => TileType.PowerPellet,
            ' ' => TileType.Empty,
            '-' => TileType.Empty,
            _ => TileType.Empty
        };

        private void LoadBoard(string[] layout)
        {
            RemainingDots = 0;
            int numRows = layout.Length;
            int numCols = layout.Max(s=>s.Length);
            Board = new Board(height:numRows, width:numCols);
            FoodMap = new Board(height: numRows, width: numCols);
            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < layout[r].Length; c++)
                {
                    FoodMap[r,c] = Board[r,c] = asTile(layout[r][c]);
                    if (Board[r,c] == TileType.Dot)
                    {
                        RemainingDots++;
                    }
                }
            }

            // Set initial Pacman position
            PacmanPosition = new Position(14, 23);
            Board[PacmanPosition] = TileType.PacMan;

            // Set initial ghost positions
            Ghosts = [
                 new Ghost {Name = TileType.Pinky,  Position=new Position(13, 14),Direction=Direction.None},
                 new Ghost {Name = TileType.Blinky, Position=new Position(14, 14),Direction=Direction.None},
                 new Ghost {Name = TileType.Inky,   Position=new Position(15, 14),Direction=Direction.None},
                 new Ghost {Name = TileType.Clyde,  Position=new Position(16, 14),Direction=Direction.None}
            ];

            foreach (var ghost in Ghosts)
            {
                Board[ghost.Position] =  ghost.Name;
            }
        }


        public void StartGame()
        {
            timer?.Dispose();
            timer = new PeriodicTimer(TimeSpan.FromMilliseconds(TimerInterval));
            IsStarted = true;
            gameLoop = Task.Run(GameLoop);
        }

        public void StopGame()
        {
            IsStarted = false;
            IsGameOver = true;
            gameLoop?.Wait();
            timer?.Dispose();
            timer = null;
        }

        private async void GameLoop()
        {
            while (!IsGameOver)
            {
                await timer!.WaitForNextTickAsync();
                NextMove();
                NextGhostMove();
                CheckGameOver();
                Updated?.Invoke();
            }
        }

        void NextGhostMove()
        {
            var ghostMoves = ghostStrategy.GetNextMove(this);
            for (int i = 0; i < Ghosts.Length; i++)
            {
                var ghost= Ghosts[i];
                Position ghostPosition = ghost.Position;
                Direction ghostDirection = ghostMoves[i];
                Position newGhostPosition = CalculateNewPosition(ghostPosition, ghostDirection);
                if (IsValidMove(newGhostPosition))
                {
                    Board[ghost.Position] = FoodMap[ghost.Position];
                    Board[newGhostPosition] = ghost.Name;
                    ghost.Position = newGhostPosition;
                    ghost.Direction = ghostDirection;
                }
            }
        }

        void NextMove()
        {
            if (IsGameOver) return;
            var nextDirection = directionQueue.Count > 0 ? directionQueue.Peek() : Direction.None;
            Position candidatePosition = CalculateNewPosition(PacmanPosition, nextDirection);
            if (IsValidPacmanMove(candidatePosition))
            {
                currentDirection = nextDirection;
                directionQueue.Dequeue();
                UpdatePosition(candidatePosition);
                return;
            }
            else
            {
                candidatePosition = CalculateNewPosition(PacmanPosition, currentDirection);
                if (IsValidPacmanMove(candidatePosition))
                    UpdatePosition(candidatePosition);
                else //pacman got stuck
                    directionQueue.Clear();
            }
        }

        static public Position CalculateNewPosition(Position current, Direction direction) =>
            direction switch
            {
                Direction.Up => current with { Y = current.Y - 1 },
                Direction.Down => current with { Y = current.Y + 1 },
                Direction.Left => current with { X = current.X - 1 },
                Direction.Right => current with { X = current.X + 1 },
                _ => current
            };


        public bool IsValidMove(Position position)
        {
            if (position.X < 0 || position.X >= Board.Width ||
                position.Y < 0 || position.Y >= Board.Height)
                return false;

            return Board[position] != TileType.Wall;
        }


        private bool IsValidPacmanMove(Position position) => IsValidMove(position) && (position.X!=PacmanPosition.X || position.Y!=PacmanPosition.Y);


        private void UpdatePosition(Position newPosition)
        {
            // Clear old position
            Board[PacmanPosition] = TileType.Empty;

            // Update score and handle dot collection
            if (Board[newPosition] == TileType.Dot)
            {
                Score += 10;
                RemainingDots--;
                FoodMap[newPosition] = TileType.Empty; // Remove dot from FoodMap
            }
            else if (Board[newPosition] == TileType.PowerPellet)
            {
                Score += 50;
                FoodMap[newPosition] = TileType.Empty;
                IsPoweredUp = true;
                // TODO: Implement power pellet timer
            }

            // Update Pacman's position
            Board[newPosition] = TileType.PacMan;
            PacmanPosition = newPosition;
        }

        private void CheckGameOver()
        {
            // Check if Pacman collides with a ghost
            bool collidesWithGhost = Ghosts.Any(ghost =>
                ghost.Position.X == PacmanPosition.X && ghost.Position.Y == PacmanPosition.Y);

            if (collidesWithGhost && !IsPoweredUp)
            {
                IsGameOver = true;
            }
            else if (RemainingDots == 0)
            {
                IsGameOver = true; // Victory!
            }
        }

        public void Dispose()
        {
            StopGame();
        }
    }
} 