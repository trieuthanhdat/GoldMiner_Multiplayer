using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Configs/AudioConfigs")]
public class AudioConfigs : ScriptableObject
{
    [System.Serializable]
    public class AudioConfig
    {
        public AudioType audioType;
        public AudioClip clip;
    }

    [Header("Audio Clips")]
    [SerializeField] AudioConfig[] audioConfigs = new AudioConfig[0];

    public AudioConfig[] AudioArray { get => audioConfigs;}
}
