namespace pacman.Models
{
    public interface IGhostStrategy
    {
        Direction[] GetNextMove(GameState game);
    }

    public class RandomGhostStrategy : IGhostStrategy
    {
        private static readonly Random random = new Random();
        private readonly Direction[] possible_directions = Enum.GetValues<Direction>().Where(d=>d!=Direction.None).ToArray();

        static Direction opposite(Direction dir)
        {
            return dir switch
            {
                Direction.Up => Direction.Down,
                Direction.Down => Direction.Up,
                Direction.Left => Direction.Right,
                Direction.Right => Direction.Left,
                _ => throw new ArgumentOutOfRangeException(nameof(dir), dir, null)
            };
        }

        double directionScore(GameState game, Direction dir, int i_ghost)
        {
            Direction curr = game.Ghosts[i_ghost].Direction;
            if (curr == Direction.None) return 1;
            var pos = GameState.CalculateNewPosition(game.Ghosts[i_ghost].Position, curr);
            if (!game.IsValidMove(pos))
                return 1;
            return curr==dir ? 8 : dir == opposite(curr) ? 2 : 5;
        }


        public Direction[] GetNextMove(GameState game)
        {
            var moves = new Direction[game.Ghosts.Length];
            for (int i = 0; i < game.Ghosts.Length; i++)
            {
                double[] scores = possible_directions.Select(d=>directionScore(game,d,i)).ToArray();
                double s_sum= scores.Sum();
                for (int j = 0; j < scores.Length; j++)
                {
                    scores[j] /= s_sum;
                }

                (Direction d, double score)[] probs = possible_directions
                    .Zip(scores)
                    .OrderByDescending(x=>x.Item2)
                    .ToArray();

                double p = random.NextDouble();
                double cumulative = 0.0;
                foreach (var (d, score) in probs)
                {
                    cumulative += score;
                    if (cumulative >= p)
                    {
                        moves[i] = d;
                        break;
                    }
                }
            }
            return moves;
        }
    }
}
