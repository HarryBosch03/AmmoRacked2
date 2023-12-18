using UnityEngine;

namespace AmmoRacked2.Runtime.Meta
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Gamemode")]
    public class Gamemode : ScriptableObject
    {
        public bool respawn;
        public float respawnTime;
        public int pointsOnKill = 1;
        public int pointsOnDeath = -1;
    }
}
