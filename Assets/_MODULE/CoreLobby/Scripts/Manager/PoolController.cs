using UnityEngine;
using PathologicalGames;
using System.Collections.Generic;
using TMPro;
using DG.Tweening;

public class PoolController : MonoSingleton<PoolController>
{
    [SerializeField] SpawnPool pools = null;

    [SerializeField] PrefabConfigs prefabConfigs = null;
    Dictionary<PrefabTypes, Transform> dicPrefabs = new Dictionary<PrefabTypes, Transform>();

    public PrefabConfigs PrefabConfigs { get => prefabConfigs; }

    protected override void OnInitiate()
    {
        base.OnInitiate();
        PrefabConfigs?.InitDic(ref dicPrefabs);
    }

    public Transform SpawnObject(PrefabTypes prefabType, Vector3 position, Quaternion rotation)
    {
        Transform prefab = this.GetPrefab(prefabType);
        if (prefab == null)
        {
            Debug.LogError("[error]Cant spawn, Missing prefab");
            return null;
        }
        Transform clone = pools.Spawn(prefab, position, rotation);
        return clone;
    }
    public Transform SpawnObject(PrefabTypes prefabType, Vector3 position, Quaternion rotation, float despawnAfter)
    {
        Transform prefab = this.GetPrefab(prefabType);
        if (prefab == null)
        {
            Debug.LogError("[error]Cant spawn, Missing prefab");
            return null;
        }
        Transform clone = pools.Spawn(prefab, position, rotation);
        pools.Despawn(clone, despawnAfter);
        return clone;
    }

    public Transform SpawnObject(Transform prefab)
    {
        if (prefab == null)
        {
            Debug.LogError("[error]Cant spawn, Missing prefab");
            return null;
        }
        Transform clone = pools.Spawn(prefab);
        return clone;// AssignPool(clone, poolManager);
    }

    public Transform Spawn(Transform prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null)
        {
            Debug.LogError("[error]Cant spawn, Missing prefab");
            return null;
        }
        Transform clone = pools.Spawn(prefab, position, rotation);
        return clone;// AssignPool(clone, poolManager);
    }

    public Transform SpawnObject(Transform prefab, Vector3 position, Quaternion rotation, float timeLife)
    {
        if (prefab == null)
        {
            Debug.LogError("[error]Cant spawn, Missing prefab");
            return null;
        }
        Transform clone = pools.Spawn(prefab, position, rotation);
        pools.Despawn(clone, timeLife);
        return clone;// AssignPool(clone, poolManager);
    }

    //public Transform SpawnObject(Transform prefab, Vector3 position, Quaternion rotation, Transform parent)
    //{
    //    if (prefab == null)
    //    {
    //        Debug.LogError("[error]Cant spawn, Missing prefab");
    //        return null;
    //    }
    //    Transform clone = poolManager.Spawn(prefab, position, rotation, parent);
    //    return AssignPool(clone, poolManager);
    //}

    //Transform AssignPool(Transform clone, SpawnPool pool)
    //{
    //    clone.GetComponent<PoolElement>()?.OnSpawned(pool);
    //    return clone;
    //}

    public void DeSpawnAll()
    {
        this.pools?.DespawnAll();
    }

    public void DespawnObject(Transform clone)
    {
        if (pools.IsSpawned(clone))
        {
            pools.Despawn(clone);
        }
    }

    public void DespawnObject(Transform clone, float delay)
    {
        if (pools.IsSpawned(clone))
        {
            pools.Despawn(clone, delay);
        }
    }

    public Transform GetPrefab(PrefabTypes prefabType)
    {
        if (dicPrefabs.ContainsKey(prefabType))
        {
            return dicPrefabs[prefabType];
        }
        Debug.LogError($"Missing prefab :{prefabType}");
        return null;
    }

    //public void SpawnTextFly3D(string text, Vector3 position)
    //{
    //    Transform textPf = GetPrefab(PrefabType.TextFly3D);
    //    if (textPf != null)
    //    {
    //        Transform textClone = pools.Spawn(textPf, position, Quaternion.identity);
    //        textClone.GetComponent<TextMeshPro>()?.SetText(text);
    //        textClone.DOLocalMoveY(.50f, 0.75f).SetEase(Ease.OutExpo).SetRelative().OnComplete(delegate
    //        {
    //            pools.Despawn(textClone);
    //        });
    //    }
    //}

    //public void SpawnGUIText(string text, Vector3 position, Color color)
    //{
    //    Transform effectPf = PrefabConfigs.GetPrefab(PrefabTypes.gui_text_effect);
    //    if (effectPf != null)
    //    {
    //        //SpawnObject(effectPf).GetComponent<UITextEff>()?.SetText(text, position, color);
    //    }
    //}
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (pools == null)
            pools = GetComponent<SpawnPool>();
    }
#endif
}
