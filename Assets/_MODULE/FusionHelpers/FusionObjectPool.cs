using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace FusionHelpers
{
	/// <summary>
	/// Example of a Fusion Object Pool.
	/// The pool keeps a list of available instances by prefab and also a list of which pool each instance belongs to.
	/// </summary>

	public class FusionObjectPool : ObjectPool<NetworkObject>, INetworkObjectPool
	{
		public NetworkObject AcquireInstance(NetworkRunner runner, NetworkPrefabInfo info)
		{
			if (runner.Config.PrefabTable.TryGetPrefab(info.Prefab, out var prefab))
			{
				return AcquireInstance(prefab);
			}
			Debug.LogError("No prefab for " + info.Prefab);
			return null;
		}

		public void ReleaseInstance(NetworkRunner runner, NetworkObject no, bool isSceneObject)
		{
			Debug.Log($"Releasing {no} instance, isSceneObject={isSceneObject}");
			ReleaseInstance(no);
		}
	}
}