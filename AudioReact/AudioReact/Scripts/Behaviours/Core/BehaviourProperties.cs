using UnityEngine;
using AudioReact;

[System.Serializable]
public class BehaviorProperties
{
    public FrequencyRange FrequencyRange = FrequencyRange.Decibel;
    public float Sensitivity = 10;
    public float Smoothing = 1.0f;
    public float ClampMin = 0.0f;
    public float ClampMax = 1.0f;

    public BehaviorProperties()
    {
        if (ClampMax < ClampMin)
        {
            Debug.LogError("AudioReactBehaviorProperties: clamp max cannot be lower than clamp min");
        }
    }

    public float GetSample()
    {
        float sample = AudioReactSampler.Instance.FrequencySamples[(int)FrequencyRange] * Sensitivity;

        if (float.IsNaN(sample))
        {
            sample = 0;
        }

        sample = Mathf.Lerp(ClampMin, ClampMax, sample);

        return sample;
    }
}
