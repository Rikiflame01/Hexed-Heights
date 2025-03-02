using System;
using UnityEngine;

public class CollisionStick : MonoBehaviour
{
    public float forceAmount = 10f;
    private Rigidbody rb;

    public Transform StickResetPoint;

    private void OnEnable()
    {
        ActionManager.OnStickReset += ResetStick;
    }

    private void ResetStick()
    {
        gameObject.transform.position = StickResetPoint.position;
    }
    private void OnDisable()
    {
        ActionManager.OnStickReset -= ResetStick;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("CollisionStick: No Rigidbody found on this object. Please attach a Rigidbody.");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (rb == null)
            return;


        if (collision.gameObject.layer == LayerMask.NameToLayer("Block"))
        {
            TurnManager.Instance.MarkBlockAsTouched(collision.gameObject);
        }

        ContactPoint contact = collision.contacts[0];
        Debug.DrawRay(contact.point, transform.position - contact.point, Color.red, 2f);

        Vector3 contactDirection = contact.point - transform.position;
        Vector3 absDir = new Vector3(
            Mathf.Abs(contactDirection.x),
            Mathf.Abs(contactDirection.y),
            Mathf.Abs(contactDirection.z)
        );

        Vector3 forceDir = Vector3.zero;
        if (absDir.x > absDir.y && absDir.x > absDir.z)
        {
            forceDir = (contactDirection.x < 0) ? Vector3.right : Vector3.left;
        }
        else if (absDir.y > absDir.x && absDir.y > absDir.z)
        {
            forceDir = (contactDirection.y < 0) ? Vector3.up : Vector3.down;
        }
        else if (absDir.z > absDir.x && absDir.z > absDir.y)
        {
            forceDir = (contactDirection.z < 0) ? Vector3.forward : Vector3.back;
        }

        rb.AddForce(forceDir * forceAmount, ForceMode.Impulse);
    }
}
    