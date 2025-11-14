using UnityEngine;

public class DestroyOnAnimationEnd : MonoBehaviour
{
    private Animator anim;
    private float timer;

    void Awake() => anim = GetComponent<Animator>();

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= anim.GetCurrentAnimatorStateInfo(0).length)
            Destroy(gameObject);
    }
}
