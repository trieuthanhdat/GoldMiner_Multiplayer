
using System.Collections.Generic;
using UnityEngine;

public class UserData
{
    public static UserData Local;

    public bool IsBanned;
    public string Email;
    public int Gold;
    public int FeeOfMatch;
    
    public string RefferalCode { get;set; }

    public int TimeFindMatch = 50;
    public int TimeBattle = 180;

    public string DisplayName
    {
        get => PlayerPrefs.GetString("DisplayName", "Dev Name"); 
        set
        {
            PlayerPrefs.SetString("DisplayName", value);
            PlayerPrefs.Save();
        }
    }
    public UserData()
    {
    }
}
