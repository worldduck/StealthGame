using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public enum State { Idle, Yellow, Red }
    public State state = State.Idle;
    public float speed = 5; // �̵� �ӵ�
    public float waitTime = 2f; // ��� �ð�
    public float turnSpeed = 180; // ȸ�� �ӵ�

    public Light spotlight1; // �þ߰Ÿ��� ǥ������ spotlight
    public Light spotlight2; // ���ݰŸ��� ǥ������ spotlight

    public float viewDistance; // �þ߰Ÿ�
    public float hitDistance; // ���ݰŸ�
    public LayerMask viewMask;
    private float seeAngle; // �þ߰���
    private float attackAngle; // ���ݰ���

    public Transform pathHolder; // �̵� ��θ� ��Ƶ� ����
    private Vector3[] waypoints;
    private Transform player; // �÷��̾��� Transform
    private Color originalSpotlightColor; // ����Ʈ����Ʈ�� �⺻��

    private Animator animator; // ������ �ִϸ�����

    private bool a = false;

    private void Start()
    {
        seeAngle = spotlight1.spotAngle;
        attackAngle = spotlight2.spotAngle;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpotlightColor = spotlight1.color;

        animator = GetComponent<Animator>();

        waypoints = new Vector3[pathHolder.childCount]; // ��������Ʈ���� �迭
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
            a = false;
            state = State.Yellow;
            // ����Ʈ����Ʈ1 �÷��� yellow�� ����
            spotlight1.color = Color.yellow;
            // �������ǥ ������Ʈ Ȱ��ȭ
            GameObject.Find("Goblin01").transform.Find("Exclamation_yellow").gameObject.SetActive(true);
            animator.SetBool("isMove", false);
        }
        else if (CanAttackPlayer())
        {
            spotlight1.color = originalSpotlightColor;
            spotlight2.color = Color.red;
            Debug.Log("11");
        }
        else
        {
            // ����Ʈ����Ʈ1,2 �÷��� ������� ����
            spotlight1.color = originalSpotlightColor;
            spotlight2.color = originalSpotlightColor;
            // ����ǥ ������Ʈ ��Ȱ��ȭ
            GameObject.Find("Goblin01").transform.Find("Exclamation_yellow").gameObject.SetActive(false);
            state = State.Idle;
            StartCoroutine(FollowPath(waypoints));
        }
    }

    bool CanSeePlayer()
    {
        // �÷��̾ �þ� �Ÿ� �ݰ� �ȿ� �ִٸ�
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            // �÷��̾ seeAngle �ȿ� �ִٸ�
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

    bool CanAttackPlayer()
    {
        // �÷��̾ ���� �Ÿ� �ݰ� �ȿ� �ִٸ�
        if (Vector3.Distance(transform.position, player.position) < hitDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            // �÷��̾ attackAngle �ȿ� �ִٸ�
            if (angleBetweenGuardAndPlayer < attackAngle / 2f)
            {
                // �÷��̾ ��ֹ��� �������� �ʾҴٸ�
                if (!Physics.Linecast(transform.position, player.position, viewMask))
                {
                    return true;
                }
            }
        }
        return false;
    }

    IEnumerator FollowPath(Vector3 [] waypoints)
    {
        if (a == true)
        {
            yield break;
        }
        a = true;

        // ù��° ��������Ʈ
        transform.position = waypoints [0];

        // �̵��� ������ ��������Ʈ �ε���
        int targetWaypointIndex = 1;
        // ���� ��ġ
        Vector3 targetwaypoint = waypoints[targetWaypointIndex];
        // ���� ȸ���� �ʱⰪ
        transform.LookAt(targetwaypoint);

        while (state == State.Idle)
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

        // �þ� �Ÿ� ��������� ǥ��
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);

        // ���� �Ÿ� ���������� ǥ��
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * hitDistance);
    }
}
