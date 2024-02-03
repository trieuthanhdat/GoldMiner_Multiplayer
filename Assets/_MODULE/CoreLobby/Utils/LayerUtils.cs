using UnityEngine;

namespace Utils
{
    public static class LayerUtils
    {
        public static int GetDoorsLayerMask()
        {
            return LayerMask.GetMask("Default", "Victim", "Player");
        }

        public static int GetRadarAllLayerMask()
        {
            return ~LayerMask.GetMask("Victim");
        }

        public static int GetVictimLayerMask()
        {
            return LayerMask.GetMask("Victim");
        }

        public static int GetVictimLayer(bool isVictim)
        {
            return (isVictim) ? LayerMask.NameToLayer("Victim") : LayerMask.NameToLayer("Default");
        }
    }
}