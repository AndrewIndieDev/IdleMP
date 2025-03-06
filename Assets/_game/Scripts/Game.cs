public struct Game
{
    public static implicit operator Game(GameIdea a) => new Game(a.Name, a.Genre, (ulong)a.GameDevSteps.Count * DevelopGameManager.GAME_PRICE_PER_STEP);

    public string Name;
    public string Genre;
    public ulong Price;

    public Game(string name, string genre, ulong price)
    {
        Name = name;
        Genre = genre;
        Price = price;
    }
}
