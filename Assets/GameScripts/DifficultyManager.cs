using UnityEngine;

public enum Difficulty { Easy = 0, Normal = 1, Hard = 2 }

// Static access anywhere: DifficultyManager.Current, DifficultyManager.Set(d)
public static class DifficultyManager
{
    const string PREF_KEY = "Difficulty";

    public static Difficulty Current => (Difficulty)PlayerPrefs.GetInt(PREF_KEY, 1);

    public static float OxygenDrainMultiplier
    {
        get
        {
            switch (Current)
            {
                case Difficulty.Easy: return 0.55f;
                case Difficulty.Hard: return 1.45f;
                default:              return 1.00f;
            }
        }
    }

    public static float EnemySpeedMultiplier
    {
        get
        {
            switch (Current)
            {
                case Difficulty.Easy: return 0.65f;
                case Difficulty.Hard: return 1.35f;
                default:              return 1.00f;
            }
        }
    }

    public static void Set(Difficulty d)
    {
        PlayerPrefs.SetInt(PREF_KEY, (int)d);
        PlayerPrefs.Save();
    }
}
