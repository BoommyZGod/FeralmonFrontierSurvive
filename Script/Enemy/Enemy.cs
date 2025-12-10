using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public static event Action<int> OnEnemyKilled;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Rigidbody2D rb;
    private Vector3 direction;

    [SerializeField] private float moveSpeed;
    [SerializeField] private float damage;
    [SerializeField] private float health;
    [SerializeField] private int experienceToGive;
    [SerializeField] private float pushTime;

    private float pushCounter;
    [SerializeField] private GameObject destroyEffect;

    // STATE PATTERN: Context Fields
    private IEnemyState currentState;
    public IEnemyState ChasingState { get; private set; } // สถานะไล่ล่า
    public IEnemyState PushedState { get; private set; }  // สถานะถูกผลัก

    // Public Getters/Setters สำหรับ State Classes
    public float MoveSpeed => moveSpeed;
    public float PushCounter { get => pushCounter; set => pushCounter = value; }
    public SpriteRenderer SpriteRenderer => spriteRenderer;
    public Rigidbody2D Rigidbody => rb;
    public void SetMoveSpeed(float newSpeed) => moveSpeed = newSpeed;

    void Awake()
    {
        // สร้าง Instance ของ State ต่างๆ
        ChasingState = new EnemyChasingState();
        PushedState = new EnemyPushedState();

        // เริ่มต้นด้วยสถานะไล่ล่า
        currentState = ChasingState;
        currentState.EnterState(this);
    }

    // *** เพิ่ม Update() สำหรับ Logic และ Timer (Time.deltaTime) ***
    void Update()
    {
        // ให้ State ปัจจุบันจัดการ Logic/Timer (เช่น ลด pushCounter)
        currentState.UpdateState(this);
    }
    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController.Instance.TakeDamage(damage);
        }
    }

    // FixedUpdate ใช้สำหรับฟิสิกส์ (Rigidbody.velocity)
    void FixedUpdate()
    {
        // ให้ State ปัจจุบันจัดการ Movement/Velocity
        // ใน StateChasingState จะมีการคำนวณและตั้งค่า rb.velocity
        // ใน StatePushedState จะปล่อยให้ rb.velocity ถูกใช้ต่อไปจนหมดเวลา
        // เนื่องจาก UpdateState จัดการทั้ง Timer และ Movement ในโค้ดตัวอย่างของเรา
        // เราสามารถเรียกซ้ำที่นี่ หรือปรับให้ State มี FixedUpdateState()
        // เพื่อความง่ายตามตัวอย่างเดิม ให้ UpdateState จัดการทั้งหมด และเราเรียก UpdateState ใน Update()
        // แต่เพื่อควบคุม FixedUpdate อย่างถูกต้อง ให้เราเรียกใช้ UpdateState ใน Update() เท่านั้น 
        // แล้วให้ State ตั้งค่า rb.velocity เอง

        // ถ้าต้องการให้ Movement เกิดใน FixedUpdate ตามชื่อเมธอด
        // เราสามารถเพิ่ม FixedUpdateState ใน IEnemyState
        // แต่ในตัวอย่างนี้ เราจะใช้ UpdateState ใน Update() และปล่อยให้มันจัดการ rb.velocity 
        // (ซึ่งจะทำงานได้ แต่ FixedUpdate จะไม่สามารถควบคุมความสม่ำเสมอได้ 100% ถ้าใช้ Time.deltaTime)

        // เพื่อให้ตรงตามหลักการของ Unity: Movement should be here, and logic in Update()
        // *** แต่เนื่องจาก UpdateState ใน Concrete States ของเราจัดการทั้งสองอย่าง ผมจะลบโค้ดนี้ออกเพื่อป้องกันการเรียกซ้ำ
        // โดยอาศัยว่า FixedUpdate ถูกเรียกถี่กว่า Update และ Movement ก็จะถูกควบคุมใน UpdateState ที่เรียกใน Update()
    }

    // เมธอดสำหรับเปลี่ยนสถานะ
    public void TransitionToState(IEnemyState newState)
    {
        currentState.ExitState(this);
        currentState = newState;
        currentState.EnterState(this);
    }

    

    public void TakeDamage(float damage)
    {
        health -= damage;
        DamageNumberController.Instance.CreateNumber(damage, transform.position);
        pushCounter = pushTime;

        // เมื่อได้รับความเสียหาย และ pushCounter > 0, State Pattern จะจัดการการเปลี่ยนไป PushedState เองใน UpdateState
        // ถ้าต้องการให้เปลี่ยนสถานะทันที: 
        // TransitionToState(PushedState); 

        if (health <= 0)
        {
            /*if (PlayerController.Instance != null)
            {
                PlayerController.Instance.GetExperience(experienceToGive); // ส่ง EXP ให้ผู้เล่นโดยตรง
            }

            // 2. ทำลายวัตถุและเอฟเฟกต์
            Destroy(gameObject);
            Instantiate(destroyEffect, transform.position, transform.rotation);
            AudioController.Instance.PlayModifiedSound(AudioController.Instance.enemyDie);*/

            OnEnemyKilled?.Invoke(experienceToGive);

            
            Destroy(gameObject);
            Instantiate(destroyEffect, transform.position, transform.rotation);
            AudioController.Instance.PlayModifiedSound(AudioController.Instance.enemyDie);
        }
    }
}