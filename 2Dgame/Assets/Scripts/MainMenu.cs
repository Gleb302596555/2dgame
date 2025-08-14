using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Text globalScoreText;
    [SerializeField] private Button continueButton;

    public void ResetGlobalScore()
    {
        PlayerPrefs.SetInt("GlobalScore", 0);
        PlayerPrefs.Save();
        globalScoreText.text = "0";
    }

    void Start()
    {
        int globalScore = PlayerPrefs.GetInt("GlobalScore", 0);
        globalScoreText.text = globalScore.ToString();

        if (PlayerPrefs.HasKey("LastLevel"))
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    public void Back()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Levels()
    {
        SceneManager.LoadScene("Levels");
    }
    public void LevelOne()
    {
        SceneManager.LoadScene("LevelOne");
    }
    public void LevelTwo()
    {
        SceneManager.LoadScene("LevelTwo");
    }
    public void LevelThree()
    {
        SceneManager.LoadScene("LevelThree");
    }

    public void LevelFour()
    {
        SceneManager.LoadScene("LevelFour");
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("LevelOne");
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }
    public void ContinueLastLevel()
    {
        string lastLevel = PlayerPrefs.GetString("LastLevel", "LevelOne");
        SceneManager.LoadScene(lastLevel);
    }
}
