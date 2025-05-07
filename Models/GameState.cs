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

    public class Board<T> where T : struct
    {
        public T[,] Cells { get; set; }
        public int Width => Cells.GetLength(1);
        public int Height => Cells.GetLength(0);


        public Board(int width, int height)
        {
            Cells = new T[height, width];
        }

        public T this[int row, int col] 
        {
            get => Cells[row,col];
            set => Cells[row,col] = value;
        }
        public T this[Position pos] 
        {
            get => Cells[pos.Y,pos.X];
            set => Cells[pos.Y,pos.X] = value;
        }
    }

    public class Ghost
    {
        public TileType Name { get; set; }

        public Position Position { get; set; } = new Position(0, 0);
        public Position InitialPosition { get; set; } = new Position(0, 0);
        public Direction Direction { get; set; } = Direction.None;
    }

    [Flags]
    public enum GameStatus
    {
        None     = 0b000,
        Running  = 0b001,
        GameOver = 0b010,
        Victory  = 0b011,
    }

    public class GameState : IDisposable
    {

        public readonly TimeSpan TimerInterval = TimeSpan.FromMilliseconds(150);

        public Board<TileType> Board { get; private set; }
        Board<TileType> FoodMap = null;
        public Position PacmanPosition { get; set; }
        public Ghost[] Ghosts { get; set; }
        public int Score { get; set; }
        public GameStatus Status { get; private set; } = GameStatus.None;
        public bool IsGameOver => Status.HasFlag(GameStatus.GameOver);
        public bool IsRunning => Status.HasFlag(GameStatus.Running);
        public bool GameOver => Status.HasFlag(GameStatus.GameOver);
        public bool IsVictory => Status.HasFlag(GameStatus.Victory);

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

        public void InitializeGame()
        {
            Score = 0;
            Status = GameStatus.None;
            IsPoweredUp = false;
            LoadBoard(KnownBoards.layout1);
            currentDirection = Direction.None;
        }

        static TileType asTile(char c) => c switch
        {
            'w' => TileType.Wall,
            '.' => TileType.Dot,
            'o' => TileType.PowerPellet,
            'P' => TileType.PacMan,
            'b' => TileType.Blinky,
            'p' => TileType.Pinky,
            'i' => TileType.Inky,
            'c' => TileType.Clyde,
            ' ' => TileType.Empty,
            '-' => TileType.Empty,
            _ => TileType.Empty
        };

        private void LoadBoard(string[] layout)
        {
            RemainingDots = 0;
            int numRows = layout.Length;
            int numCols = layout.Max(s=>s.Length);
            Board = new (height:numRows, width:numCols);
            FoodMap = new (height: numRows, width: numCols);

            Ghosts = [
                 new Ghost {Name = TileType.Pinky},
                 new Ghost {Name = TileType.Blinky},
                 new Ghost {Name = TileType.Inky},
                 new Ghost {Name = TileType.Clyde}
            ];

            for (int r = 0; r < numRows; r++)
            {
                for (int c = 0; c < layout[r].Length; c++)
                {
                    var tile = asTile(layout[r][c]);
                    Board[r,c] = tile;
                    FoodMap[r, c] = TileType.Empty;
                    switch (tile)
                    {
                        case TileType.Dot:
                            RemainingDots++;
                            FoodMap[r, c] = TileType.Dot;
                            break;
                        case TileType.PowerPellet:
                            RemainingDots++;
                            FoodMap[r, c] = TileType.PowerPellet;
                            break;
                        case TileType.PacMan:
                            PacmanPosition = new Position(c, r);
                            break;
                        case TileType.Pinky:
                            Ghosts[0].Position = Ghosts[0].InitialPosition = new Position(c, r);
                            break;
                        case TileType.Blinky:
                            Ghosts[1].Position = Ghosts[1].InitialPosition = new Position(c, r);
                            break;
                        case TileType.Inky:
                            Ghosts[2].Position = Ghosts[2].InitialPosition = new Position(c, r);
                            break;
                        case TileType.Clyde:
                            Ghosts[3].Position = Ghosts[3].InitialPosition = new Position(c, r);
                            break;
                        default:
                            break;
                    }
                }
            }

        }

        public TimeSpan ElapsedTime = TimeSpan.Zero;
        private TimeSpan remainingPoweredTime = TimeSpan.Zero;
        public void StartGame()
        {
            timer?.Dispose();
            timer = new PeriodicTimer(TimerInterval);
            Status = GameStatus.Running;
            gameLoop = Task.Run(GameLoop);
        }

        public void StopGame()
        {
            Status = GameStatus.GameOver;
            gameLoop?.Wait();
            timer?.Dispose();
            timer = null;
        }

        private async void GameLoop()
        {
            while (!IsGameOver)
            {
                NextMove();
                NextGhostMove();
                CheckGameOver();
                Updated?.Invoke();
                await timer!.WaitForNextTickAsync();
                ElapsedTime += TimerInterval;
                if (IsPoweredUp)
                {
                    remainingPoweredTime -= TimerInterval;
                    if (remainingPoweredTime <= TimeSpan.Zero)
                    {
                        IsPoweredUp = false;
                        remainingPoweredTime = TimeSpan.Zero;
                    }
                }
            }
        }

        void MoveGhost(Ghost ghost, Position newGhostPosition, Direction newDirection)
        {
            Position ghostPosition = ghost.Position;
            if (IsValidMove(newGhostPosition))
            {
                Board[ghost.Position] = FoodMap[ghost.Position];
                Board[newGhostPosition] = ghost.Name;
                ghost.Position = newGhostPosition;
                ghost.Direction = newDirection;
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
                MoveGhost(ghost,CalculateNewPosition(ghostPosition, ghostDirection),ghostDirection);
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
                remainingPoweredTime = TimeSpan.FromSeconds(5); // Power-up duration
            }

            // Update Pacman's position
            Board[newPosition] = TileType.PacMan;
            PacmanPosition = newPosition;
        }


        private void CheckGameOver()
        {
            if (RemainingDots == 0)
            {
                Status = GameStatus.GameOver | GameStatus.Victory;
                return;
            }
            foreach (var ghost in Ghosts)
            {
                if (ghost.Position == PacmanPosition)
                {
                    if (IsPoweredUp)
                    {
                        Score += 200; // Bonus for eating a ghost
                        MoveGhost(ghost, ghost.InitialPosition, Direction.None); // Reset ghost position
                    }
                    else
                    {
                        Status = GameStatus.GameOver;
                        return;
                    }
                }
            }
        }

        public void Dispose()
        {
            StopGame();
        }
    }
} 