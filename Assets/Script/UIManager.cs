using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    //Screen object variables
    public GameObject loginUI;
    public GameObject registerUI;
    public GameObject userDataUI;
    public GameObject RecycleUI;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != null)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    private void Start()
    {
       
    }
    public void isTransferable()
    {
        //if (PlayerPrefs.GetString("Account") == "")
        //{
        //    transferButton.interactable = false;
        //    logimMMButton.interactable = true;
        //}

        //else
        //{
        //    logimMMButton.interactable = false;
        //    transferButton.interactable = true;
        //}

    }

    
    //Functions to change the login screen UI

    public void ClearScreen() //Turn off all screens
    {
        loginUI.SetActive(false);
        registerUI.SetActive(false);
        userDataUI.SetActive(false);
        RecycleUI.SetActive(false);
       // scoreboardUI.SetActive(false);
    }

    public void LoginScreen() //Back button
    {
        ClearScreen();
        loginUI.SetActive(true);
    }
    public void RegisterScreen() // Regester button
    {
        ClearScreen();
        registerUI.SetActive(true);
    }

    public void RecycleScreen()
    {
        isTransferable();
        ClearScreen();
        RecycleUI.SetActive(true);
    }
    public void UserDataScreen() //Logged in
    {
        ClearScreen();
       userDataUI.SetActive(true);
    }

}