using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Fusion;
using System.Linq;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif


public static class Utility
{
    private static System.Random rng = new System.Random();

    public static int GetRandom(int min, int max)
    {
        return rng.Next(min, max);
    }
    public static void Shuffle<T>(this IList<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    /// <summary>
    /// Identifiers the generation.
    /// </summary>
    /// <returns>A Unique ID</returns>
    public static string generateUniqueID(int _characterLength = 11)
    {
        System.Text.StringBuilder _builder = new System.Text.StringBuilder();
        System.Linq.Enumerable
            .Range(65, 26)
            .Select(e => ((char)e).ToString())
            .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
            .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
            .OrderBy(e => Guid.NewGuid())
            .Take(_characterLength)
            .ToList().ForEach(e => _builder.Append(e));
        return _builder.ToString();
    }

    public static void SetVisible(this GameObject obj, bool isVisible)
    {
        if (obj == null)
            return;
        obj.SetActive(isVisible);
    }
    public static void Log(this Exception exception)
    {
        Debug.Log(string.Format("<color=yellow>[Exception] {0}</color>", exception.Message));
    }
    public static void ShowLog(this MonoBehaviour mono, string log)
    {
        Debug.Log(string.Format("<color=cyan>[Log] {0}</color>", log));
    }
    public static void DebugLog(this MonoBehaviour mono, string log, Color color)
    {
        Debug.Log($"<color={ColorUtility.ToHtmlStringRGBA(color)}>[Log] {log}</color>");
    }
    public static void LogError(this MonoBehaviour mono, string log)
    {
        Debug.Log(string.Format("[Error]<color=red>[Error] {0}</color>", log));
    }

    public static void SetVisible(this CanvasGroup canvasGroup, bool isVisible, float alphaHide = 0.0f)
    {
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = isVisible;
            canvasGroup.alpha = isVisible ? 1 : alphaHide;
        }
    }

    public static string FormatTimerDisplay(float timer)
    {
        int second = Mathf.FloorToInt(timer / 60f);
        int minute = Mathf.FloorToInt(timer % 60f);
        return string.Format("{0:00}:{1:00}", second, minute);
    }

    public static TextAsset LoadTextAsset(string path)
    {
        try
        {
            return Resources.Load(path, typeof(TextAsset)) as TextAsset;
        }
        catch (Exception e)
        {
            Debug.Log("Cannot load text asset " + path + "with error " + e.Message);
        }
        return null;
    }
}

[System.Serializable]
public class UnityButton : UnityEvent { }
[System.Serializable]
public class InputNetworkEvent<NetworkRunner, NetworkInput> : UnityEvent<NetworkRunner, NetworkInput> { }

public class ReadOnlyAttribute : UnityEngine.PropertyAttribute
{

}

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property,
                                            GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect position,
                               SerializedProperty property,
                               GUIContent label)
    {
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, true);
        GUI.enabled = true;
    }
}
#endif