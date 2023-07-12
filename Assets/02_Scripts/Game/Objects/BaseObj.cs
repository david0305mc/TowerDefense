using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObj : MonoBehaviour
{
    [SerializeField] private TextureSheetAnimationScript textureSeetAnimation = default;
    [SerializeField] private MeshRenderer MeshRenderer = default;

    private readonly int defaultTextureScale = 20;
    private readonly int defaultTextureOffsetX = 0;
    private readonly int defaultTextureOffsetY = 0;
    private readonly int defaultGridSize = 1;


    public void CreateQuad()
    {
        var material = Instantiate(GameManager.Instance.RenderQuadMaterial);
        material.mainTexture = GameManager.Instance.GridTexture;
        MeshRenderer.material = material;
        int numOfCulumns = 1;
        int numOfRows = 1;

        Vector3 defaultImgSize = new Vector3(1.4142f, 1.4142f, 1.4142f) * 4 * defaultTextureScale / 100.0f / defaultGridSize;
        float heightFactor = (material.mainTexture.height / (float)material.mainTexture.width) * ((float)numOfCulumns / numOfRows);

        float offsetX = (1.414f / 256.0f) * defaultTextureOffsetX * 4 / defaultGridSize;
        float offsetY = (1.414f / 256.0f) * defaultTextureOffsetY * 4 / defaultGridSize;

        MeshRenderer.gameObject.transform.localPosition = new Vector3(offsetX, offsetY, 0);
        MeshRenderer.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        MeshRenderer.gameObject.transform.localScale = new Vector3(defaultImgSize.x, defaultImgSize.x * heightFactor, 1);
        textureSeetAnimation.SetTextureSheetData(numOfCulumns, numOfRows, 1, 10);
    }

    public void CreateSwordMan()
    {
        var material = Instantiate(GameManager.Instance.RenderQuadMaterial);
        material.mainTexture = GameManager.Instance.CharacterTexture;
        MeshRenderer.material = material;
        int numOfCulumns = 4;
        int numOfRows = 2;

        Vector3 defaultImgSize = new Vector3(1.4142f, 1.4142f, 1.4142f) * 4 * defaultTextureScale / 100.0f / defaultGridSize;
        float heightFactor = (material.mainTexture.height / (float)material.mainTexture.width) * ((float)numOfCulumns / numOfRows);

        float offsetX = (1.414f / 256.0f) * defaultTextureOffsetX * 4 / defaultGridSize;
        float offsetY = (1.414f / 256.0f) * defaultTextureOffsetY * 4 / defaultGridSize;

        MeshRenderer.gameObject.transform.localPosition = new Vector3(offsetX, offsetY, 0);
        MeshRenderer.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        MeshRenderer.gameObject.transform.localScale = new Vector3(defaultImgSize.x, defaultImgSize.x * heightFactor, 1);

        textureSeetAnimation.SetTextureSheetData(numOfCulumns, numOfRows, 8, 10);
    }
}
