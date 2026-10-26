
public class GameProgressionManager
{
    public List<GameProgressionLevel> levels = new(); 
    public int currentIndex = 0; 

    public void Proceed()
    {
        currentIndex += 1; 
    }
    public void Load(int index)
    {
        currentIndex = index; 
    }
}


public class GameProgressionLevel 
{
    public FullTextAsset startingText;
    public BattleData battle;  
    public FullTextAsset endingText; 
}


[CreateAssetMenu(menuName = "BattleData")]
public class BattleData : ScriptableObject
{
    public List<CharacterData> enemyCharacters = new(); 
}