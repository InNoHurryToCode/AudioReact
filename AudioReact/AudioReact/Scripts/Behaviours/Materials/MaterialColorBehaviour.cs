using UnityEngine;

public class MaterialColorBehaviour : MaterialsBehaviour
{
    public BehaviorProperties properties;
    public Material[] Materials;
    public Gradient gradient;

    private void Awake()
    {
        CheckMaterials(Materials);
    }

    private void Update()
    {
        float sample = properties.GetSample(properties.FrequencyRange, properties.Sensitivity, properties.ClampMin, properties.ClampMax);
        LerpMaterialColorProperty(Materials, sample, properties.Smoothing, "_Color", gradient);
    }
}