using UnityEngine;
using Fusion;
using PathologicalGames;

namespace CoreGame
{
    public class FusionPrebabPools : MonoSingleton<FusionPrebabPools>, INetworkObjectPool
    {
        [SerializeField] SpawnPool pools = null;
        //protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        //{
        //    try
        //    {
        //        NetworkObject prefab;
        //        if (NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out prefab))
        //        {
        //            if (prefab != null)
        //            {
        //                Transform clone = pools.Spawn(prefab.transform);
        //                return clone.GetComponent<NetworkObject>();
        //            }
        //            else
        //            {
        //                Debug.LogError("No prefab for " );
        //            }
        //        }
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Debug.LogError(ex.Message);
        //    }
        //    return base.InstantiatePrefab(runner, prefab);
        //}

        //protected override void DestroyPrefabInstance(NetworkRunner runner, NetworkPrefabId prefabId, NetworkObject instance)
        //{
        //    //base.DestroyPrefabInstance(runner, prefabId, instance);
        //    if (instance == null)
        //    {
        //        Debug.LogError("Missing-instance");
        //        return;
        //    }
        //    if (this.pools.IsSpawned(instance.transform))
        //    {
        //        this.pools.Despawn(instance.transform);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("instance-is-not-spawned");
        //    }
        //}

        //@Spawn
        public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
        {
            try
            {
                NetworkObject prefab;
                if (NetworkProjectConfig.Global.PrefabTable.TryGetPrefab(info.Prefab, out prefab))
                {
                    if (prefab != null && prefab.transform != null)
                    {
                        Transform clone = pools.Spawn(prefab.transform);
                        return clone.GetComponent<NetworkObject>();
                    }
                    else
                    {
                        Debug.LogError("No prefab for " + info.Prefab);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError(ex.Message);
            }

            Debug.LogError("No prefab for " + info.Prefab);
            return null;
        }

        // @Despawn
        public void ReleaseInstance(NetworkRunner runner, NetworkObject instance, bool isSceneObject)
        {
            if (instance == null)
            {
                Debug.LogError("Missing-instance");
                return;
            }
            if (this.pools.IsSpawned(instance.transform))
            {
                this.pools.Despawn(instance.transform);
            }
            else
            {
                Debug.LogWarning("instance-is-not-spawned");
            }
        }

        public void OnReconnect()
        {
            this.pools.DespawnAll();
        }
    }

}
