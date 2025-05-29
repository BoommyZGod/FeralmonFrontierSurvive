using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationObjectDestroy : MonoBehaviour
{
    [SerializeField] private Animator animator;

    // Update is called once per frame
    void Start()
    {
        Destroy(gameObject,animator.GetCurrentAnimatorStateInfo(0).length);
    }
}
