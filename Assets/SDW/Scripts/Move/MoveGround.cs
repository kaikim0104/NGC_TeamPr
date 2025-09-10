using UnityEngine;
namespace SDW
{
    public class MoveGround : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float moveDistanceX = 10f;
        [SerializeField] private float moveDistanceY = 0f;

        private Vector3 startPosition;
        private Vector3 targetPosition;
        private bool movingToTarget = true;

        private void Awake()
        {
            // 초기 위치와 목표 위치 설정
            startPosition = transform.position;
            targetPosition = new Vector3(
                startPosition.x + moveDistanceX,
                startPosition.y + moveDistanceY,
                startPosition.z
            );
        }

        private void Update()
        {
            Move();
        }

        private void Move()
        {
            // 우선 목표 위치로 이동
            Vector3 destination = movingToTarget ? targetPosition : startPosition;
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

            // 목표 위치에 도달했는지 확인
            if (Vector3.Distance(transform.position, destination) < 0.01f)
            {
                transform.position = destination;
                movingToTarget = !movingToTarget;
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                // 플레이어를 자식으로 넣어서 팅기지 않도록 하기
                collision.transform.SetParent(transform, true);
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.collider.CompareTag("Player"))
            {
                // 플레이어를 자식에서 빼기
                collision.transform.SetParent(null);
            }
        }
    }
}