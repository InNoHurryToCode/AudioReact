using UnityEngine;

public class MaterialEmissiveColorBehaviour : MaterialsBehaviour
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
        float sample = properties.GetSample();
        LerpMaterialColorProperty(Materials, sample, properties.Smoothing, "_EmissionColor", gradient);
    }
}
