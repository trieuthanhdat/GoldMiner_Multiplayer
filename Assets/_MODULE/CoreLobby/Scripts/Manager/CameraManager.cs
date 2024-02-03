using UnityEngine;
using DG.Tweening;
using UnityEngine.SocialPlatforms;
using UnityEditor;

[ExecuteInEditMode]
public class CameraManager : MonoBehaviour
{
    public static CameraManager Default = null;

    [SerializeField]
    private float speedLerp = 10.0f;
    [SerializeField]
    private Transform cacheTransform = null;
    [SerializeField]
    private Camera cam = null;

    public Transform Target { get; set; }
    public Quaternion Rotation { get; private set; }

#if UNITY_EDITOR
    [Header("If u want to follow in scene")]
    public Transform editorTarget = null;
#endif


    private void Awake()
    {
        Default = this;
        cacheTransform = transform;
        Rotation = cam.transform.rotation;
    }

    // Update is called once per frame
    private void Update()
    {
        if (cacheTransform != null && Target != null)
        {
            cacheTransform.position = Vector3.Lerp(cacheTransform.position, Target.position, Time.deltaTime * speedLerp);
            return;
        }

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            if (editorTarget != null)
                cacheTransform.position = editorTarget.position;
        }
#endif
    }

    public void DOFieldOfView(float toValue)
    {
        if (cam != null)
            cam.DOFieldOfView(toValue, 0.4f);
    }

    public void SetTarget(Transform target, bool isForceUpdate = true)
    {
        this.Target = target;
        if (isForceUpdate)
            cacheTransform.position = target.position;
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (cam == null)
            cam = GetComponentInChildren<Camera>();
    }
#endif
}
