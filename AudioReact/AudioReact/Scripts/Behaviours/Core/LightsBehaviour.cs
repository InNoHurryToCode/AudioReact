using UnityEngine;

public class LightsBehaviour : MonoBehaviour
{
    public void CheckLights(Light[] lights)
    {
        if (lights != null)
        {
            for (int i = 0; i < lights.Length; i++)
            {
                if (lights[i] == null)
                {
                    Debug.LogError("AudioReactBehaviourLights: lights[ " + i + "] not assigned");
                }
            }
        }
        else
        {
            Debug.LogError("AudioReactBehaviourLights: lights not assigned");
        }
    }

    public void LerpLightColor(Light[] lights, float sample, float smoothing, Gradient gradient)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].color = Color.Lerp(lights[i].color, gradient.Evaluate(sample), smoothing * Time.deltaTime);
        }
    }

    public void LerpLightIntensity(Light[] lights, float sample, float smoothing)
    {
        for (int i = 0; i < lights.Length; i++)
        {
            lights[i].intensity = Mathf.Lerp(lights[i].intensity, sample, smoothing * Time.deltaTime);
        }
    }
}