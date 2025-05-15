namespace Shin_Megami_Tensei;

public class SkillData
{
    public string name { get; set; }
    public string type { get; set; }
    public int cost { get; set; }
    public int power { get; set; }
    public string target { get; set; }
    public string hits { get; set; }
    public string effect { get; set; }
    
    public string GetSkillActionVerb(string skillType)
    {
        return skillType switch
        {
            "Phys" => "ataca a",
            "Gun" => "dispara a",
            "Fire" => "lanza fuego a",
            "Ice" => "lanza hielo a",
            "Elec" => "lanza electricidad a",
            "Force" => "lanza viento a",
            _ => "usa una habilidad contra"
        };
    }
    
    public int GetHitsCount(int usedSkillsCount)
    {
        if (string.IsNullOrEmpty(hits))
            return 1;
        if (hits.Contains("-"))
        {
            string[] range = hits.Split('-');
            if (int.TryParse(range[0], out int min) && int.TryParse(range[1], out int max))
            {
                int offset = usedSkillsCount % (max - min + 1);
                return min + offset;
            }
        }
        if (int.TryParse(hits, out int fixedHits))
            return fixedHits;
        return 1;
    }
}