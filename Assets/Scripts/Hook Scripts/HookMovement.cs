using CoreGame;
using Cysharp.Threading.Tasks;
using Fusion;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HookMovement : NetworkBehaviour
{
    private float rotate_Speed = 55f;
    public Vector3 HookPosition => transform.position;
    #region _____ SERIALIZED _____
    [SerializeField] float move_Speed = 3f;
    [SerializeField] float return_Speed_Modifier = 1.2f;
    [SerializeField] float min_Y = -2.5f;
    [SerializeField] float min_Z = -55f, max_Z = 55f;
    #endregion
   
    public float MoveSpeed { get => move_Speed; set => move_Speed = value; }

    #region _____ LOCAL _____
    private float rotate_Angle;
    private bool rotate_Right;
    private bool canRotate;
    private float initial_Move_Speed;
    private float initial_Y;
    private bool moveDown;
    public  bool MoveDown  { get => moveDown; }
    public  bool CanRotate { get => canRotate; }
    #endregion
    public override void Spawned()
    {
        initial_Y = transform.position.y;
        initial_Move_Speed = move_Speed;
        canRotate = true;
    }
    // Update is called once per frame
    public override void FixedUpdateNetwork()
    {
        Rotate();
        MoveRope();
        CheckExitScreen();
    }
    void Rotate()
    {
        if (!canRotate)
            return;

        if (rotate_Right)
        {
            rotate_Angle += rotate_Speed * Runner.DeltaTime;
        }
        else
        {
            rotate_Angle -= rotate_Speed * Runner.DeltaTime;
        }
        transform.rotation = Quaternion.AngleAxis(rotate_Angle, Vector3.forward);

        if(rotate_Angle >= max_Z)
        {
            rotate_Right = false;
        }else if (rotate_Angle <= min_Z)
        {
            rotate_Right = true;
        }
    } //can rotate
    public void StartHook()
    {
        if (canRotate)
        {
            canRotate = false;
            moveDown = true;
            Debug.Log("START HOOK");
        }
    } // get input
    void MoveRope()
    {
        if (canRotate) return;
        SoundManager.instance.RopeStretch(true);
        Vector3 temp = transform.position;
        Debug.Log("MOVING HOOK");
        if (moveDown)
        {
            temp -= transform.up * Runner.DeltaTime * move_Speed;
        }
        else
        {
            temp += transform.up * Runner.DeltaTime * move_Speed;
        }
        transform.position = temp;

        if (temp.y <= min_Y)
        {
            moveDown = false;
            move_Speed = initial_Move_Speed;
        }
        if (temp.y >= initial_Y)
        {
            canRotate = true;
            // reset move speed
            move_Speed = initial_Move_Speed;
            SoundManager.instance.RopeStretch(false);
        }
    }// move rope
    public void HookAttachedItem()
    {
        moveDown = false;
    }
    private void CheckExitScreen()
    {
        GoldMiner_GameManagerFusion manager = GoldMiner_GameManagerFusion.Instance;
        if (!manager || manager.LevelBoundaries.IsInsideBoundaries(transform.position))
            return;
        moveDown = false;
    }

}
