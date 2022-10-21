using System.Collections;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement;

public class FbAuthManager : MonoBehaviour
{
    //Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
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
    public TMP_InputField emailRegisterField;
    public TMP_InputField passwordRegisterField;
    public TMP_InputField passwordRegisterVerifyField;
    public TMP_Text warningRegisterText;

    //User Data variables
    // [Header("UserData")]
    // public TMP_InputField usernameField;
    // public TMP_InputField xpField;
    // public TMP_InputField killsField;
    // public TMP_InputField deathsField;
    // public GameObject scoreElement;
    // public Transform scoreboardContent;

    void Awake()
    {
        // Firebase icin gerekli bilesenler sistemde mevcut olup olmadýðýný kontrol edelim
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                //Varsa, Firebase'i baslatalim
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Tüm Firebase bilesenleri cozulemedi: " + dependencyStatus);
            }
        });
    }

    private void InitializeFirebase()//Firebase'i baslat
    {
        Debug.Log("Firebase Auth Kuruluyor");
        //Kimlik dogrulama nesnesi olusturuyoruz
        auth = FirebaseAuth.DefaultInstance;
       
        //buna bu kisimda gerek yok
        //DBreference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void LoginButton()
    {
        //Login butonuna basildiginda login coroutine calisir
        StartCoroutine(Login(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator Login(string _email, string _password)
    {
        //E-postayý ve þifreyi girerek Firebase auth oturum açma fonksiyonunu cagiralim
        var LoginTask = auth.SignInWithEmailAndPasswordAsync(_email, _password);
        //Islem bitene kadar bekle diyoruz
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted);

        if (LoginTask.Exception != null)
        {
            //Hata varsa nedenini bulmak icin ayristiralim
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
            //Kullanici Giris Yapti
            //Sonucu alalim
            User = LoginTask.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})", User.DisplayName, User.Email);
            warningLoginText.text = "";
            confirmLoginText.text = "Logged In";
            //StartCoroutine(LoadUserData());

            yield return new WaitForSeconds(2);//2 saniye bekle
            confirmLoginText.text = "";

            //Yeni sahneye gecebiliriz

            SceneManager.LoadScene("TestScene");

            //usernameField.text = User.DisplayName;
            //UIManager2.instance.UserDataScreen(); // Change to user data UI

            //ClearLoginFeilds();
            //ClearRegisterFeilds();
        }
    }

    public void RegisterButton()
    {
        //Register butonuna basildiginda Register coroutine calisir
        StartCoroutine(Register(emailRegisterField.text, passwordRegisterField.text, usernameRegisterField.text));
    }
    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //Kullanýcý adý alaný boþsa bir uyarý gösterir
            warningRegisterText.text = "Missing Username";
        }
        else if (passwordRegisterField.text != passwordRegisterVerifyField.text)
        {
            //Þifre eþleþmiyorsa bir uyarý gösterir
            warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //E-postayý ve sifreyi ileten Firebase auth oturum acma fonksiyonunu cagiralim 
            var RegisterTask = auth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Islem bitene kadar bekle diyoruz
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //Hata varsa nedenini bulmak icin ayristiralim
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
                //Kullanici olusturuldu
                //Sonucu alalim
                User = RegisterTask.Result;

                if (User != null)
                {
                    //Bir kullanici profili olusturalim ve kullanici adini ayarlayalim
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    // Profili kullanici adiyla gecen (Firebase auth kullanýcý güncelleme profili fonksiyonunu cagiralim
                    var ProfileTask = User.UpdateUserProfileAsync(profile);
                    //Islem bitene kadar bekle diyoruz
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //Hata var mi bakiyoruz
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        warningRegisterText.text = "Username Set Failed!";
                    }
                    else
                    {
                        //usarname Kaydedildi
                        //Simdi logine veya diger sahneye gecebiliriz
                        warningRegisterText.text = "Basarili"; 


                       // UIManager2.instance.LoginScreen()
                       // ClearRegisterFeilds();
                       // ClearLoginFeilds();
                    }
                }
            }
        }
    }



}
