using System;
using System.Collections.Generic;
using System.Text;

internal static class Program
{
    private const int Width = 42;
    private const int Height = 20;
    private const int AlienCols = 9;
    private const int AlienRows = 4;

    private static readonly List<int[]> Aliens = new List<int[]>();
    private static int formationX;
    private static int formationY;
    private static int direction = 1;
    private static int dropTimer;
    private static int speed;
    private static int playerX;
    private static int playerY = Height - 2;
    private static readonly List<int[]> Bullets = new List<int[]>();
    private static readonly List<int[]> Bombs = new List<int[]>();
    private static readonly List<int[]> Shields = new List<int[]>();
    private static int score;
    private static int lives = 3;
    private static int wave = 1;
    private static bool over;
    private static int shootCooldown;
    private static readonly Random Rng = new Random();

    private static void Main()
    {
        Term.Setup("Space Invaders");
        bool again = true;
        while (again)
        {
            NewWave();
            again = Play();
        }

        Term.Line();
        Term.Line(Term.Yellow + "  Final score " + score + "   Wave " + wave + Term.Reset);
        Term.AnyKey("Press any key to exit...");
    }

    private static void NewWave()
    {
        Aliens.Clear();
        for (int r = 0; r < AlienRows; r++)
        {
            for (int c = 0; c < AlienCols; c++)
            {
                Aliens.Add(new int[] { formationX + c * 4, formationY + r * 2, 1 + ((r + c) % 3) });
            }
        }
        direction = 1;
        dropTimer = 0;
        speed = Math.Max(6, 30 - wave * 3);
        playerX = Width / 2;
        Bullets.Clear();
        Bombs.Clear();
    }

    private static void BuildShields()
    {
        Shields.Clear();
        int[] columns = { 8, 16, 24, 32 };
        foreach (int col in columns)
        {
            for (int x = 0; x < 4; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    Shields.Add(new int[] { col + x, Height - 5 + y });
                }
            }
        }
    }

    private static bool Play()
    {
        BuildShields();
        long last = DateTime.UtcNow.Ticks;
        Term.ClearScreen();

        while (!over)
        {
            while (Term.KeyAvailable)
            {
                ConsoleKeyInfo? key = Term.Key();
                if (!key.HasValue) break;
                if (key.Value.Key == ConsoleKey.LeftArrow) playerX = Math.Max(1, playerX - 1);
                else if (key.Value.Key == ConsoleKey.RightArrow) playerX = Math.Min(Width - 2, playerX + 1);
                else if (key.Value.Key == ConsoleKey.Space && shootCooldown <= 0)
                {
                    Bullets.Add(new int[] { playerX, playerY - 1 });
                    shootCooldown = 8;
                }
            }

            long now = DateTime.UtcNow.Ticks;
            if (now - last >= TimeSpan.TicksPerMillisecond * 40)
            {
                last = now;
                Step();
            }

            Render();
            Term.Sleep(20);
        }

        Render();
        Term.Line();
        Term.Line(Term.BrightRed + "  The invaders got through! Score " + score + Term.Reset);
        ConsoleKeyInfo? again = Term.WaitKey("  Press Enter to play again, Q to quit...");
        return again.HasValue && again.Value.Key != ConsoleKey.Q && again.Value.Key != ConsoleKey.Escape;
    }

    private static void Step()
    {
        if (shootCooldown > 0) shootCooldown--;

        bool edge = false;
        foreach (int[] alien in Aliens)
        {
            if (alien[0] <= 1 && direction < 0) edge = true;
            if (alien[0] >= Width - 3 && direction > 0) edge = true;
        }

        if (edge)
        {
            direction = -direction;
            foreach (int[] alien in Aliens) alien[1] += 1;
            dropTimer = 0;
        }
        else
        {
            foreach (int[] alien in Aliens) alien[0] += direction;
            dropTimer++;
            if (dropTimer >= speed)
            {
                dropTimer = 0;
                foreach (int[] alien in Aliens) alien[1] += 1;
            }
        }

        bool reachedBottom = false;
        foreach (int[] alien in Aliens)
        {
            if (alien[1] >= playerY - 1) reachedBottom = true;
        }
        if (reachedBottom)
        {
            lives = 0;
            over = true;
            return;
        }

        for (int i = Bullets.Count - 1; i >= 0; i--)
        {
            Bullets[i][1] -= 1;
            if (Bullets[i][1] < 1)
            {
                Bullets.RemoveAt(i);
                continue;
            }
            bool hit = false;
            for (int a = Aliens.Count - 1; a >= 0; a--)
            {
                if (Aliens[a][0] == Bullets[i][0] && Aliens[a][1] == Bullets[i][1])
                {
                    score += (4 - Aliens[a][2]) * 5 + 10;
                    Aliens.RemoveAt(a);
                    Bullets.RemoveAt(i);
                    hit = true;
                    break;
                }
            }
            if (hit) continue;
        }

        if (Aliens.Count == 0)
        {
            wave++;
            NewWave();
            BuildShields();
            Term.Line(Term.BrightGreen + "  Wave " + wave + " cleared!" + Term.Reset);
            Term.Sleep(900);
            return;
        }

        if (Rng.Next(24) == 0 && Bombs.Count < 2)
        {
            int[] shooter = Aliens[Rng.Next(Aliens.Count)];
            Bombs.Add(new int[] { shooter[0], shooter[1] + 1 });
        }

        for (int i = Bombs.Count - 1; i >= 0; i--)
        {
            Bombs[i][1] += 1;
            if (Bombs[i][1] > Height - 1)
            {
                Bombs.RemoveAt(i);
                continue;
            }
            bool blocked = false;
            for (int s = Shields.Count - 1; s >= 0; s--)
            {
                if (Shields[s][0] == Bombs[i][0] && Shields[s][1] == Bombs[i][1])
                {
                    Shields.RemoveAt(s);
                    blocked = true;
                    break;
                }
            }
            if (blocked) { Bombs.RemoveAt(i); continue; }

            if (Bombs[i][0] == playerX && Bombs[i][1] == playerY)
            {
                Bombs.RemoveAt(i);
                lives--;
                if (lives <= 0)
                {
                    over = true;
                    return;
                }
                playerX = Width / 2;
                Bombs.Clear();
                Term.Sleep(500);
                return;
            }
        }
    }

    private static string AlienArt(int type)
    {
        if (type == 3) return "  ^  ";
        if (type == 2) return " [] ";
        return " <> ";
    }

    private static void Render()
    {
        char[,] board = new char[Height, Width];
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++) board[y, x] = ' ';
        }

        foreach (int[] alien in Aliens)
        {
            if (alien[1] < 0 || alien[1] >= Height) continue;
            if (alien[0] < 0 || alien[0] >= Width) continue;
            board[alien[1], alien[0]] = 'A';
        }
        foreach (int[] bullet in Bullets)
        {
            if (bullet[1] >= 0 && bullet[1] < Height) board[bullet[1], bullet[0]] = '|';
        }
        foreach (int[] bomb in Bombs)
        {
            if (bomb[1] >= 0 && bomb[1] < Height) board[bomb[1], bomb[0]] = 'v';
        }
        foreach (int[] shield in Shields)
        {
            if (shield[1] >= 0 && shield[1] < Height) board[shield[1], shield[0]] = 'H';
        }
        board[playerY, playerX] = '^';

        StringBuilder sb = new StringBuilder();
        for (int y = 0; y < Height; y++)
        {
            for (int x = 0; x < Width; x++)
            {
                char c = board[y, x];
                if (c == 'A') sb.Append(Term.BrightGreen).Append('A').Append(Term.Reset);
                else if (c == '|') sb.Append(Term.BrightYellow).Append('|').Append(Term.Reset);
                else if (c == 'v') sb.Append(Term.BrightMagenta).Append('v').Append(Term.Reset);
                else if (c == 'H') sb.Append(Term.BrightGreen).Append('H').Append(Term.Reset);
                else if (c == '^') sb.Append(Term.BrightCyan).Append('^').Append(Term.Reset);
                else sb.Append(c);
            }
            sb.Append('\n');
        }

        sb.Append(Term.Yellow).Append("  Score ").Append(score)
          .Append("   Lives ").Append(lives)
          .Append("   Wave ").Append(wave).Append(Term.Reset).Append('\n');

        Term.ClearScreen();
        Term.Write(sb.ToString());
    }
}
