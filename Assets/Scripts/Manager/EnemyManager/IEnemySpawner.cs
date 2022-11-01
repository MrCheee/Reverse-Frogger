interface IEnemySpawner
{
    void SetDifficulty(string difficultyLevel);
    void SpawnEnemies();
    void IncrementLevel();
    void ReduceLevel();
}