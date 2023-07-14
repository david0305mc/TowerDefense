using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseObj : MonoBehaviour
{
    [SerializeField] protected TextureSheetAnimationScript textureSeetAnimation = default;
    [SerializeField] protected MeshRenderer MeshRenderer = default;
    [SerializeField] private GameObject renderRoot = default;

    protected readonly int defaultTextureScale = 20;
    protected readonly int defaultTextureOffsetX = 0;
    protected readonly int defaultTextureOffsetY = 0;
    protected readonly int defaultGridSize = 1;
    public BaseObjData BaseObjData { get; protected set; }
    public GameType.Direction direction;

    public  static BaseObj Create(BaseObjData objData, GameObject prefab, Transform parent)
    {
        var baseObj = Utill.InstantiateGameObject<BaseObj>(prefab, parent);
        baseObj.SetPosition(GroundManager.Instance.GetRandomFreePosition());
        baseObj.UpdateRenderQuads(DataManager.Instance.GetSpriteSheetData(objData.TID));
        baseObj.BaseObjData = objData;
        return baseObj;
    }

    public void UpdateRenderQuads(DataManager.SpriteSheet _spriteSheet)
    {
        var material = Instantiate(GameManager.Instance.RenderQuadMaterial);
        material.mainTexture = Utill.Load<Texture>(_spriteSheet.respath);
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

        textureSeetAnimation.SetTextureSheetData(_spriteSheet.culumns, _spriteSheet.rows, _spriteSheet.frame, 10);

        if (direction == GameType.Direction.BOTTOM_LEFT || direction == GameType.Direction.LEFT || direction == GameType.Direction.TOP_LEFT)
        {
            this.FlipRenderers(true);
        }
        else
        {
            this.FlipRenderers(false);
        }
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
        Debug.Log($"point {point}  transform.position  {transform.position } angle {angle}");
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
        this.SetDirection(direction);
    }
    private void SetDirection(GameType.Direction direction)
    {
        this.direction = direction;
        this.UpdateConnectedItems();
    }
    private void UpdateConnectedItems()
    {
    }
    
}
