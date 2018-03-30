using UnityEngine;

public class MaterialAmbientOcclusionBehaviour : MaterialsBehaviour
{
    public BehaviorProperties properties;
    public Material[] Materials;

    private void Awake()
    {
        CheckMaterials(Materials);
    }

    private void Update()
    {
        float sample = properties.GetSample();
        LerpMaterialFloatProperty(Materials, sample, properties.Smoothing, "_OcclusionStrength");
    }
}
