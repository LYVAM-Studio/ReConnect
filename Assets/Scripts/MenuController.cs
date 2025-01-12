using System;
using Mirror;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    enum Menu { Main, Multiplayer, Settings }
    [Header("Menus canvas")]
    public GameObject mainMenu;
    public GameObject multiplayerMenu;
    public GameObject settingsMenu;
    [Header("Player prefab")]
    public GameObject playerPrefab;

    private Menu _currentMenu;
    public void Start()
    {
        SetActiveMenu(Menu.Main);
    }

    // Activates the given menu and deactivates all the others
    private void SetActiveMenu(Menu menu)
    {
        _currentMenu = menu;
        mainMenu.SetActive(menu == Menu.Main);
        multiplayerMenu.SetActive(menu == Menu.Multiplayer);
        settingsMenu.SetActive(menu == Menu.Settings);
    }

    public void PlaySingleplayer()
    {
        // SceneManager.LoadScene("GameScene");
        NetworkManager.singleton.StartHost();
    }

    public void PlayMultiplayer()
    {
        SetActiveMenu(Menu.Multiplayer);
    }

    public void OpenSettings()
    {
        SetActiveMenu(Menu.Settings);
    }

    public void BackToMainMenu()
    {
        SetActiveMenu(Menu.Main);
    }

    public void JoinServer()
    {
        throw new NotImplementedException();
        var serverAdress = multiplayerMenu.GetComponentInChildren<InputField>().text;
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
