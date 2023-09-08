using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FilteredReferences : MonoBehaviour
{
    // AssetReferenceT<원하는 타입> 을 상속 받습니다.
    // AssetReferenceMaterial 를 통해서 Material 타입의 에셋을 참조할 수 있습니다.
    [Serializable]
    public class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        public AssetReferenceMaterial(string guid) : base(guid) { }
    }

    private SpriteRenderer spriteRenderer;

    // Material 타입 에셋을 참조할 수 있는 커스텀 변수입니다.
    public AssetReferenceMaterial materialReference;
    public AssetReferenceSprite spriteReference;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void OnLoadSprite()
    {
        spriteReference.LoadAssetAsync().Completed += SpriteLoaded;
    }

    public void OnLoadMaterial()
    {
        materialReference.LoadAssetAsync().Completed += MaterialLoaded;
    }

    public void OnUnLoadSprite()
    {
        // ReleaseAsset() 을 통해 메모리에 올라간 에셋레퍼런스를 해제합니다.
        spriteReference.ReleaseAsset();
    }

    public void OnUnLoadMaterial()
    {
        // ReleaseAsset() 을 통해 메모리에 올라간 에셋레퍼런스를 해제합니다.
        materialReference.ReleaseAsset();
    }


    private void SpriteLoaded(AsyncOperationHandle<Sprite> obj)
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                spriteRenderer.sprite = obj.Result;
                break;

            case AsyncOperationStatus.Failed:
                Debug.Log("스프라이트 로드 실패");
                break;

            default:
                break;
        }
    }

    private void MaterialLoaded(AsyncOperationHandle<Material> obj)
    {
        switch (obj.Status)
        {
            case AsyncOperationStatus.Succeeded:
                spriteRenderer.material = obj.Result;
                break;

            case AsyncOperationStatus.Failed:
                Debug.Log("스프라이트 로드 실패");
                break;

            default:
                break;
        }
    }
}