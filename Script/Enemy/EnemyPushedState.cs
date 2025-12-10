using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyPushedState : IEnemyState
{
    public void EnterState(Enemy enemy)
    {
        // เริ่มต้นการผลัก
        if (enemy.MoveSpeed > 0)
        {
            enemy.SetMoveSpeed(-enemy.MoveSpeed); // กลับทิศทางความเร็ว
        }
    }

    public void UpdateState(Enemy enemy)
    {
        enemy.PushCounter -= Time.deltaTime; // นับถอยหลัง

        // การเคลื่อนที่ถอยหลังจะถูกจัดการด้วย rb.velocity ใน State เดิม
        // เราแค่ต้องการให้มันหมดเวลา แล้วกลับไป ChasingState

        // ตรวจสอบการเปลี่ยนสถานะ (Transition)
        if (enemy.PushCounter <= 0)
        {
            enemy.TransitionToState(enemy.ChasingState);
        }
    }

    public void ExitState(Enemy enemy)
    {
        // โค้ดเดิมในการทำให้ moveSpeed กลับมาเป็นบวกเมื่อถูกผลักเสร็จ
        enemy.SetMoveSpeed(Mathf.Abs(enemy.MoveSpeed));
    }

}
