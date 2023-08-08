using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
public class BaseObj : MonoBehaviour
{

    [SerializeField] protected TextureSheetAnimationScript textureSeetAnimation = default;
    [SerializeField] protected MeshRenderer MeshRenderer = default;
    [SerializeField] private GameObject renderRoot = default;

    protected readonly int defaultTextureOffsetX = 0;
    protected readonly int defaultTextureOffsetY = 0;
    protected readonly int defaultGridSize = 1;
    public BaseObjData BaseObjData { get; protected set; }

    public  static BaseObj Create(BaseObjData objData, GameObject prefab, Transform parent)
    {
        var baseObj = Utill.InstantiateGameObject<BaseObj>(prefab, parent);
        baseObj.BaseObjData = objData;
        baseObj.SetPosition(new Vector3(objData.X, 0, objData.Y));
        baseObj.StartFSM();
        baseObj.UpdateRenderQuads();
        return baseObj;
    }
    public virtual void StartFSM()
    {
    }

    protected virtual DataManager.SpriteCollection GetSpriteCollection()
    {
        return DataManager.Instance.GetSpriteCollectionData(BaseObjData.RefObjData.idle_collectionid);
    }

    public void UpdateRenderQuads()
    {
        var collectionData = GetSpriteCollection();
        DataManager.SpriteSheet _spriteSheet = DataManager.Instance.GetSpriteSheetData(collectionData.GetSpriteCollection(BaseObjData.Direction));
        MeshRenderer.material = ResourceManager.Instance.GetTextureMaterial(ResourceManager.Instance.GetTexture(_spriteSheet.respath), RenderingLayer.GROUND, 1);
        
        Vector3 defaultImgSize = new Vector3(1.4142f, 1.4142f, 1.4142f) * 4 * _spriteSheet.scale / 100.0f / defaultGridSize;
        float heightFactor = (MeshRenderer.material.mainTexture.height / (float)MeshRenderer.material.mainTexture.width) * ((float)_spriteSheet.culumns / _spriteSheet.rows);

        float offsetX = (1.414f / 256.0f) * defaultTextureOffsetX * 4 / defaultGridSize;
        float offsetY = (1.414f / 256.0f) * defaultTextureOffsetY * 4 / defaultGridSize;

        MeshRenderer.gameObject.transform.localPosition = new Vector3(offsetX, offsetY, 0);
        MeshRenderer.gameObject.transform.localRotation = Quaternion.Euler(Vector3.zero);
        MeshRenderer.gameObject.transform.localScale = new Vector3(defaultImgSize.x, defaultImgSize.x * heightFactor, 1);

        textureSeetAnimation.SetTextureSheetData(_spriteSheet.culumns, _spriteSheet.rows, _spriteSheet.frame, 10);
        if (BaseObjData.Direction == GameType.Direction.BOTTOM_LEFT || BaseObjData.Direction == GameType.Direction.LEFT || BaseObjData.Direction == GameType.Direction.TOP_LEFT)
        {
            this.FlipRenderers(true);
        }
        else
        {
            this.FlipRenderers(false);
        }
        RandomizeRenderQuadsPosition();
    }

    private void RandomizeRenderQuadsPosition()
    {
        Vector3 randomDeltaPosition = new Vector3(Random.Range(-0.3f, 0.3f), 0, Random.Range(-0.3f, 0.3f));
        MeshRenderer.gameObject.transform.localPosition += randomDeltaPosition;
    }

    private void FlipRenderers(bool value)
    {
        if (value)
        {
            renderRoot.transform.localScale = new Vector3(-1, 1, 1);
        }
        else
        {
            renderRoot.transform.localScale = new Vector3(1, 1, 1);
        }
    }
    protected void LookAt(Vector3 point)
    {
        Vector2 a = GameUtil.GetScreenPosition(CameraManager.Instance.MainCamera, transform.position - new Vector3(0, 0, 1));
        Vector2 b = GameUtil.GetScreenPosition(CameraManager.Instance.MainCamera, transform.position);
        Vector2 c = GameUtil.GetScreenPosition(CameraManager.Instance.MainCamera, point);

        float angle = GameUtil.ClockwiseAngleOf3Points(a, b, c);
        SetAngle(angle);
    }

    public void SetAngle(float angle)
    {
        GameType.Direction direction = GameType.Direction.BOTTOM_RIGHT;
        float minAnge = 999;
        //Debug.Log($"angle {angle}");
        foreach (KeyValuePair<float, GameType.Direction> entry in GameConfig.AngleToDirectionMap)
        {
            float a = Mathf.Abs(angle - entry.Key);
            if (a < minAnge)
            {
                minAnge = a;
                direction = entry.Value;
            }
        }

        if (BaseObjData.Direction != direction)
        {
            BaseObjData.Direction = direction;
            UpdateRenderQuads();
        }
    }
    private void UpdateConnectedItems()
    {
    }
    
}
