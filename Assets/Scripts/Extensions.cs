using UnityEngine;

namespace RockTools
{
    public static class Extensions
    {
        public static Quaternion Inverse(this Quaternion q)
        {
            return new Quaternion(-q.x, -q.y, -q.z, q.w);
        }

        public static Quaternion Quaternion(this float angle, Vector3 axis)
        {
            var w = Mathf.Cos(angle / 2f);
            var x = Mathf.Sin(angle / 2f) * axis.x;
            var y = Mathf.Sin(angle / 2f) * axis.y;
            var z = Mathf.Sin(angle / 2f) * axis.z;

            return new Quaternion(x, y, z, w);
        }

        public static Vector2 RadianToVector2(this float radian)
        {
            return new Vector2(Mathf.Cos(radian), Mathf.Sin(radian));
        }

        public static Vector2 DegreeToVector2(this float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }

        public static void ClearChildren(this Transform transform)
        {
            var children = transform.childCount;
            for (var i = children - 1; i >= 0; i--)
#if UNITY_EDITOR
                Object.DestroyImmediate(transform.GetChild(i).gameObject);
#else
                Object.Destroy(transform.GetChild(i).gameObject);
#endif
        }

        public static Vector3 RandomVectorFloat()
        {
            return new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
        }

        public static void BlockCopy<T>(this T[] src, int srcOffset, T[] dst, int dstOffset, int count)
        {
            for (var i = 0; i < count; i++)
                dst[i + dstOffset] = src[i + srcOffset];
        }
    }
}