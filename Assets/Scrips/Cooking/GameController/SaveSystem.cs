using UnityEngine;

public static class SaveSystem
{
    private const string SAVE_KEY = "PlayerSaveExists";
    private const string POS_X = "PlayerX";
    private const string POS_Y = "PlayerY";
    private const string POS_Z = "PlayerZ";

    public static void SaveGame(Vector3 position)
    {
        PlayerPrefs.SetInt(SAVE_KEY, 1);
        PlayerPrefs.SetFloat(POS_X, position.x);
        PlayerPrefs.SetFloat(POS_Y, position.y);
        PlayerPrefs.SetFloat(POS_Z, position.z);
        PlayerPrefs.Save();
    }

    public static bool HasSave()
    {
        return PlayerPrefs.GetInt(SAVE_KEY, 0) == 1;
    }

    public static Vector3 LoadPosition()
    {
        float x = PlayerPrefs.GetFloat(POS_X);
        float y = PlayerPrefs.GetFloat(POS_Y);
        float z = PlayerPrefs.GetFloat(POS_Z);
        return new Vector3(x, y, z);
    }
}
