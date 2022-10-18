using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System;

public class DBManager : MonoBehaviour
{
    public DatabaseReference usersReference;

    public InputField usernameInput;
    public InputField passwordInput;


    private void Start()
    {
        StartCoroutine(Initialization());
    }

    private IEnumerator Initialization()
    {
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.Log("DataBase error: " + task.Exception);
        }


        var dependenStatus = task.Result;

        if (dependenStatus == DependencyStatus.Available)
        {
            usersReference = FirebaseDatabase.DefaultInstance.GetReference("Users");
            Debug.Log("init complate");
        }
        else
        {
            Debug.Log("db error");        }

    }

    public void SaveUser()
    {
        string username = usernameInput.text;
        string password= passwordInput.text;
        //dictionary : key, value
        Dictionary<string, object> user = new Dictionary<string, object>();
        user["username"] = username;
        user["password"] = password;

        string key = usersReference.Push().Key;

        usersReference.Child(key).UpdateChildrenAsync(user);
    }

    public void getData()
    {
        StartCoroutine(GetUserData());
    }

    public IEnumerator GetUserData()
    {
        string name = usernameInput.text;
        var task = usersReference.Child(name).GetValueAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.IsCanceled || task.IsFaulted)
        {
            Debug.Log("DataBase error: " + task.Exception);
            yield break;
        }

        DataSnapshot snapshot = task.Result;

        foreach (DataSnapshot user in snapshot.Children)
        {
            if (user.Key == "password")
            {
                Debug.Log("password:" + user.Value.ToString()); 
            }
            if (user.Key=="username")
            {
                Debug.Log("Username:" + user.Value.ToString());
            }
        }

    }


}
