using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{

    public GameObject mainMenu;
    public GameObject optionsMenu;
    public void PlaySolo()
    {
        //SceneManager.LoadScene("SoloScene");
    }

    public void PlayCoop()
    {
        //SceneManager.LoadScene("CoopScene");
    }

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void CloseOptions()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }


    public void QuitGame()
    {
        Application.Quit();
    }
}
