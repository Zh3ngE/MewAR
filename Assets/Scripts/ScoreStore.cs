using UnityEngine;

public static class ScoreStore
{
    const string PreKey = "PreScore";
    const string PostKey = "PostScore";

    public static bool HasPre  => PlayerPrefs.HasKey(PreKey);
    public static bool HasPost => PlayerPrefs.HasKey(PostKey);
    public static int Pre  => PlayerPrefs.GetInt(PreKey, 0);
    public static int Post => PlayerPrefs.GetInt(PostKey, 0);

    public static void SavePre(int score)  { PlayerPrefs.SetInt(PreKey, score);  PlayerPrefs.Save(); }
    public static void SavePost(int score) { PlayerPrefs.SetInt(PostKey, score); PlayerPrefs.Save(); }

    public static void MarkLesson(int index)
    {
        PlayerPrefs.SetInt("Lesson" + index, 1);
        PlayerPrefs.Save();
    }

    public static int LessonsDone
    {
        get
        {
            int c = 0;
            for (int i = 0; i < 5; i++)
                if (PlayerPrefs.GetInt("Lesson" + i, 0) == 1) c++;
            return c;
        }
    }

    public static void ResetAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
