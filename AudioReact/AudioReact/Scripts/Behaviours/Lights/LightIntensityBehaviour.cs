using UnityEngine;

public class LightIntensityBehaviour : LightsBehaviour
{
    public BehaviorProperties properties;
    public Light[] Lights;

    private void Awake()
    {
        CheckLights(Lights);
    }

    private void Update()
    {
        float sample = properties.GetSample();
        LerpLightIntensity(Lights, sample, properties.Smoothing);
    }
}
