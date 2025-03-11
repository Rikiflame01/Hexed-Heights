using UnityEngine;

public class CollisionHandler : MonoBehaviour
{

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            SoundManager.Instance.PlaySoundEffectByName("wood-thud", 1f);
        }
    }

}
