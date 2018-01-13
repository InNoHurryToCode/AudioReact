using UnityEngine;

public class MaterialEmissiveIntensityBehaviour : MaterialsBehaviour
{
    public BehaviorProperties properties;
    public Material[] Materials;

    private void Awake()
    {
        CheckMaterials(Materials);
    }

    private void Update()
    {
        float sample = properties.GetSample(properties.FrequencyRange, properties.Sensitivity, properties.ClampMin, properties.ClampMax);
        LerpMaterialAlphaProperty(Materials, sample, properties.Smoothing, "_EmissionColor");
    }
}