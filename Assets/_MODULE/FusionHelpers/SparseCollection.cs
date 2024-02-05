using Fusion;
using UnityEngine;
using Object = UnityEngine.Object;

namespace FusionHelpers
{
	/// <summary>
	/// ISparseState represents the networked part of a simple object that require infrequent (network) updates,
	/// but still needs a visual game object on all clients.
	/// For example a bullet following a straight line can be rendered just knowing its start location and velocity.
	/// </summary>
	/// <typeparam name="P">The MonoBehaviour that represents this state visually</typeparam>
	public interface ISparseState<P> : INetworkStruct where P: MonoBehaviour
	{
		public int StartTick { get; set; }
		public int EndTick { get; set; }

		/// <summary>
		/// The extrapolate method should update properties on the state struct using the given local time t
		/// (local time is the offset in seconds from StartTick).
		/// Note that these changes are just local predictions/interpolations - they will not automatically be networked.
		/// (See the SparseCollection.Process method)
		/// </summary>
		/// <param name="t">Local time in seconds to extrapolate to (t is 0 at Runner.Tick==StartTick)</param>
		/// <param name="prefab">Prefab used to create the visual (handy for accessing visual object configuration - it is *NOT* the actual game object of this state)</param>
		public void Extrapolate(float t, P prefab);
	}

	/// <summary>
	/// ISparseVisual is implemented by local game objects that "visualize" an ISparseState.
	/// </summary>
	/// <typeparam name="T">The actual type of the sparse state</typeparam>
	/// <typeparam name="P">The MonoBehaviour used to visualise the sparse state</typeparam>
	public interface ISparseVisual<T,P> where T: unmanaged, ISparseState<P> where P: MonoBehaviour, ISparseVisual<T,P>
	{
		/// <summary>
		/// This method is called every frame to update the visual to the current state.
		/// </summary>
		/// <param name="owner">The NB that owns the collection of sparse states</param>
    /// <param name="state">The current render state of this particular visual</param>
    /// <param name="t">The current local time that the state represents</param>
		/// <param name="isFirstRender">True the first time this is called for a new visual</param>
		/// <param name="isLastRender">True the last time this is called for a given visual</param>
		public void ApplyStateToVisual(NetworkBehaviour owner, T state, float t, bool isFirstRender, bool isLastRender);
  }

	/// <summary>
	/// The sparse collection maps sparse states to sparse visuals and takes care of calculating local time.
	/// </summary>
  /// <typeparam name="T">The actual type of the sparse state</typeparam>
  /// <typeparam name="P">The MonoBehaviour used to visualise the sparse state</typeparam>
	public class SparseCollection<T,P> where T: unmanaged, ISparseState<P> where P: MonoBehaviour, ISparseVisual<T,P>
	{
		private struct Entry
		{
			public P visual;
			public T cache;
			public bool disable;
		}

		// To give proxies a chance to disable the local GO before it's re-used, we wait a few additional ticks before re-using a state.
		private const int REUSE_DELAY_TICKS = 12;

		private NetworkArray<T> _states;
		private Entry[] _entries;
		private readonly P _prefab;
		private float _lastRenderTime;

		/// <summary>
		/// The sparse collection itself is not a networked object, so you need to create its backing data elsewhere (in a NB)
		/// and pass it to the constructor along with a reference to the prefab to use for the associated visuals.
		/// Once created, call
		/// * Render() from the NetworkBehaviour's Render() method
		/// * Process() from the NetworkBehaviour's FixedUpdateNetwork() method if you want to alter state, and
		/// * Add() from Input or State auth to "spawn" a new object.
		/// </summary>
		/// <param name="states">Networked array of sparse state structs</param>
		/// <param name="prefab">Prefab to use for visuals</param>
		public SparseCollection(NetworkArray<T> states, P prefab) 
		{
			_entries = new Entry[states.Length];
			_states = states;
			_prefab = prefab;
		}

		/// <summary>
		/// Call Render() every frame to update visuals to their associated sparse state.
		/// </summary>
		/// <param name="owner">The NB that contains the networked state objects</param>
		public void Render(NetworkBehaviour owner)
		{
			NetworkRunner runner = owner.Runner;

			// Pick the relevant timeframe - input authority is predicted, others are interpolated (for state auth, those are the same, so doesn't matter)
			float renderTime = owner.HasInputAuthority ? runner.SimulationRenderTime : runner.InterpolationRenderTime;

			// In the event that tick-rate is higher than framerate, there's a risk that we will completely miss single-tick effects like eg. a hit-scan projectile. 
			// To fix this, we "Render" all ticks between last render time and current render time (mostly this is just one render)
			if (_lastRenderTime == 0)
				_lastRenderTime = renderTime;
			
			// As long as framerate is higher than tickrate, renderTime will be the smaller value and we'll only do one iteration.
			float t0 = Mathf.Min( renderTime, _lastRenderTime + runner.DeltaTime);

			do 
			{
				for (int i = 0; i < _entries.Length; i++)
				{
					Entry e = _entries[i];
					T state = _states[i];

					if (e.disable)
					{
						e.visual.gameObject.SetActive(false);
						e.disable = false;
					}
					float t = t0;

					if (e.cache.StartTick != state.StartTick)
					{
						if(t>e.cache.EndTick*runner.DeltaTime)
							e.cache = state;
					}

					if (t < state.StartTick*runner.DeltaTime)
						state = e.cache;
					else
						e.cache = state;
						
					if (state.StartTick == 0)
					{
						if(e.visual)
							e.visual.gameObject.SetActive(false); // This is needed to disable initial mis-predicted instances
						continue;
					}
					
					t -= state.StartTick * runner.DeltaTime;

					bool isLastRender = t > (state.EndTick - state.StartTick) * runner.DeltaTime;
					bool isSpawned = t >= 0 ;
					bool isFirstRender = false;
 
					// Make sure we have a valid enabled GameObject if this state represents an active instance
					if (isSpawned && !isLastRender) 
					{
						if (!e.visual) 
						{
							e.visual = Object.Instantiate(_prefab);
							isFirstRender = true;
						}
						if (!e.visual.gameObject.activeSelf) 
						{
							e.visual.gameObject.SetActive(true);
							isFirstRender = true; 
						}
					}

					// Update the GameObject to current state
					if(e.visual && e.visual.gameObject.activeSelf) 
					{
						// Update state to t
						state.Extrapolate(t, _prefab);
						// Update visual to match the state
						e.visual.ApplyStateToVisual(owner, state, t, isFirstRender, isLastRender);

						// Disable the GameObject if this was its last render update
						// We delay disabling of the object one frame since "last render" isn't really a last render if the object is immediately hidden.
						if (isLastRender)
							e.disable = true; 
					}

					_entries[i] = e;
				}
				t0 += runner.DeltaTime;
			} while (	t0<renderTime);

			_lastRenderTime = renderTime;
		}

    public delegate bool Processor(ref T state, int tick);

		/// <summary>
		/// Call process every tick if you want to adjust *networked* properties on the sparse state.
		/// As the name suggests, these updates should be infrequent, so if you *do* change the state
		/// you must return true from the delegate to update the backing array.
		/// </summary>
		/// <param name="owner">The NB that owns the sparse state list</param>
		/// <param name="process">A delegate that will process each (active) sparse state</param>
		public void Process( NetworkBehaviour owner, Processor process)
		{
			if (owner.IsProxy)
				return;

			NetworkRunner runner = owner.Runner;
			for (int i = 0; i < _states.Length; i++) 
			{
				T state = _states[i];
				int simtick = runner.Tick;
				float t = (simtick - state.StartTick) * runner.DeltaTime;
				if (simtick <= state.EndTick) 
				{
					state.Extrapolate(t, _prefab);
					if (process(ref state, simtick)) 
					{
						// Since we're storing the extrapolated state, we must also update the start tick, as this is our new starting point going forward.
						state.StartTick = simtick;
						// Update the networked backing storage so the change is propagated
						_states[i] = state;
					}
				}
			}
		}

		/// <summary>
		/// Call Add to "instantiate" (or rather, "activate") a new sparse state. Note that this will not allocate
		/// but simply select the next in-active sparse state in the array. It will do nothing if none is found.
		/// </summary>
		/// <param name="runner"></param>
		/// <param name="state">The initial state to add</param>
		/// <param name="secondsToLive">Initial number of ticks for the sparse state to be alive</param>
		public void Add(NetworkRunner runner, T state, float secondsToLive)
		{
			state.StartTick = runner.Tick;
			state.EndTick = state.StartTick + Mathf.Max(1,(int)(secondsToLive / runner.DeltaTime));

			for (int i = 0; i < _states.Length; i++)
			{
				if ( runner.Tick > _states[i].EndTick+REUSE_DELAY_TICKS)
				{
					_states[i] = state;
					return;
				}
			}
			Debug.LogWarning("No free slots in state array!");
		}

		/// <summary>
		/// Call Clear to destroy all visuals for this sparse set.
		/// </summary>
		public void Clear()
		{
			for (int i=0;i<_entries.Length;i++)
			{
				Entry e = _entries[i];
				if(e.visual)
					Object.Destroy(e.visual.gameObject);
				_entries[i] = default;
			}
		}
	}
}