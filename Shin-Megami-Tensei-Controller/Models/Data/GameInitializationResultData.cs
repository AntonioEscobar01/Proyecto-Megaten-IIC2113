namespace Shin_Megami_Tensei;

public class GameInitializationResultData
{
    public Team? TeamPlayer1 { get; }
    public Team? TeamPlayer2 { get; }
    public bool IsSuccessful { get; }

    public GameInitializationResultData(Team? teamPlayer1, Team? teamPlayer2, bool isSuccessful)
    {
        TeamPlayer1 = teamPlayer1;
        TeamPlayer2 = teamPlayer2;
        IsSuccessful = isSuccessful;
    }
}