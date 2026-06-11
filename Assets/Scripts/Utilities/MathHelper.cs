using UnityEngine;

namespace LaserParticleAnalyzer.Utilities
{
    public static class MathHelper
    {
        public static float SampleLogNormal(float meanLog, float stdLog)
        {
            float u1 = Random.value;
            float u2 = Random.value;
            float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
            return Mathf.Exp(meanLog + z * stdLog);
        }

        public static float GaussianRandom(float mean, float std)
        {
            float u1 = Random.value;
            float u2 = Random.value;
            float z = Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
            return mean + z * std;
        }

        public static float Smoothstep(float edge0, float edge1, float x)
        {
            float t = Mathf.Clamp01((x - edge0) / (edge1 - edge0));
            return t * t * (3f - 2f * t);
        }
    }
}
