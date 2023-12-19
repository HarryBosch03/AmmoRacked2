using System;
using UnityEngine;

namespace AmmoRacked2.Runtime.Meta
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Gamemode")]
    public class Gamemode : ScriptableObject
    {
        public bool respawn;
        public float respawnTime;
        public bool keepScore = true;
        public bool allowNegativeScore = true;
        public int pointsOnKill = 1;
        public int pointsOnDeath = -1;
        public bool keepTime = true;
        public float timeLimitSeconds = 2.0f * 60.0f;

        [Space]
        public Color[] playerColors =
        {
            new Color(1f, 0.54f, 0.12f),
            new Color(0.5f, 1f, 0.17f),
            new Color(0.22f, 0.71f, 1f),
            new Color(0.68f, 0.17f, 1f),
        };
        
        private void OnValidate()
        {
            playerColors = ResizeArray(playerColors, 4);
        }

        private static T[] ResizeArray<T>(T[] input, int count)
        {
            if (input.Length == count) return input;
            
            var output = new T[count];
            
            for (var i = 0; i < count && i < input.Length; i++)
            {
                output[i] = input[i];
            }

            return output;
        }
    }
}