using System.Collections.Generic;
using UnityEngine;

namespace LaserParticleAnalyzer.Models
{
    [System.Serializable]
    public class ParticleData
    {
        public float diameter;
        public float density;
        public float refractiveIndex;
        public Vector3 position;
        public Vector3 velocity;
        public bool isActive;
        public float createdTime;

        public ParticleData()
        {
            diameter = 10f;
            density = 2.65f;
            refractiveIndex = 1.46f;
            position = Vector3.zero;
            velocity = Vector3.zero;
            isActive = false;
            createdTime = 0f;
        }

        public void Initialize(float d, Vector3 pos, Vector3 vel)
        {
            diameter = d;
            position = pos;
            velocity = vel;
            isActive = true;
            createdTime = Time.time;
        }

        public void UpdatePosition(float deltaTime)
        {
            if (!isActive) return;
            position += velocity * deltaTime;
        }
    }

    public enum DistributionType
    {
        Monodisperse,
        Bimodal,
        WideDistribution,
        RosinRammler,
        Custom
    }

    public class DistributionPreset
    {
        public DistributionType type;
        public List<float> diameters;
        public string description;

        public DistributionPreset(DistributionType t)
        {
            type = t;
            diameters = new List<float>();
            description = GetDescription(t);
            GenerateDistribution();
        }

        private void GenerateDistribution()
        {
            diameters.Clear();
            int baseCount = 100;

            switch (type)
            {
                case DistributionType.Monodisperse:
                    for (int i = 0; i < baseCount; i++)
                    {
                        float d = 10f + Random.Range(-0.5f, 0.5f);
                        diameters.Add(Mathf.Max(0.1f, d));
                    }
                    break;

                case DistributionType.Bimodal:
                    for (int i = 0; i < baseCount * 0.4f; i++)
                    {
                        float d = 5f + Random.Range(-0.8f, 0.8f);
                        diameters.Add(Mathf.Max(0.1f, d));
                    }
                    for (int i = 0; i < baseCount * 0.6f; i++)
                    {
                        float d = 20f + Random.Range(-1.5f, 1.5f);
                        diameters.Add(Mathf.Max(0.1f, d));
                    }
                    break;

                case DistributionType.WideDistribution:
                    float logMean = Mathf.Log(15f);
                    float logStd = 0.5f;
                    for (int i = 0; i < baseCount; i++)
                    {
                        float logD = logMean + logStd * GaussianRandom();
                        float d = Mathf.Exp(logD);
                        diameters.Add(Mathf.Clamp(d, 0.1f, 100f));
                    }
                    break;

                case DistributionType.RosinRammler:
                    float d50 = 12f;
                    float n = 2.0f;
                    for (int i = 0; i < baseCount; i++)
                    {
                        float u = Random.value;
                        float d = d50 * Mathf.Pow(-Mathf.Log(u), 1f / n);
                        diameters.Add(Mathf.Clamp(d, 0.1f, 100f));
                    }
                    break;

                default:
                    for (int i = 0; i < baseCount; i++)
                    {
                        diameters.Add(10f);
                    }
                    break;
            }
        }

        private static float GaussianRandom()
        {
            float u1 = Random.value;
            float u2 = Random.value;
            return Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Cos(2f * Mathf.PI * u2);
        }

        private static string GetDescription(DistributionType t)
        {
            return t switch
            {
                DistributionType.Monodisperse => "Single Disperse (10±0.5 μm)",
                DistributionType.Bimodal => "Bimodal (5μm + 20μm)",
                DistributionType.WideDistribution => "Wide Distribution (1~50 μm)",
                DistributionType.RosinRammler => "Rosin-Rammler Distribution",
                _ => "Unknown"
            };
        }
    }
}
