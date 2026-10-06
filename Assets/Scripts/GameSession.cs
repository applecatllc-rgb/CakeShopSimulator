using System.Collections.Generic;

public static class GameSession
{
    public static int Score { get; private set; }

    public static List<string> Orders { get; } = new List<string>();

    public static void AddScore(int amount)
    {
        Score += amount;
    }

    public static void AddOrder(string content)
    {
        Orders.Add(content);
    }

    public static void Reset()
    {
        Score = 0;
        Orders.Clear();
    }
}