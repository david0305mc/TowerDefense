using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

public class CharacterObj : BaseObj
{
    private CancellationTokenSource cts;

    private float speed = 5;
    private GroundManager.Path _path;
    private int _currentNodeIndex;

    public static CharacterObj Create(int tid, GameObject prefab, Transform parent)
    {
        var baseObj = Utill.InstantiateGameObject<CharacterObj>(prefab, parent);
        baseObj.SetPosition(GroundManager.Instance.GetRandomFreePosition());
        baseObj.UpdateSprite(DataManager.Instance.GetSpriteSheetData(tid));

        return baseObj;
    }

    public void UpdateSprite(DataManager.SpriteSheet _spriteSheet)
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
    }

    private void MoveToPosition(Vector3 targetPosition)
    {
        cts?.Clear();
        cts = new CancellationTokenSource();
        UniTaskAsyncEnumerable.EveryUpdate(PlayerLoopTiming.Update).ForEachAsync(_ =>
        {
            float frameDistance = Time.deltaTime * speed;
            float interpolationValue = frameDistance / (targetPosition - transform.localPosition).magnitude;
            transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, interpolationValue);
            if (transform.localPosition == targetPosition)
            {
                WalkNextNode();
            }
        }, cts.Token);
    }

    private void WalkNextNode()
    {
        if (_path != null && _path.nodes != null && _currentNodeIndex < _path.nodes.Length - 1)
        {
            _currentNodeIndex++;
            MoveToPosition(_path.nodes[_currentNodeIndex]);
            //if (this.OnBetweenWalk != null)
            //{
            //    this.OnBetweenWalk.Invoke();
            //}
        }
        else
        {
            FinishWalk();
        }
    }


    public void WalkToPosition(Vector3 position)
    {
        //CancelWalk();
        WalkThePath(GroundManager.Instance.GetPath(transform.localPosition, position, false));
    }

    private void WalkThePath(GroundManager.Path path)
    {
        if (path.nodes == null || path.nodes.Length == 0)
        {
            FinishWalk();
            return;
        }

        //_baseItem.SetState(GameData.State.WALK);
        _path = path;
        _currentNodeIndex = 0;
        if (path != null || path.nodes != null && path.nodes.Length > 0)
        {
            MoveToPosition(_path.nodes[0]);
        }
    }

    private void FinishWalk()
    {
        cts?.Clear();
        cts = null;

        //_baseItem.SetState(GameData.State.IDLE);
        //_isWalking = false;
        //_targetPosition = transform.position;
        //if (OnFinishWalk != null)
        //{
        //    OnFinishWalk.Invoke();
        //}

        //OnBetweenWalk = null;
    }

    private void OnDestroy()
    {
        cts?.Clear();
        cts = null;
    }
}
