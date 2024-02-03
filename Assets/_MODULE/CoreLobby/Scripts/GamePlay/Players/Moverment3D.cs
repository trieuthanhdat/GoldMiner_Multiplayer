using UnityEngine;
using Fusion;
using CoreGame;

namespace CoreLobby
{
    [RequireComponent(typeof(Rigidbody))]
    public class Moverment3D : NetworkBehaviour
    {
        const float ROTATION_SPEED = 10F;

        [ReadOnly][SerializeField] protected PlayerNetworked player;
        [ReadOnly][SerializeField] protected Rigidbody rigi;
        [ReadOnly][SerializeField] protected Animator animator;
        [SerializeField]
        private float speedMove = 2.0f;
        [SerializeField]
        private bool isUsingLerp = false;

        private Transform CacheTrans = null;
        private InputData data;
        private bool IsMine => Object.HasInputAuthority;

        #region Networked
        public bool IsMoving { get; set; }
        #endregion


        private void Awake()
        {
            CacheTrans = transform;
        }
        public override void FixedUpdateNetwork()
        {
            if (IsMine)
            {
                //Local
                if (!player.CanMove())
                {
                    if (animator != null)
                        animator.SetBool("IsMove", false);
                    return;
                }
            }

            // Todo: All-Client handle input 
            if (GetInput(out data))
            {
                Vector3 moverment = data.Direction;
                Debug.Log(moverment.ToString());
                IsMoving = moverment != Vector3.zero;

                if (IsMoving)
                {
                    if (IsMine)
                    {
                        CacheTrans.rotation = Quaternion.Lerp(CacheTrans.rotation, Quaternion.LookRotation(data.Direction), Runner.DeltaTime * ROTATION_SPEED);
                        rigi?.MovePosition(transform.position + CacheTrans.forward * Runner.DeltaTime * speedMove);
                    }
                }

                if (animator != null)
                    animator.SetBool("IsMove", IsMoving);
            }
        }

        public void TeleportTo(Vector3 position)
        {
            position.y = 0;
            rigi.position = position;
            transform.position = position;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (player == null)
                player = GetComponent<PlayerNetworked>();
            if (rigi == null)
                rigi = GetComponent<Rigidbody>();
            if (animator == null)
                animator = GetComponentInChildren<Animator>();
        }
#endif
    }
}
