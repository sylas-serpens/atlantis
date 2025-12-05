using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string newGameScene = "StoryOne"; 

    public void OnClick_NewGame()
    {
        SceneManager.LoadScene(newGameScene);
    }

    public void OnClick_LoadGame()
    {
        if (!SaveSystem.HasSave())
        {
            Debug.LogWarning("MainMenu: No save file exists!");
            return;
        }

        SaveData data = SaveSystem.LoadGame();

        GameManager.Instance.LoadFromLastSave();
    }

    public void OnClick_Quit()
    {
        Application.Quit();
    }
}
