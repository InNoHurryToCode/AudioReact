using UnityEngine;

public class MaterialSmoothnessBehaviour : MaterialsBehaviour
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
        LerpMaterialFloatProperty(Materials, sample, properties.Smoothing, "_Glossiness");
    }
}