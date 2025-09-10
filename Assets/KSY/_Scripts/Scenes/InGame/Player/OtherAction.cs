using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static BackendFunctionInGame;

public class OtherAction : Player
{
    //network
    public bool IsItemHolding; // 아이템을 들었는가?
    public bool IsItemShoting;// 아이템을 던졌는가?
    private byte _chargeGauge = 0;
    private Vector2 _throwDir;

    [SerializeField] private Transform HoldTransform;
    [SerializeField] private GameObject HoldObject;
    private Rigidbody2D _rb;

    [SerializeField] private float DefaultShotForce = 60f;
    [SerializeField] private float UpwardForce = 30f;
    //던지기 반동
    [SerializeField] private float PlayerRecoil = 40f;
    [SerializeField] private GameObject ChargeUiObject;
    [SerializeField] private Image ChargeImage;



    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        if (HoldTransform == null) HoldTransform = transform.Find("Hold");
        if (ChargeUiObject == null) ChargeUiObject = transform.Find("ChageCanvas").gameObject;
        if (ChargeImage == null) ChargeImage = transform.Find("ChageCanvas/ChageBackground/ChageImage").GetComponent<Image>();
    }
    private void Hold(GameObject obj)
    {
        Debug.Log($"obj is {obj.name}");
        //아이템 들기 처리
        if (obj.TryGetComponent(out Item itemSc))
        {
            HoldObject = obj;
            itemSc.owner = gameObject;
            itemSc.Grab();
        }

        if (obj.TryGetComponent(out Rigidbody2D rb))
        {
            rb.simulated = false;
            rb.transform.parent = HoldTransform;
            rb.gravityScale = 2.75f;
            rb.GetComponent<Collider2D>().isTrigger = false;

            obj.transform.localPosition = Vector2.zero;
        }
    }
    private void ThrowItem(GameObject item, Vector2 dir, byte force)
    {
        Debug.Log($"GameObject : {item}, Vector2  : {dir}, byte : {force}");
        if (HoldObject == null) return;

        Item itemScript = item.GetComponent<Item>();
        if (_chargeGauge >= 3)
        {
            itemScript.preowner = transform;
            itemScript.shootingdir = Vector2.zero;
            itemScript.Eat();
            HoldObject = null;
            return;
        }

        if (dir == Vector2.zero) return;

        HoldObject.transform.parent = null;
        HoldObject.transform.position = transform.position + ((Vector3)dir * 1.25f);

        itemScript.isshooting = true;
        itemScript.preowner = transform;
        itemScript.CooldownActive();

        Rigidbody2D hrb = HoldObject.GetComponent<Rigidbody2D>();
        //아이템 물리연산 O
        hrb.simulated = true;
        //던지는 방향과 힘을 정함
        itemScript.shootingdir = dir * _chargeGauge;

        if (!itemScript.thisisnoforceobject)
        {
            hrb.linearVelocity = Vector2.zero;
            hrb.AddForce(dir * DefaultShotForce * force + (dir.y == 0 ? new Vector2(0, UpwardForce)
                : new Vector2(0, 0)), ForceMode2D.Impulse);
            hrb.angularVelocity += Random.Range(-180f, 180f);
        }

        //플레이어 던지는 반동 이펙트
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(-dir * PlayerRecoil, ForceMode2D.Impulse);

        itemScript.Launching();

        HoldObject = null;
    }

    //수정할코드
    public override void ApplyUShortData(ushort id)
    {
        HoldObject = Game.Instance.MapCompo.SpawnerCompo.FindItem(id);
    }
    public override void ApplyByteData(byte state, byte charge)
    {
        //Debug.Log("Success Apply Byte data");
        bool isHolding = (state & (byte)flagActionState.IsHolding) != 0;
        bool isThrowing = (state & (byte)flagActionState.IsThrowing) != 0;

        _chargeGauge = charge;

        if (isHolding)
        {
            Debug.Log($"isHolding : {isHolding}");
            Hold(HoldObject);
        }
        if (isThrowing)
        {
            Debug.Log($"isThrowing : {isThrowing}");
            ThrowItem(HoldObject, _throwDir, _chargeGauge);
        }
    }
    public override void ApplySbyteData(sbyte dirX, sbyte dirY)
    {
        _throwDir = new Vector2(dirX, dirY);
    }
}