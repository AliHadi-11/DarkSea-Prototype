using UnityEngine;

// Offline achievement system — stored per-user in PlayerPrefs.
// Call AchievementSystem.Unlock("id") from anywhere.
// Display via AchievementSystem.GetAll() in the main menu.
public static class AchievementSystem
{
    public struct Achievement
    {
        public string id;
        public string title;
        public string desc;
    }

    public static readonly Achievement[] All =
    {
        new Achievement { id = "FirstElim",  title = "FIRST BLOOD",    desc = "Eliminate your first enemy"          },
        new Achievement { id = "IronLungs",  title = "IRON LUNGS",     desc = "Finish a level with 60%+ oxygen"     },
        new Achievement { id = "SpeedRun",   title = "SPEED RUNNER",   desc = "Complete a level in under 90 seconds" },
        new Achievement { id = "TankMaster", title = "TANK MASTER",    desc = "Collect all oxygen tanks in Level 2"  },
        new Achievement { id = "DeepDiver",  title = "DEEP DIVER",     desc = "Complete all 3 levels"               },
    };

    public static bool IsUnlocked(string id)
        => PlayerData.GetInt("ACH_" + id, 0) == 1;

    // Call this from gameplay code — silently skips if already unlocked.
    public static void Unlock(string id)
    {
        if (IsUnlocked(id)) return;

        PlayerData.SetInt("ACH_" + id, 1);
        PlayerPrefs.Save();

        // Find the title for the notification
        string title = id;
        foreach (var a in All)
            if (a.id == id) { title = a.title; break; }

        NotificationUI.Show("ACHIEVEMENT: " + title, NotificationUI.NotifType.Kill);
        Debug.Log("[Achievement] Unlocked: " + id);
    }
}
