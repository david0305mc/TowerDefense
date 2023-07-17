using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class ResourceManager : Singleton<ResourceManager>
{
    private Dictionary<Texture2D, Material> textureMaterialMap;
    private Dictionary<string, Texture2D> textureMap;
        

    protected override void init()
    {
        textureMaterialMap = new Dictionary<Texture2D, Material>();
        textureMap = new Dictionary<string, Texture2D>();
    }

    public Texture2D GetTexture(string _path)
    {
        Texture2D texture;
        if (!textureMap.TryGetValue(_path, out texture))
        {
            texture = Utill.Load<Texture2D>(_path);
            textureMap.Add(_path, texture);
        }
        return texture;
    }

    public Material GetTextureMaterial(Texture2D texture, RenderingLayer layer, int order)
    {
        Material material = null;
        if (!textureMaterialMap.TryGetValue(texture, out material))
        {
            material = Object.Instantiate(GameManager.Instance.RenderQuadMaterial) as Material;
            material.mainTexture = texture;
            textureMaterialMap.Add(texture, material);
        }        
        return material;
    }

}
