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

public class GameSceneManager : MonoBehaviour
{

    [Header("Oyun Panel UI")]

    [SerializeField] Text NameText;
    [SerializeField] Text LastNameText;
    [SerializeField] Text expText;
    [SerializeField] Text levelText;
    [SerializeField] Text AdressText;
    [SerializeField] Text AllPaymentText;

    //[SerializeField] InputField isimDegisInput;
    [SerializeField] TMP_InputField isimDegisInput;

    [Header("Loading Panel UI")]
    [SerializeField] Text loadingText;

    [Header("Panels")]
    [SerializeField] GameObject gamePanel;
    [SerializeField] GameObject loadinPanel;

    FirebaseAuth auth;
    DatabaseReference reference;

    Coroutine loadingCoroutine;
    string loadingString = "Loading";
    int timer = 0;
    bool loaded = false;

    void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
    }
    void Start()
    {
        if (auth.CurrentUser == null)
        {
            SceneManager.LoadScene("LoginScene");
        }
        else
        {
            loadingCoroutine = StartCoroutine(Loading());
            //Database

            reference = FirebaseDatabase.DefaultInstance.RootReference;//veritabanin refaransini tuttugumuz degisken
            VerileriCek();
        }
    }

    IEnumerator Loading()
    {
        loaded = false;
        gamePanel.SetActive(false);
        loadinPanel.SetActive(true);
        while (!loaded)
        {

            timer += 1;
            loadingText.text = loadingString + new String('.', timer);

            if (timer > 3)
                timer = 0;
            yield return new WaitForSeconds(.1f);
        }
        gamePanel.SetActive(true);
        loadinPanel.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))//yenileme tusu gibi dusunebiliriz
        {
            VerileriCek();
        }
    }

    public void IsimGuncelle()
    {
        if (isimDegisInput.text != null || isimDegisInput.text != "" || isimDegisInput.text != NameText.text)
        {
            //sadece bir yerin degerini degistirme
            reference.Child("OyunVerileri").Child(auth.CurrentUser.UserId).Child("Name").SetValueAsync(isimDegisInput.text);
        }
    }

    void VerileriCek()
    {
        loadingCoroutine = StartCoroutine(Loading()); //yuklenirken kullanici bir seylerle ugrasmasin

        //Child bi alt basliga inmek olarak dusunelim
        //ve childine kullanicimizin idsini veriyoruz(ona ozel olan) 
        reference.Child("OyunVerileri").Child(auth.CurrentUser.UserId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted)
            {
                Debug.Log("Database Hata");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
                if (snapshot.GetRawJsonValue() == null)//bu bos ise henuz veri kaydi olusturmamisiz demek
                {
                    Debug.Log("Boþ");
                    BosVeriOlustur();//baslangic verisi olusturuyoruz
                    VerileriCek();
                }
                else
                {
                    //varsa direk cekiyoruz
                    Debug.Log(snapshot.GetRawJsonValue());
                    //jsondan objeye - objeden jsona cevirme
                    PersonelData data = JsonUtility.FromJson<PersonelData>(snapshot.GetRawJsonValue());
                    NameText.text = "Adý: " + data.Name;
                    LastNameText.text = "LastName: " + data.LastName;
                    expText.text = "Exp: " + data.exp;
                    levelText.text = "Level: " + data.level;
                    AdressText.text = "adress: " +data.Adress;
                    AllPaymentText.text = data.AllPayment.ToString();
                    

                    loaded = true;
                }
            }
        });
    }

    void BosVeriOlustur()
    {
        PersonelData bosveri = new PersonelData
        {
            Name = "TestName",
            LastName = "TestLastName",
            exp = 0,
            level = 1,
            Adress = "",
            AllPayment = 0f
            
        };
        //jsona ceviriyoruz burada 
        string bosJson = JsonUtility.ToJson(bosveri);
        //database yazdiriyoruz
        reference.Child("OyunVerileri").Child(auth.CurrentUser.UserId).SetRawJsonValueAsync(bosJson);
    }

    public void CikisYap()
    {
        auth.SignOut();//current useri null yapiyo
        SceneManager.LoadScene("LoginScene");
    }
}

