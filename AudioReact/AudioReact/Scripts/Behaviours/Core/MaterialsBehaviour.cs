using UnityEngine;

public class MaterialsBehaviour : MonoBehaviour
{
    public void CheckMaterials(Material[] materials)
    {
        if (materials != null)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i] != null)
                {
                    if (materials[i].shader != Shader.Find("Standard"))
                    {
                        Debug.LogError("AudioReactBehaviourMaterials: materials[ " + i + "] does not use Standard shader");
                    }
                }
                else
                {
                    Debug.LogError("AudioReactBehaviourMaterials: materials[ " + i + "] not assigned");
                }
            }
        }
        else
        {
            Debug.LogError("AudioReactBehaviourMaterials: materials not assigned");
        }
    }

    public void LerpMaterialFloatProperty(Material[] materials, float sample, float smoothing, string property)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetFloat(property, Mathf.Lerp(materials[i].GetFloat(property), sample, smoothing * Time.deltaTime));
        }
    }

    public void LerpMaterialColorProperty(Material[] materials, float sample, float smoothing, string property, Gradient gradient)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            materials[i].SetColor(property, Color.Lerp(materials[i].GetColor(property), gradient.Evaluate(sample), smoothing * Time.deltaTime));
        }
    }

    public void LerpMaterialAlphaProperty(Material[] materials, float sample, float smoothing, string property)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            Color color = materials[i].GetColor(property);
            color.a = Mathf.Lerp(color.a, sample, smoothing * Time.deltaTime);
            materials[i].SetColor(property, color);
        }
    }
}