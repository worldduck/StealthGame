using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Guard : MonoBehaviour
{
    public float speed = 5; // 이동 속도
    public float waitTime = 2f; // 대기 시간
    public float turnSpeed = 180; // 회전 속도

    public Light spotlight;
    public float viewDistance;
    public LayerMask viewMask;
    private float seeAngle;
    private float attackAngle;

    public Transform pathHolder; // 이동 경로를 담아둘 변수
    private Transform player; // 플레이어의 Transform
    private Color originalSpotlightColor; // 스포트라이트의 기본색

    private Animator animator; // 가드의 애니메이터

    private void Start()
    {
        seeAngle = spotlight.spotAngle;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        originalSpotlightColor = spotlight.color;

        animator = GetComponent<Animator>();

        Vector3[] waypoints = new Vector3[pathHolder.childCount]; // 웨이포인트들의 배열
        for (int i = 0; i < waypoints.Length; i++)
        {
            // 웨이포인트들의 위치정보를 배열에 채우기
            waypoints[i] = pathHolder.GetChild(i).position;
            // 웨이포인트 y값 고정
            waypoints[i] = new Vector3(waypoints[i].x, transform.position.y, waypoints[i].z);
        }

        StartCoroutine(FollowPath(waypoints));
    }

    private void Update()
    {
        // CanSeePlayer() 메서드가 작동 한다면
        if (CanSeePlayer())
        {
            // 스포트라이트 컬러를 yellow로 변경
            spotlight.color = Color.yellow;
        }
        //else if (CanAttackPlayer())
        //{
        //    spotlight.color = Color.red;
        //}
        else
        {
            // 스포트라이트 컬러가 원래대로 변경
            spotlight.color = originalSpotlightColor;
        }
    }

    bool CanSeePlayer()
    {
        // 플레이어가 시야 거리 반경 안에 있다면
        if (Vector3.Distance(transform.position, player.position) < viewDistance)
        {
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            float angleBetweenGuardAndPlayer = Vector3.Angle(transform.forward, dirToPlayer);
            // 플레이어가 뷰 앵글 안에 있다면
            if (angleBetweenGuardAndPlayer < seeAngle / 2f)
            {
                // 플레이어가 장애물에 가려지지 않았다면
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
        // 첫번째 웨이포인트
        transform.position = waypoints [0];

        // 이동할 지점의 웨이포인트 인덱스
        int targetWaypointIndex = 1;
        // 실제 위치
        Vector3 targetwaypoint = waypoints[targetWaypointIndex];
        // 가드 회전의 초기값
        transform.LookAt(targetwaypoint);

        while (true)
        {
            // 현재위치(transform.position)를 앞으로(Vector3.MoveTowards)
            // 목표지점(targetwaypoint)까지 속도(speed * Time.deltaTime)로 이동
            transform.position = Vector3.MoveTowards(transform.position, targetwaypoint, speed * Time.deltaTime);
            // Move 애니메이션으로 변경
            animator.SetBool("isMove", true);
            // 도착하면 다음 웨이포인트로 이동
            if (transform.position == targetwaypoint)
            {
                // 인덱스가 다시 0으로 돌아오기 위해 나머지 연산자 사용
                targetWaypointIndex = (targetWaypointIndex + 1) % waypoints.Length;
                targetwaypoint = waypoints[targetWaypointIndex];

                // Move -> Idle 애니메이션으로 변경
                animator.SetBool("isMove", false);
                // 웨이포인트에 도착했을때 잠시 대기
                yield return new WaitForSeconds (waitTime);
                // 가드가 회전하는 동안 대기
                yield return StartCoroutine(TurnToFace(targetwaypoint));
            }
            // 다음 루틴까지 대기
            yield return null;
        }
    }

    IEnumerator TurnToFace(Vector3 lookTarget)
    {
        // 타겟을 바라보는 벡터 방향을 구함
        Vector3 dirToLookTarget = (lookTarget - transform.position).normalized;
        float targetAngle = 90 - Mathf.Atan2(dirToLookTarget.z, dirToLookTarget.x) * Mathf.Rad2Deg;

        // 가드의 시선이 목표를 향하는 동안
        while (Mathf.Abs(Mathf.DeltaAngle(transform.eulerAngles.y, targetAngle)) > 0.05f)
        {
            // 축 : transform.eulerAngles.y , 목표 : targetAngle, 속도 : turnSpeed * Time.deltaTime
            float angle = Mathf.MoveTowardsAngle(transform.eulerAngles.y, targetAngle, turnSpeed * Time.deltaTime);
            // 오일러 앵글
            transform.eulerAngles = Vector3.up * angle;
            // 다음 루틴까지 대기
            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 startPosition = pathHolder.GetChild(0).position;
        // 이전 위치에 시작위치를 넣어서 초기화
        Vector3 previousPosition = startPosition;

        foreach (Transform waypoint in pathHolder)
        {
            // 웨이포인트들을 선으로 이어 이동경로 표시
            Gizmos.DrawLine(previousPosition, waypoint.position);
            // 다음차례에는 현재 검색된 웨이포인트가 이전 포인트가 되도록 함
            previousPosition = waypoint.position;
        }
        // 마지막 웨이포인트에서 시작점을 이어주는 선
        Gizmos.DrawLine(previousPosition, startPosition);

        // 시야 거리 빨간선으로 표시
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
    }
}
