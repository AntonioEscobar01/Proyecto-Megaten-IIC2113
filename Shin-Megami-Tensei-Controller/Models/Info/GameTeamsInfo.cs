namespace Shin_Megami_Tensei;

public class GameTeamsInfo
{
    public Team Team1 { get; }
    public Team Team2 { get; }
    
    public GameTeamsInfo(Team team1, Team team2)
    {
        Team1 = team1;
        Team2 = team2;
    }
}