using UnityEngine;

public class LightColorBehaviour : LightsBehaviour
{
    public BehaviorProperties properties;
    public Light[] Lights;
    public Gradient Gradient;

    private void Awake()
    {
        CheckLights(Lights);
    }

    private void Update()
    {
        float sample = properties.GetSample();
        LerpLightColor(Lights, sample, properties.Smoothing, Gradient);
    }
}
