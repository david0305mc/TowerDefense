using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class FilteredReferences : MonoBehaviour
{
    // AssetReferenceT<���ϴ� Ÿ��> �� ��� �޽��ϴ�.
    // AssetReferenceMaterial �� ���ؼ� Material Ÿ���� ������ ������ �� �ֽ��ϴ�.
    [Serializable]
    public class AssetReferenceMaterial : AssetReferenceT<Material>
    {
        public AssetReferenceMaterial(string guid) : base(guid) { }
    }

    private SpriteRenderer spriteRenderer;

    // Material Ÿ�� ������ ������ �� �ִ� Ŀ���� �����Դϴ�.
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
        // ReleaseAsset() �� ���� �޸𸮿� �ö� ���·��۷����� �����մϴ�.
        spriteReference.ReleaseAsset();
    }

    public void OnUnLoadMaterial()
    {
        // ReleaseAsset() �� ���� �޸𸮿� �ö� ���·��۷����� �����մϴ�.
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
                Debug.Log("��������Ʈ �ε� ����");
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
                Debug.Log("��������Ʈ �ε� ����");
                break;

            default:
                break;
        }
    }
}