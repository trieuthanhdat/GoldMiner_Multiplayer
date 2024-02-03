using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CoreGame;
using Cysharp.Threading.Tasks;

public class LobbyScene : MonoBehaviour
{
    [SerializeField] FusionLauncher FusionLauncherPf = null;

    IEnumerator Start()
    {
        //yield return new WaitForSeconds(0.2f);
        FusionLauncher fusionLauncher = FindObjectOfType<FusionLauncher>();
        if (fusionLauncher != null)
        {
            Destroy(fusionLauncher.gameObject);
        }
        yield return null;
        // New Fusion
        Instantiate<FusionLauncher>(FusionLauncherPf);
    }

}
