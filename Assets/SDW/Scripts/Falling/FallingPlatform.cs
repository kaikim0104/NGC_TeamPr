using System;
using System.Collections;
using UnityEngine;

public class FallingPlatform : Platform
{
    [Header("레이어")]
    [SerializeField] private LayerMask PlayerLayerMask;

    [Header("지속시간 & 색 설정")]
    [SerializeField] private float durationTime = 2f;
    [SerializeField] private Color blinkColor = Color.red; // 깜빡일 색상

    private event Action OnStepped;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    //private int PlayerLayer;
    //플레이어와의 충돌을 감지할 레이어
    private int PlayerLayer = 10;

    //Duraition
    private float initialDuration;
    private bool _isBlinking = false;

    //network data
    public bool IsOnPlatform = false;

    private void Start()
    {
        //PlayerLayer = Mathf.RoundToInt(Mathf.Log(PlayerLayerMask.value, 2));
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        initialDuration = durationTime;

        //플랫폼 위에 플레이어가 올라갔다면 이벤트 등록
        OnStepped += () => 
        { 
            if (!_isBlinking) StartCoroutine(Blink()); 
        } ;
    }
    private void Update()
    {
        //만약 플레이어와 플랫폼 위에 올라와 있다면
        if (IsOnPlatform)
        {
            //할당된 시간 세기
            OnStepped?.Invoke();
            duration(); 
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        //서버가 아니라면 연산 금지
        if (!Server.IsSuperGamer) return;
        if(collision.gameObject.layer != PlayerLayer)
            return;

        IsOnPlatform = true;

        //플랫폼 위에 올라왔다면 이벤트 시작.
        OnStepped?.Invoke();
        Send();
    }

    private void duration()
    {
        //시간이 남았다면
        if (durationTime > 0)
        {
            //계속 시간 세기
            durationTime -= Time.deltaTime;
        }
        else
        {
            //아니라면 시간을 0초로 만든 후, 
            durationTime = 0f;
            //모든 코루틴 멈추기
            StopAllCoroutines();
            //플랫폼 떨어뜨리기
            StartCoroutine(Fall());
        }
    }

    //플랫폼 깜빡이는 동작
    private IEnumerator Blink()
    {
        //깜박거리고 있음을 표시
        _isBlinking = true;
        Color originalColor = sr.color;

        //할당된 시간이 남았다면
        while (durationTime > 0)
        {
            float interval = 1f; // 기본 1초 간격

            //남은 시간 비율에 따라 속도 변경
            if (durationTime <= initialDuration * 0.25f)
                interval = 0.25f;
            else if (durationTime <= initialDuration * 0.5f)
                interval = 0.5f;

            //깜빡임
            sr.color = blinkColor;
            yield return new WaitForSeconds(interval / 2f);
            sr.color = originalColor;
            yield return new WaitForSeconds(interval / 2f);
        }

        //할당된 시간이 끝났다면 깜빡임 멈추기 
        sr.color = originalColor;
        _isBlinking = false;
    }

    //플랫폼 떨어지는 동작
    private IEnumerator Fall()
    {
        //쓸데없는 물리연산을 하지않기 위해서 잠궈놓았던 constraints를 해제함.
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        //중력 적용
        rb.gravityScale = 1f;

        // 0.5초 후에 파괴
        yield return new WaitForSeconds(0.5f); 
        Destroy(gameObject);
    }

}
