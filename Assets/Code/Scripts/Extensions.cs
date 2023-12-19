
using UnityEngine;

namespace AmmoRacked2.Runtime
{
    public static class Extensions
    {
        public static T Find<T>(this Transform transform, string path)
        {
            var find = transform.Find(path);
            return find ? find.GetComponent<T>() : default;
        }
    }
}