using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MyAction : Player
{
    //network
    public bool IsHolding; // 아이템을 들었는가?
    public bool IsThrowing;// 아이템을 던졌는가?
    private Vector2 _throwDir;

    [SerializeField] private Transform HoldTransform;
    [SerializeField] private GameObject HoldObject;
    private Rigidbody2D rb;

    [SerializeField] private float ThrowPower = 60f;
    [SerializeField] private float UpwardForce = 30f;
    [SerializeField] private float PlayerRecoil = 40f;
    [SerializeField] private GameObject ChargeUiObject;
    [SerializeField] private Image ChargeImage;

    private float _chargeGauge = 0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (HoldTransform == null) HoldTransform = transform.Find("Hold");
        if (ChargeUiObject == null) ChargeUiObject = transform.Find("ChageCanvas").gameObject;
        if (ChargeImage == null) ChargeImage = transform.Find("ChageCanvas/ChageBackground/ChageImage").GetComponent<Image>();
    }
    void Update()
    {
        //e키를 눌렀다면 (차징중이라면)
        if (Input.GetKey(KeyCode.E))
        {
            ChargeUiObject.SetActive(true);
            //차징이 3 이상이라면 더 이상 오르지 않게 처리
            if (_chargeGauge > 3f)
            {
                _chargeGauge = 3f;
                return;
            }
            //차징이 2.5 이상이라면 차징바를 빨간색으로 변경
            if (_chargeGauge >= 2.5f)
            {
                ChargeImage.color = Color.red;
            }
            //차징이 2.5 이상이 아니라면 차징바를 하얀색으로 변경
            else
            {
                ChargeImage.color = Color.white;
            }

            //차징 이미지 변경
            ChargeImage.fillAmount = _chargeGauge / 3f;

            //차징 게이지가 갈 수록 천천히 오르기
            _chargeGauge += Time.deltaTime + ((2f - _chargeGauge) * Time.deltaTime);
        }
        //e키를 떼어냈을 경우 (아무것도 누르고 있지 않은데 차징 게이지가 있을 경우)
        else if (_chargeGauge > 0f)
        {
            //아이템 발사
            ThrowItem();
            //차징 게이지 초기화
            _chargeGauge = 0f;
            //charge ui's active = false;
            ChargeUiObject.gameObject.SetActive(false);
        }
    }

    private Vector2 GetInputDirection()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.LeftArrow)/* || Input.GetKey(KeyCode.A)*/) dir.x -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)/* || Input.GetKey(KeyCode.D)*/) dir.x += 1f;
        if (Input.GetKey(KeyCode.UpArrow)/* || Input.GetKey(KeyCode.W)*/) dir.y += 1f;
        if (Input.GetKey(KeyCode.DownArrow)/* || Input.GetKey(KeyCode.S)*/) dir.y -= 1f;

        if (dir != Vector2.zero) dir.Normalize();
        return dir;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (Keyboard.current.sKey.isPressed)
        {
            Item item = collision.gameObject.GetComponent<Item>();
            if (item == null) return;

            if (item.owner == gameObject)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D itemCol = item.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(myCol, itemCol, true);
                return;
            }

            if (item.owner == null && !item.iscooldown && !item.isshooting)
            {
                if (HoldObject != null)
                {
                    HoldObject.GetComponent<Item>().owner = null;
                    Rigidbody2D oldRb = HoldObject.GetComponent<Rigidbody2D>();
                    oldRb.simulated = true;
                    oldRb.gravityScale = 2.75f;
                    oldRb.transform.parent = null;
                    oldRb.GetComponent<Collider2D>().isTrigger = false;
                    HoldObject.GetComponent<Item>().CooldownActive();
                }

                Hold(item.gameObject);
                IsHolding = true;
                Send();
            }
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (Keyboard.current.sKey.isPressed)
        {
            Item item = collision.gameObject.GetComponent<Item>();
            if (item == null) return;

            if (item.owner == gameObject)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D itemCol = item.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(myCol, itemCol, true);
                return;
            }
            else if (item.owner == null && !item.iscooldown && !item.isshooting)
            {
                if (HoldObject != null)
                {
                    HoldObject.GetComponent<Item>().owner = null;
                    Rigidbody2D oldRb = HoldObject.GetComponent<Rigidbody2D>();
                    oldRb.simulated = true;
                    oldRb.transform.parent = null;
                    oldRb.gravityScale = 2.75f;
                    oldRb.GetComponent<Collider2D>().isTrigger = false;
                    HoldObject.GetComponent<Item>().CooldownActive();
                }

                Hold(item.gameObject);
                IsHolding = true;
                Send();
            }
        }
    }

    private void ThrowItem()
    { 
        if (HoldObject == null) return;
        _throwDir = GetInputDirection();
        Item itemScript = HoldObject.GetComponent<Item>();
        itemScript.isshooting = true;
        Rigidbody2D hrb = HoldObject.GetComponent<Rigidbody2D>();
        if (_chargeGauge >= 3)
        {
            itemScript.preowner = transform;
            itemScript.shootingdir = Vector2.zero;
            itemScript.Eat();
            HoldObject = null;
            return;
        }
        else if (_throwDir == Vector2.zero)
        {
            return;
        }

        IsHolding = false;
        IsThrowing = true;
        Send();

        itemScript.preowner = transform;
        HoldObject.transform.parent = null;

        HoldObject.transform.position = transform.position + (Vector3)(GetInputDirection() * 1.25f);

        itemScript.CooldownActive();
        hrb.simulated = true;

        itemScript.shootingdir = _throwDir * _chargeGauge;

        if (!itemScript.thisisnoforceobject)
        {
            hrb.linearVelocity = Vector2.zero;
            hrb.AddForce(_throwDir * ThrowPower * _chargeGauge + (_throwDir.y == 0 ? new Vector2(0, UpwardForce) : new Vector2(0, 0)), ForceMode2D.Impulse);
            hrb.angularVelocity += Random.Range(-180f, 180f);
        }

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(-_throwDir * PlayerRecoil, ForceMode2D.Impulse);

        itemScript.Launching();
        HoldObject = null;

        IsThrowing = false;
    }
    private void Hold(GameObject obj)
    {
        //아이템 들기 처리
        if (obj.TryGetComponent(out Item itemSc))
        {
            HoldObject = obj;
            itemSc.owner = gameObject;
            itemSc.Grab();
        }

        //위치 조정
        if (obj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.simulated = false;
            rb.transform.parent = HoldTransform;
            rb.gravityScale = 2.75f;
            rb.GetComponent<Collider2D>().isTrigger = false;

            obj.transform.localPosition = Vector2.zero;
        }
    }
    public override void Send()
    {
        ushort id = HoldObject.GetComponent<Item>().Id;
        byte chargeGauge = (byte)_chargeGauge;
        byte[] bff = Server.Instance.SerializationActionData(id, IsHolding,IsThrowing, chargeGauge, _throwDir);
        Server.Instance.Send(bff);
    }
}