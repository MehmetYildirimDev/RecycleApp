using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;

//logoya tiklayinca admin paneli acilsin

public class FirebaseManager : MonoBehaviour
{

    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth Auth;
    public FirebaseUser User;
    public DatabaseReference DBreference;


    //Login variables
    [Header("Login")]
    public TMP_InputField emailLoginField;
    public TMP_InputField passwordLoginField;
    public TMP_Text warningLoginText;
    public TMP_Text confirmLoginText;

    //Register variables
    [Header("Register")]
    public TMP_InputField usernameRegisterField;
    public TMP_InputField AddressRegisterField;
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //User Data variables
    [Header("UserData")]
    public TMP_InputField usernameField;
    public TMP_InputField AddressField;

    [Header("RecycleItemData")]
    public TMP_Text usernameTextRYC;
    public TMP_Text addressTextRYC;
    public TMP_Text itemTextRYC;
    public TMP_Text ucretTextRYC;
    public TMP_Dropdown dropdownRYC;

    public List<string> ItemNameList = new List<string>();
    public List<string> ItemucretList = new List<string>();

    [SerializeField] private Web3WalletTransfer20Example web3;

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

    private void Start()
    {
        dropdownRYC.onValueChanged.AddListener(delegate { DropDownItemSelected(dropdownRYC); });
    }

    void DropDownItemSelected(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;

        //textBox.text = dropdown.options[index].text;

        ucretTextRYC.text = ItemucretList[index];


        //web3.OnTransfer20((Convert.ToInt32(ItemucretList[index])*10000000000000).ToString());
        // amount = ItemucretList[index];
    }

    public void TRANSFER_Click_BTN()
    {
        double amount = Convert.ToDouble(ItemucretList[dropdownRYC.value]) * 1000000000;

        if (PlayerPrefs.GetString("Account") != "")
        {
           web3.OnTransfer20(amount.ToString(), PlayerPrefs.GetString("Account"));
        }
        else
        {
            Debug.Log("Adres bulunamadi");
        }


    }


    private void InitializeFirebase()
    {
        Debug.Log("Setting up Firebase Auth");
        //Set the authentication instance object
        Auth = FirebaseAuth.DefaultInstance;
        DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }
    public void ClearLoginFeilds()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }
    public void ClearRegisterFeilds()
    {
        usernameRegisterField.text = "";
        emailRegisterField.text = "";
        passwordRegisterField.text = "";
        passwordRegisterVerifyField.text = "";
    }

    //Function for the login button
    public void LoginButton()
    {
        //Call the login coroutine passing the email and password
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }
    //Function for the register button
    public void RegisterButton()
    {
        //Call the register coroutine passing the email, password, and username
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text, AddressRegisterField.text));
        //   StartCoroutine(UpdateAddress(AddressRegisterField.text));
    }
    //Function for the sign out button
    public void SignOutButton()
    {
        Auth.SignOut();
        UIManager.instance.LoginScreen();
        ClearRegisterFeilds();
        ClearLoginFeilds();
    }
    //Function for the save button
    public void SaveDataButton()
    {
        StartCoroutine(UpdateUsernameAuth(usernameField.text));
        StartCoroutine(UpdateUsernameDatabase(usernameField.text));

        StartCoroutine(UpdateAddress(AddressField.text));
        //        StartCoroutine(UpdateXp(int.Parse(xpField.text)));

    }
    //Function for the scoreboard button
    //public void ScoreboardButton()
    //{
    //    StartCoroutine(LoadScoreboardData());
    //}

    private IEnumerator Login(string _email, string _password)
    {
        //Call the Firebase auth signin function passing the email and password
        var LoginTask = Auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //If there are errors handle them
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed!";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "Account does not exist";
                    break;
            }
            warningLoginText.text = message;
        }
        else
        {
            //User is now logged in
            //Now get the result
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";

            StartCoroutine(RecycleLoadData());              //StartCoroutine(LoadUserData());
            yield return new WaitForSeconds(2);

            //usernameField.text = User.DisplayName;
            UIManager.instance.RecycleScreen();             //UIManager.instance.UserDataScreen(); // Change to user data UI
            confirmLoginText.text = "";
            ClearLoginFeilds();
            ClearRegisterFeilds();
        }
    }

    private IEnumerator Register(string _email, string _password, string _username, string _address)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = Auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);


                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        warningRegisterText.text = "";
                        ClearRegisterFeilds();
                        ClearLoginFeilds();
                    }

                    //adress
                    var DBTask = DBreference.Child("users").Child(User.UserId).Child("address").SetValueAsync(_address);

                    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

                    if (DBTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
                    }
                    else
                    {
                        //Xp is now updated
                    }


                    //username
                    //Set the currently logged in user username in the database
                    var DBTask1 = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

                    yield return new WaitUntil(predicate: () => DBTask1.IsCompleted);

                    if (DBTask.Exception != null)
                    {
                        Debug.LogWarning(message: $"Failed to register task with {DBTask1.Exception}");
                    }
                    else
                    {
                        //Database username is now updated
                    }
                }
            }
        }
    }

    private IEnumerator UpdateUsernameAuth(string _username)
    {
        //Create a user profile and set the username
        UserProfile profile = new UserProfile { DisplayName = _username };

        //Call the Firebase auth update user profile function passing the profile with the username
        var ProfileTask = User.UpdateUserProfileAsync(profile);
        //Wait until the task completes
        yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

        if (ProfileTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
        }
        else
        {
            //Auth username is now updated
        }
    }

    private IEnumerator UpdateUsernameDatabase(string _username)
    {
        //Set the currently logged in user username in the database
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("username").SetValueAsync(_username);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Database username is now updated
        }
    }

    private IEnumerator UpdateAddress(string _address)
    {
        // DBreference.Child("users").Child(User.UserId).SetValueAsync(User.UserId) ;
        var DBTask = DBreference.Child("users").Child(User.UserId).Child("address").SetValueAsync(_address);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else
        {
            //Xp is now updated
        }

    }
    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            AddressField.text = "deger yok";
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            AddressField.text = snapshot.Child("address").Value.ToString();
        }
    }

    private IEnumerator RecycleLoadData()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("users").Child(User.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet

        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            //AddressField.text = snapshot.Child("address").Value.ToString();

            usernameTextRYC.text = snapshot.Child("username").Value.ToString();
            addressTextRYC.text = snapshot.Child("address").Value.ToString();
        }

        StartCoroutine(itemGet());//dropdown degeri
    }

    private IEnumerator itemGet()
    {
        //Get the currently logged in user data
        var DBTask = DBreference.Child("Items").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;
            //AddressField.text = snapshot.Child("address").Value.ToString();

            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                ItemNameList.Add(childSnapshot.Child("Name").Value.ToString());
                ItemucretList.Add(childSnapshot.Child("ucreti").Value.ToString());
            }


            Debug.Log(ItemNameList.Count);
            itemTextRYC.text = ItemNameList[0];
            ucretTextRYC.text = ItemucretList[0];
            dropdownRYC.ClearOptions();
            dropdownRYC.AddOptions(ItemNameList);
        }
    }


    //private IEnumerator UpdateXp(int _xp)
    //{
    //    //Set the currently logged in user xp
    //    var DBTask = DBreference.Child("users").Child(User.UserId).Child("xp").SetValueAsync(_xp);

    //    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    //    if (DBTask.Exception != null)
    //    {
    //        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
    //    }
    //    else
    //    {
    //        //Xp is now updated
    //    }
    //}







    //private IEnumerator LoadScoreboardData()
    //{
    //    //Get all the users data ordered by kills amount
    //    var DBTask = DBreference.Child("users").OrderByChild("kills").GetValueAsync();//kile gore siralayacak

    //    yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

    //    if (DBTask.Exception != null)
    //    {
    //        Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
    //    }
    //    else
    //    {
    //        //Data has been retrieved
    //        DataSnapshot snapshot = DBTask.Result;

    //        //Destroy any existing scoreboard elements
    //        foreach (Transform child in scoreboardContent.transform)
    //        {
    //            Destroy(child.gameObject);
    //        }

    //        //Loop through every users UID
    //        foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
    //        {
    //            string username = childSnapshot.Child("username").Value.ToString();
    //            int kills = int.Parse(childSnapshot.Child("kills").Value.ToString());
    //            int deaths = int.Parse(childSnapshot.Child("deaths").Value.ToString());
    //            int xp = int.Parse(childSnapshot.Child("xp").Value.ToString());

    //            //Instantiate new scoreboard elements
    //            GameObject scoreboardElement = Instantiate(scoreElement, scoreboardContent);
    //            scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, kills, deaths, xp);
    //        }

    //        //Go to scoareboard screen
    //        UIManager.instance.ScoreboardScreen();
    //    }
    //}
}