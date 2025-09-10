using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    [SerializeField] private float _minCameraSize = 6.5f;
    [SerializeField] private float _maxCameraSize = 8f;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _multiple = 0.5f;

    [SerializeField] private Transform _player1;
    [SerializeField] private Transform _player2;

    [SerializeField] private Vector2 _borderSize = new Vector2(4f, 2f); // 카메라 보더 영역

    private Camera _camera;
    private Vector3 _targetPos;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _targetPos = transform.position; // 시작할 때 현재 위치를 기준으로
    }

    private void FixedUpdate()
    {
        // 플레이어 중간점
        Vector3 midPos = new Vector3(
            (_player1.position.x + _player2.position.x) / 2,
            (_player1.position.y + _player2.position.y) / 2,
            -10f);

        // 현재 카메라 중심과 플레이어 중간의 차이
        Vector3 offset = midPos - _targetPos;

        // X축 체크
        if (Mathf.Abs(offset.x) > _borderSize.x)
        {
            _targetPos.x = midPos.x - Mathf.Sign(offset.x) * _borderSize.x;
        }

        // Y축 체크
        if (Mathf.Abs(offset.y) > _borderSize.y)
        {
            _targetPos.y = midPos.y - Mathf.Sign(offset.y) * _borderSize.y;
        }

        transform.position = Vector3.Lerp(transform.position, _targetPos, _speed * Time.fixedDeltaTime);

        // 줌 (두 플레이어 사이 거리 기반)
        float targetSize = Vector2.Distance(_player1.position, _player2.position) * _multiple;
        _camera.orthographicSize = Mathf.Clamp(
            Mathf.Lerp(_camera.orthographicSize, targetSize, _speed * Time.fixedDeltaTime),
            _minCameraSize, _maxCameraSize);
    }
}
