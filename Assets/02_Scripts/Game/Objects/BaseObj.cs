using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObj : MonoBehaviour
{
    [SerializeField] private MeshRenderer MeshRenderer = default;
    [SerializeField] private GameObject renderQuadObject = default;

    private readonly int defaultTextureScale = 20;
    private readonly int defaultTextureHeight = 10;
    private readonly int defaultTextureWidth = 10;
    private readonly int defaultTextureOffsetX = 0;
    private readonly int defaultTextureOffsetY = 0;
    private readonly int defaultGridSize = 1;


    public void UpdateQuad()
    {
        var material = Instantiate(GameManager.Instance.RenderQuadMaterial);
        material.mainTexture = GameManager.Instance.GridTexture;
        MeshRenderer.material = material;

        Vector3 defaultImgSize = new Vector3(1.4142f, 1.4142f, 1.4142f) * 4 * defaultTextureScale / 100.0f / defaultGridSize;
        float heightFactor = ((float)defaultTextureHeight / (float)defaultTextureWidth);

        float offsetX = (1.414f / 256.0f) * defaultTextureOffsetX * 4 / defaultGridSize;
        float offsetY = (1.414f / 256.0f) * defaultTextureOffsetY * 4 / defaultGridSize;

        renderQuadObject.transform.localPosition = new Vector3(offsetX, offsetY, 0);
        renderQuadObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        renderQuadObject.transform.localScale = new Vector3(defaultImgSize.x, defaultImgSize.x * heightFactor, 1);
    }
}
