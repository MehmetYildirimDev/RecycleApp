using System.Collections;
using System.Collections.Generic;
using Firebase;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Database;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{

    [Header("Giris Yap UI")]
    [SerializeField] InputField girisEmail;
    [SerializeField] InputField girisSifre;

    [Header("Uye Ol UI")]
    [SerializeField] InputField uyeOlEmail;
    [SerializeField] InputField uyeOlSifre;
    [SerializeField] InputField uyeOlSifreKontrol;
    [SerializeField] InputField uyeOlName;
    [SerializeField] InputField uyeOlLastName;
    [SerializeField] InputField uyeOlAdress;    

    //kullanici islemleri icin bir degisken
    FirebaseAuth auth;
    DatabaseReference reference;


    void Awake()
    {
        //var olan sekilde calisan yapisini aliyoruz
        auth = FirebaseAuth.DefaultInstance;
    }
    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;//veritabanin refaransini tuttugumuz degisken
        //kullanici giris ya da cikis yapti ise burasi calisiyo //kullanici degisince burasi calisiyo
        auth.StateChanged += AuthStateChange;
        
        //buraa bir daha yapiyoruz
        if (auth.CurrentUser != null)
        {
             SceneManager.LoadScene("MainScene");
            //auth.SignOut();
        }
    }

    void AuthStateChange(object sender, System.EventArgs eventArgs)
    {
        //mevcut kullanici bos degilse uygulamamiza gecelim diyo
        if (auth.CurrentUser != null)
        {
            SceneManager.LoadScene("MainScene");
        }
    }

    public void UyeOl()
    {
        if (UyeOlVeriKontrol())
        {
            //kayit ediyoruz asecron            ContinueWith olan sadece arka planda calisiyo ve sikinti yaratiyor///main thread daha iyi ///sahne gecisleri iyi calismiyo
            auth.CreateUserWithEmailAndPasswordAsync(uyeOlEmail.text, uyeOlSifre.text).ContinueWithOnMainThread(task => {//islem task degiskeninde tutuluyo 
                if (task.IsCanceled)//kontrolleri yapiliyo
                {
                    Debug.Log("Ýptal Edildi");
                    return;
                }
                if (task.IsFaulted)
                {
                   // Debug.Log("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                    return;
                }

                Firebase.Auth.FirebaseUser newUser = task.Result;
            //    Debug.LogFormat("Firebase user created successfully: {0} ({1})",
            //        newUser.DisplayName, newUser.UserId);

                //burada yapmaliyim islemleri
                PersonelData oyunData = new PersonelData();
                oyunData.Name = uyeOlName.text;
                oyunData.LastName = uyeOlLastName.text;
                oyunData.Adress = uyeOlAdress.text;
                oyunData.AllPayment = 0f;

                //jsona ceviriyoruz burada 
                string FirstJson = JsonUtility.ToJson(oyunData);
                //database yazdiriyoruz
                reference.Child("OyunVerileri").Child(auth.CurrentUser.UserId).SetRawJsonValueAsync(FirstJson);

            });

            
        }
        else
        {
            Debug.Log("Alanlar hatalý");
        }
    }

    bool UyeOlVeriKontrol()
    {

        if (uyeOlEmail.text == null || uyeOlEmail.text == "")
        {
            return false;
        }
        if (uyeOlSifre.text == null || uyeOlSifre.text == "" || uyeOlSifreKontrol.text == null || uyeOlSifre.text == "")
        {
            return false;
        }

        if (uyeOlSifre.text != uyeOlSifreKontrol.text)
        {
            return false;
        }

        return true;
    }
    public void UyeGirisi()
    {
        if (auth.CurrentUser == null)
        {
            if (GirisVeriKontrol())
            {
                auth.SignInWithEmailAndPasswordAsync(girisEmail.text, girisSifre.text).ContinueWith(task => {//giris icin kontrol ediyo
                    if (task.IsCanceled)
                    {
                        Debug.Log("SignInWithEmailAndPasswordAsync was canceled.");
                        return;
                    }
                    if (task.IsFaulted)
                    {
                        Debug.Log("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                        return;
                    }

                    Firebase.Auth.FirebaseUser newUser = task.Result;//kullaniciyi degistiriyoruz
                    Debug.LogFormat("User signed in successfully: {0} ({1})",
                        newUser.DisplayName, newUser.UserId);

                });
            }
        }
       
    }
    //orjinali
    //public void UyeGirisi()
    //{

    //    if (GirisVeriKontrol())
    //    {
    //        auth.SignInWithEmailAndPasswordAsync(girisEmail.text, girisSifre.text).ContinueWith(task => {//giris icin kontrol ediyo
    //            if (task.IsCanceled)
    //            {
    //                Debug.Log("SignInWithEmailAndPasswordAsync was canceled.");
    //                return;
    //            }
    //            if (task.IsFaulted)
    //            {
    //                Debug.Log("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
    //                return;
    //            }

    //            Firebase.Auth.FirebaseUser newUser = task.Result;//kullaniciyi degistiriyoruz
    //            Debug.LogFormat("User signed in successfully: {0} ({1})",
    //                newUser.DisplayName, newUser.UserId);

    //        });
    //    }
    //}
    bool GirisVeriKontrol()
    {

        if (girisEmail.text == null || girisEmail.text == "")
        {
            return false;
        }
        if (girisSifre.text == null || girisSifre.text == "")
        {
            return false;
        }
        return true;
    }

 
}