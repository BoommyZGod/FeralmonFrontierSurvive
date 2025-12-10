using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyChasingState : IEnemyState
{
    public void EnterState(Enemy enemy)
    {
        // ตรวจสอบให้แน่ใจว่า moveSpeed กลับมาเป็นบวก (ถ้าใช้การปรับ moveSpeed ใน PushState)
        enemy.SetMoveSpeed(Mathf.Abs(enemy.MoveSpeed));
    }

    public void UpdateState(Enemy enemy)
    {
        // โค้ดเดิมในการกำหนดทิศทางและการพลิก Sprite
        if (PlayerController.Instance.gameObject.activeSelf)
        {
            if (PlayerController.Instance.transform.position.x > enemy.transform.position.x)
            {
                enemy.SpriteRenderer.flipX = true;
            }
            else
            {
                enemy.SpriteRenderer.flipX = false;
            }

            Vector3 direction = (PlayerController.Instance.transform.position - enemy.transform.position).normalized;
            enemy.Rigidbody.velocity = new Vector2(direction.x * enemy.MoveSpeed, direction.y * enemy.MoveSpeed);
        }
        else
        {
            enemy.Rigidbody.velocity = Vector2.zero;
        }

        // ตรวจสอบการเปลี่ยนสถานะ (Transition)
        if (enemy.PushCounter > 0)
        {
            enemy.TransitionToState(enemy.PushedState);
        }
    }

    public void ExitState(Enemy enemy)
    {
        // ทำความสะอาดเมื่อออกจากสถานะ (ถ้ามี)
    }
}
