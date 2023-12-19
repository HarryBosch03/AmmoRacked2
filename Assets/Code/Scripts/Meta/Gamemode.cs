using UnityEngine;

namespace AmmoRacked2.Runtime.Meta
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Gamemode")]
    public class Gamemode : ScriptableObject
    {
        public bool respawn;
        public float respawnTime;
        public bool keepScore;
        public int pointsOnKill = 1;
        public int pointsOnDeath = -1;
        public bool limitTime;
        public float timeLimitSeconds = 2.0f * 60.0f;
    }
}
