using CoreGame;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


[CreateAssetMenu(menuName = "Configs/PrefabConfigs")]
public class PrefabConfigs : ScriptableObject
{
    [System.Serializable]
    public class PrefabConfig<T>
    {
        public T PrefabType;
        public Transform Prefab;
    }

    [SerializeField] PrefabConfig<PrefabTypes>[] prefabConfigs = new PrefabConfig<PrefabTypes>[0];

    public void InitDic(ref Dictionary<PrefabTypes, Transform> keyValuePairs)
    {
        foreach (var item in prefabConfigs)
        {
            if (!keyValuePairs.ContainsKey(item.PrefabType))
            {
                keyValuePairs.Add(item.PrefabType, item.Prefab);
            }
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {

    }
#endif
}

public enum PrefabTypes
{
    Footstep = 0,
}

