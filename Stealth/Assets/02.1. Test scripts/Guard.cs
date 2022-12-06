using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public float speed = 5;
    public float waitTime = 2f;
    public float turnSpeed = 180;

    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    private float viewAngle;

    public Transform pathHolder;
    private Transform player; // �÷��̾��� Transform
    private Color originalSpotlightColor; // ����Ʈ����Ʈ�� �⺻��

    private Animator animator;

    private void Start()
    {
        viewAngle = spotlight.spotAngle;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpotlightColor = spotlight.color;

        animator = GetComponent<Animator>();

        Vector3[] waypoints = new Vector3[pathHolder.childCount];
        for (int i = 0; i < waypoints.Length; i++)
        {
            waypoints[i] = pathHolder.GetChild(i).position;
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    private void Update()
    {
        if (CanSeePlayer())
        {
            spotlight.color = Color.red;
        }
        else
        {
            spotlight.color = originalSpotlightColor;
        }
    }

    bool CanSeePlayer()
    {
        // �÷��̾ �þ� �Ÿ� �ݰ� �ȿ� �ִٸ�
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            // �÷��̾ �� �ޱ� �ȿ� �ִٸ�
            if (angleBetweenGuardAndPlayer < viewAngle / 2f)
            {
                // �÷��̾ ��ֹ��� �������� �ʾҴٸ�
                if (!Physics.Linecast (transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3 [] waypoints)
    {
        transform.position = waypoints [0];

        int targetWaypointIndex = 1;
        Vector3 targetwaypoint = waypoints[targetWaypointIndex];
        transform.LookAt(targetwaypoint);

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetwaypoint, speed * Time.deltaTime);
            animator.SetBool("isMove", true);
            if (transform.position == targetwaypoint)
            {
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetwaypoint = waypoints[targetWaypointIndex];

                animator.SetBool("isMove", false);
                yield return new WaitForSeconds (waitTime);
                yield return StartCoroutine(TurnToFace(targetwaypoint));
            }
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            transform.eulerAngles = Vector3.up * angle;
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            //Gizmos.DrawSphere(waypoint.position, 1f);
            Gizmos.DrawLine(previousPosition, waypoint.position);
            previousPosition = waypoint.position;
        }
        Gizmos.DrawLine(previousPosition, startPosition);

        // �þ� �Ÿ� ���������� ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
