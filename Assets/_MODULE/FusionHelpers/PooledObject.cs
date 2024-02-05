using UnityEngine;

namespace FusionHelpers
{
	public class PooledObject : MonoBehaviour
	{
		private static LocalObjectPool _localPool;

		public static T Acquire<T>(T prefab, Vector3 pos = default, Quaternion rot = default, Transform p = default) where T : PooledObject
		{
			if (_localPool == null)
			{
				GameObject go = new GameObject("LocalObjectPool");
				DontDestroyOnLoad(go);
				_localPool = go.AddComponent<LocalObjectPool>();
			}
			return (T)_localPool.AcquireInstance(prefab, pos,rot,p);
		}

		public void Release()
		{
			_localPool.ReleaseInstance(this);
		}
	}
}