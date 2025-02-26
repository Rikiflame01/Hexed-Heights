using UnityEngine;
using System.Collections;

public class StickStick : MonoBehaviour
{

    public float stickDuration = 5f;

    public float positionSpring = 10000f;
    public float positionDamper = 100f;
    public float maximumForce = Mathf.Infinity;

    public LineRenderer jointLineRenderer;

    private ConfigurableJoint activeJoint;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer != LayerMask.NameToLayer("Block"))
            return;

        Rigidbody otherRb = collision.rigidbody;
        if (otherRb == null)
        {
            Debug.LogWarning("Colliding object does not have a Rigidbody. Joint cannot be created.");
            return;
        }

        if (collision.gameObject.GetComponent<ConfigurableJoint>() != null)
            return;

        Vector3 collisionPoint = collision.contacts[0].point;

        Vector3 localAnchorOther = collision.transform.InverseTransformPoint(collisionPoint);
        Vector3 localAnchorThis = transform.InverseTransformPoint(collisionPoint);

        ConfigurableJoint joint = collision.gameObject.AddComponent<ConfigurableJoint>();

        Rigidbody thisRb = GetComponent<Rigidbody>();
        if (thisRb == null)
        {
            Debug.LogWarning("No Rigidbody found on the object with CollisionStickWithConfigurableJoint. Joint cannot be established.");
            Destroy(joint);
            return;
        }
        joint.connectedBody = thisRb;
        joint.anchor = localAnchorOther;
        joint.connectedAnchor = localAnchorThis;

        joint.angularXMotion = ConfigurableJointMotion.Locked;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;

        joint.xMotion = ConfigurableJointMotion.Limited;
        joint.yMotion = ConfigurableJointMotion.Limited;
        joint.zMotion = ConfigurableJointMotion.Limited;


        JointDrive drive = new JointDrive();
        drive.positionSpring = positionSpring;
        drive.positionDamper = positionDamper;
        drive.maximumForce = maximumForce;

        joint.xDrive = drive;
        joint.yDrive = drive;
        joint.zDrive = drive;

        joint.autoConfigureConnectedAnchor = false;

        activeJoint = joint;

        StartCoroutine(RemoveJointAfterDuration(joint));
    }

    private IEnumerator RemoveJointAfterDuration(ConfigurableJoint joint)
    {
        yield return new WaitForSeconds(stickDuration);
        if (joint != null)
        {
            Destroy(joint);
            activeJoint = null;
            if (jointLineRenderer != null)
                jointLineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (activeJoint != null && jointLineRenderer != null)
        {
            Vector3 worldAnchorOther = activeJoint.gameObject.transform.TransformPoint(activeJoint.anchor);
            Vector3 worldAnchorThis = activeJoint.connectedBody.transform.TransformPoint(activeJoint.connectedAnchor);

            jointLineRenderer.SetPosition(0, worldAnchorOther);
            jointLineRenderer.SetPosition(1, worldAnchorThis);

            if (!jointLineRenderer.enabled)
                jointLineRenderer.enabled = true;
        }
        else if (jointLineRenderer != null && jointLineRenderer.enabled)
        {
            jointLineRenderer.enabled = false;
        }
    }
}
