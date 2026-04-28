using System;
using System.Collections.Generic;
using System.Linq;
using DefaultNamespace.GrayScale;
using UnityEngine;

public class GrayscaleToggle : MonoBehaviour, IGrayScalable
{
    private Material colorMaterial;
    public Material grayscaleMaterial;

    private Renderer _renderer;
    private bool _isGrayscale;

    void Start()
    {
        _renderer = GetComponent<Renderer>();
        // https://stackoverflow.com/questions/65455308/how-to-get-all-the-materials-assigned-to-a-gameobject-in-unity
        List<Material> myMaterials = GetComponent<Renderer>().materials.ToList();
        colorMaterial =  myMaterials[0];
    }
    
    public void ToggleGrayScale()
    {
        _isGrayscale = !_isGrayscale;
        _renderer.material = _isGrayscale ? grayscaleMaterial : colorMaterial;
        Debug.Log("Toggle Grayscale on Object {}", gameObject);
    }
}
