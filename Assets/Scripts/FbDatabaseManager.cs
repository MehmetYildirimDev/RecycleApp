using System;
using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class FbDatabaseManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth Auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;

    [Header("Data")]
    public TMP_Text UsernameText;

    void Awake()
    {
        //Check that all of the necessary dependencies for Firebase are present on the system
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //If they are avalible Initialize Firebase
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Firabase baglaniliyo");
        //Set the authentication instance object
        Auth= FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }



    public void SignOutButton()
    {
        Auth.SignOut();
        SceneManager.LoadScene("LoginScene");

    }

    private void Start()
    {
        if (Auth.CurrentUser == null)
        {
            Debug.Log("currentuser yok");
        }
        else
        {
            Debug.Log(Auth.CurrentUser.DisplayName);
            UsernameText.text = Auth.CurrentUser.DisplayName;
        }
    }







}
