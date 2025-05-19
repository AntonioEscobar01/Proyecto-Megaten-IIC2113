// ...existing code...

public int GetHitsCount()
{
    if (IsMultiHitSkill())
    {
        return GetRandomHitCount();
    }
    
    return 1;
}

private bool IsMultiHitSkill()
{
    return Name.Contains("Multi") || Name.Contains("multi");
}

private int GetRandomHitCount()
{
    Random random = new Random();
    return random.Next(2, 5);
}

// ...existing code...
