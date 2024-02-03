﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public abstract class Singleton<T> where T : new()
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new T();
            }
            return instance;
        }
    }
}

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T instance = null;
    public static T Instance
    {
        get
        {
            // Instance requiered for the first time, we look for it
            if (instance == null)
            {
                instance = GameObject.FindObjectOfType(typeof(T)) as T;

                // Object not found, we create a temporary one
                if (instance == null)
                {
                    Debug.LogWarning("No instance of " + typeof(T).ToString() + ", a temporary one is created.");

                    instance = new GameObject("Temp Instance of " + typeof(T).ToString(), typeof(T)).GetComponent<T>();

                    // Problem during the creation, this should not happen
                    if (instance == null)
                    {
                        Debug.LogError("Problem during the creation of " + typeof(T).ToString());
                    }
                }

                instance.OnInitiate();
            }
            return instance;
        }
    }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            instance = this as T;
            instance.OnInitiate();
        }
        else if (instance != this)
        {
            //Debug.LogError("Another instance of " + GetType() + " is already exist! Destroying self...");
            Destroy(this.gameObject);
            return;
        }
    }


    /// <summary>
    /// This function is called when the instance is used the first time
    /// Put all the initializations you need here, as you would do in Awake
    /// </summary>
    protected virtual void OnInitiate() { }

    /// Make sure the instance isn't referenced anymore when the user quit, just in case.
    private void OnApplicationQuit()
    {
        instance = null;
    }
}