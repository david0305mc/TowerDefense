using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileStraight : MonoBehaviour
{
    [SerializeField] private Rigidbody2D rigidBody2d;
    [SerializeField] private AnimationCurve curve;
    [SerializeField] private SpriteRenderer spriteRenderer;
    private Vector3 srcPos;
    private Vector3 dstPos;

    private float elapse;
    private float speed;
    private Vector2 prevPos;
    MEnemyObj enemyObj;

    private Quaternion quaternionRot;
    private bool ToEnemy;

    public void UpdateData(int _itemTID)
    {
        //DataManager.ItemLevel itemData = DataManager.Instance.GetItemLevelData(_itemTID);
        //spriteRenderer.SetSprite(itemData.iconpath);
    }

    public void Shoot(MEnemyObj _enemyObj, float _speed)
    {
        enemyObj = _enemyObj;
        dstPos = enemyObj.transform.position;
        srcPos = transform.position;
        elapse = 0f;
        speed = _speed;
        prevPos = srcPos;
        ToEnemy = true;
    }

    private void FixedUpdate()
    {
        UpdateMissile();
    }

    private void UpdateMissile()
    {
        if (enemyObj != null)
        {
            dstPos = enemyObj.transform.position;
        }

        float dist = Vector2.Distance(srcPos, dstPos);
        elapse += Time.deltaTime / dist * 5f;

        var height = curve.Evaluate(elapse);

        var pos = Vector2.Lerp(srcPos, dstPos, elapse) + new Vector2(0, height);
        rigidBody2d.MovePosition(pos);
        rigidBody2d.MoveRotation(GameUtil.LookAt2D(prevPos, pos, GameUtil.FacingDirection.RIGHT));

        prevPos = pos;
        if (elapse >= 1)
        {
            if (enemyObj != null)
            {
                var enemyData = UserData.Instance.GetEnemyData(enemyObj.UID);
                if (enemyData != null)
                {
                    Debug.Log("enemyData != null");
                    if (enemyData.hp > 0)
                    {
                        Debug.Log("enemyData.hp > 0");
                    }
                }
            }
            
            //Debug.Log($"Dispose boomed : {boomed}");
            Dispose();
        }
    }

    private void Dispose()
    {
        Lean.Pool.LeanPool.Despawn(gameObject);
        elapse = 0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var damagable = collision.GetComponent<Damageable>();
        if (damagable != null)
        {
            damagable.GetDamaged(1);
            MGameManager.Instance.ShowBoomEffect(0, collision.ClosestPoint(transform.position));
            Dispose();
        }
    }
}