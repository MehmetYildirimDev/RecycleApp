using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
public class DropDownScript : MonoBehaviour
{
    public Text textBox;
    public TMP_Dropdown dropdown;

    DatabaseReference reference;


    public List<string> items;
    public List<string> keyList;
    private void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
 

        
        dropdown.options.Clear();


        //Burayi moduler yapmak gerek
        
        items.Add("CamSise");
        items.Add("PetSise");
        items.Add("CamBardak");
        items.Add("SuSisesi");

        foreach (var item in items)
        {
            dropdown.options.Add(new TMP_Dropdown.OptionData() { text = item });
        }

        DropDownItemSelected(dropdown);
        dropdown.onValueChanged.AddListener(delegate { DropDownItemSelected(dropdown); });

        VerileriAl();
        Doldur();
        
        
    }

    private void Doldur()
    {
        foreach (string item in items)
        {
            reference.Child("Items").Child(item).GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.Log("db hata");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                   // Debug.Log(snapshot.GetRawJsonValue());
                    if (snapshot.GetRawJsonValue() == null)
                    {
                        Debug.Log("bos hatasi");
                        return;
                    }
                    else
                    {//burada cozulecek her sey
                        
                       // Debug.Log("snapshot.Key: " + snapshot.Key + " BURADAYIZ ");
                        ItemData data1 = JsonUtility.FromJson<ItemData>(snapshot.GetRawJsonValue());
                        Debug.Log("name, ucret: " + data1.Name + data1.ucreti);
                        
                        
                    }
                }

            });
        }

        //reference.Child("Items").GetValueAsync().ContinueWithOnMainThread(task =>
        //{
        //    if (task.IsFaulted)
        //    {
        //        Debug.Log("db hata");
        //    }
        //    else if (task.IsCompleted)
        //    {
        //        DataSnapshot snapshot = task.Result;
        //        Debug.Log(snapshot.GetRawJsonValue());
        //        if (snapshot.GetRawJsonValue() == null)
        //        {
        //            Debug.Log("bos hatasi");
        //            return;
        //        }
        //        else
        //        {//islemler buraya



        //        }
        //    }

        //});
    }


    void DropDownItemSelected(TMP_Dropdown dropdown)
    {
        int index = dropdown.value;

        textBox.text = dropdown.options[index].text;
        VerileriAl();
    }


    private void VerileriAl()
    {

        reference.Child("Items").Child(textBox.text).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                Debug.Log("Database Hata");
            }
            else if (task.IsCompleted)
            {
                DataSnapshot snapshot = task.Result;
               // Debug.Log(snapshot.Key);//bunun ciktisi cam sisi(vs) yani basligi
                //Debug.Log(snapshot.GetRawJsonValue());
                if (snapshot.GetRawJsonValue() == null)//bu bos ise henuz veri kaydi olusturmamisiz demek
                {
                    Debug.Log("Boþ");
                    BosVeriOlustur();//baslangic verisi olusturuyoruz
                    VerileriAl();
                }
                else
                {
                    //varsa direk cekiyoruz
                   // Debug.Log(snapshot.GetRawJsonValue());
                    //jsondan objeye - objeden jsona cevirme
                    ItemData data = JsonUtility.FromJson<ItemData>(snapshot.GetRawJsonValue());
                   // Debug.Log(data.ucreti);
                }
            }
        });
    }

    void BosVeriOlustur()
    {
        ItemData bosveri = new ItemData
        {
            Name = "",
            gramaj = 0,
            ucreti = 0,
            type = 0
        };
        //jsona ceviriyoruz burada 
        string bosJson = JsonUtility.ToJson(bosveri);
        //database yazdiriyoruz
        reference.Child("Items").Child(textBox.text).SetRawJsonValueAsync(bosJson);
    }


}
