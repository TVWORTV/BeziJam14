

public class TurnDecider
{
    readonly List<Character> turnQueue = new();
    int currentTurnIndex;

    public void Initialize(List<Character> chars)
    {
        RebuildQueue(chars)
    }

    public void RebuildQueue(List<Character> allCharacters)
    {
        allCharacters.Clear();
        allCharacters.AddRange(characters);

        turnQueue.Clear();

        for (int i = 0; i < allCharacters.Count; i++)
        {
            Character character = allCharacters[i];

            character.initiative =
                character.GetStat(StatType.Speed) + UnityEngine.Random.Range(1, 9);

            turnQueue.Add(character);
        }

        for (int i = 1; i < turnQueue.Count; i++)
        {
            Character character = turnQueue[i];
            int j = i - 1;

            while (j >= 0 &&
                   turnQueue[j].initiative < character.initiative)
            {
                turnQueue[j + 1] = turnQueue[j];
                j--;
            }

            turnQueue[j + 1] = character;
        }

        currentTurnIndex = 0;
    }

    public Character OnTurnStart()
    {
        if (currentTurnIndex >= turnQueue.Count)
            return null;

        return turnQueue[currentTurnIndex];
    }

    public void OnTurnEnd(Character character)
    {
        if (currentTurnIndex >= turnQueue.Count ||
            turnQueue[currentTurnIndex] != character)
            return;

        currentTurnIndex++;

        if (currentTurnIndex >= turnQueue.Count)
            RebuildQueue(allCharacters);
    }
}
