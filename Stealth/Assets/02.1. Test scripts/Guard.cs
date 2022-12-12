using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public float speed = 5; // �̵� �ӵ�
    public float waitTime = 2f; // ��� �ð�
    public float turnSpeed = 180; // ȸ�� �ӵ�

    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    private float seeAngle;
    private float attackAngle;

    public Transform pathHolder; // �̵� ��θ� ��Ƶ� ����
    private Transform player; // �÷��̾��� Transform
    private Color originalSpotlightColor; // ����Ʈ����Ʈ�� �⺻��

    private Animator animator; // ������ �ִϸ�����

    private void Start()
    {
        seeAngle = spotlight.spotAngle;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpotlightColor = spotlight.color;

        animator = GetComponent<Animator>();

        Vector3[] waypoints = new Vector3[pathHolder.childCount]; // ��������Ʈ���� �迭
        for (int i = 0; i < waypoints.Length; i++)
        {
            // ��������Ʈ���� ��ġ������ �迭�� ä���
            waypoints[i] = pathHolder.GetChild(i).position;
            // ��������Ʈ y�� ����
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    private void Update()
    {
        // CanSeePlayer() �޼��尡 �۵� �Ѵٸ�
        if (CanSeePlayer())
        {
            // ����Ʈ����Ʈ �÷��� yellow�� ����
            spotlight.color = Color.yellow;
        }
        //else if (CanAttackPlayer())
        //{
        //    spotlight.color = Color.red;
        //}
        else
        {
            // ����Ʈ����Ʈ �÷��� ������� ����
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
            if (angleBetweenGuardAndPlayer < seeAngle / 2f)
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

    //bool CanAttackPlayer()
    //{

    //}

    IEnumerator FollowPath(Vector3 [] waypoints)
    {
        // ù��° ��������Ʈ
        transform.position = waypoints [0];

        // �̵��� ������ ��������Ʈ �ε���
        int targetWaypointIndex = 1;
        // ���� ��ġ
        Vector3 targetwaypoint = waypoints[targetWaypointIndex];
        // ���� ȸ���� �ʱⰪ
        transform.LookAt(targetwaypoint);

        while (true)
        {
            // ������ġ(transform.position)�� ������(Vector3.MoveTowards)
            // ��ǥ����(targetwaypoint)���� �ӵ�(speed * Time.deltaTime)�� �̵�
            transform.position = Vector3.MoveTowards(transform.position, targetwaypoint, speed * Time.deltaTime);
            // Move �ִϸ��̼����� ����
            animator.SetBool("isMove", true);
            // �����ϸ� ���� ��������Ʈ�� �̵�
            if (transform.position == targetwaypoint)
            {
                // �ε����� �ٽ� 0���� ���ƿ��� ���� ������ ������ ���
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetwaypoint = waypoints[targetWaypointIndex];

                // Move -> Idle �ִϸ��̼����� ����
                animator.SetBool("isMove", false);
                // ��������Ʈ�� ���������� ��� ���
                yield return new WaitForSeconds (waitTime);
                // ���尡 ȸ���ϴ� ���� ���
                yield return StartCoroutine(TurnToFace(targetwaypoint));
            }
            // ���� ��ƾ���� ���
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        // Ÿ���� �ٶ󺸴� ���� ������ ����
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        // ������ �ü��� ��ǥ�� ���ϴ� ����
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            // �� : transform.eulerAngles.y , ��ǥ : targetAngle, �ӵ� : turnSpeed * Time.deltaTime
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            // ���Ϸ� �ޱ�
            transform.eulerAngles = Vector3.up * angle;
            // ���� ��ƾ���� ���
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        // ���� ��ġ�� ������ġ�� �־ �ʱ�ȭ
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            // ��������Ʈ���� ������ �̾� �̵���� ǥ��
            Gizmos.DrawLine(previousPosition, waypoint.position);
            // �������ʿ��� ���� �˻��� ��������Ʈ�� ���� ����Ʈ�� �ǵ��� ��
            previousPosition = waypoint.position;
        }
        // ������ ��������Ʈ���� �������� �̾��ִ� ��
        Gizmos.DrawLine(previousPosition, startPosition);

        // �þ� �Ÿ� ���������� ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
