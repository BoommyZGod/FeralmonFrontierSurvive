using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DamageNumber : MonoBehaviour
{
    [SerializeField] private TMP_Text damageText;
    private float floatSpeed ;

    void Start()
    {
        floatSpeed = Random.Range(0.1f, 1.5f);
        Destroy(gameObject ,1);
    }

    
    void Update()
    {
        transform.position += Vector3.up * Time.deltaTime * floatSpeed;
    }

    public void SetValue(int Value)
    {
        damageText.text = Value.ToString();
    }
}
