using UnityEngine;

public class nBone : MonoBehaviour
{
    // ...existing code...

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.1f);

        if (transform.parent != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.parent.position);
        }
    }
}
