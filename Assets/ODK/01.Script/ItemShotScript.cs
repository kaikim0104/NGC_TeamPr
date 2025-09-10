using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class ItemShotScript : MonoBehaviour
{
    public bool isItemHold; // 아이템을 들었는가?
    public bool isItemShot;// 아이템을 던졋는가?

    [SerializeField] private Transform holdTransform;
    [SerializeField] private GameObject holdObject;
    private Rigidbody2D rb;

    [SerializeField] private float shootPower = 60f;
    [SerializeField] private float upwardForce = 30f;
    [SerializeField] private float playerRecoil = 40f;
    [SerializeField] private GameObject chargeUiObject;
    [SerializeField] private Image chargeImage;
    private float charge = 0f;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {


        if (Input.GetKey(KeyCode.E))
        {
            
            chargeUiObject.SetActive(true);
            if (charge > 3f)
            {
                charge = 3f;
                return;
            }
            if (charge >= 2.5f)
            {
                chargeImage.color = Color.red;
            }
            else
            {
                chargeImage.color = Color.white;
            }
            
            chargeImage.fillAmount = charge / 3f;
            charge += Time.deltaTime + ((2f - charge) * Time.deltaTime);
            


        }
        else if(charge > 0f)
        {
            Shoot();
            charge = 0f;
            chargeUiObject.gameObject.SetActive(false);
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
            Item holditem = collision.gameObject.GetComponent<Item>();
            if (holditem == null) return;


            if (holditem.owner == gameObject)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D itemCol = holditem.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(myCol, itemCol, true);
                return;
            }


            if (holditem.owner == null && !holditem.iscooldown && !holditem.isshooting)
            {
                if (holdObject != null)
                {
                    holdObject.GetComponent<Item>().owner = null;
                    Rigidbody2D oldRb = holdObject.GetComponent<Rigidbody2D>();
                    oldRb.simulated = true;
                    oldRb.gravityScale = 2.75f;
                    oldRb.transform.parent = null;
                    oldRb.GetComponent<Collider2D>().isTrigger = false;
                    holdObject.GetComponent<Item>().CooldownActive();
                }
                
                // �� ������ ���
                holdObject = holditem.gameObject;
                holditem.owner = gameObject;
                holditem.Grab();
                Rigidbody2D rbh = holditem.GetComponent<Rigidbody2D>();
                rbh.simulated = false;
                rbh.transform.parent = holdTransform;
                rbh.gravityScale = 2.75f;
                rbh.GetComponent<Collider2D>().isTrigger = false;
                holditem.transform.localPosition = Vector2.zero;
                isItemHold = true;
            }
        }

    }

    private void OnCollisionStay2D(Collision2D collision)
    {

        if (Keyboard.current.sKey.isPressed)
        {
            Item holditem = collision.gameObject.GetComponent<Item>();
            if (holditem == null) return;


            if (holditem.owner == gameObject)
            {
                Collider2D myCol = GetComponent<Collider2D>();
                Collider2D itemCol = holditem.GetComponent<Collider2D>();
                Physics2D.IgnoreCollision(myCol, itemCol, true);
                return;
            }


            if (holditem.owner == null && !holditem.iscooldown && !holditem.isshooting)
            {
                // ���� ��� �ִ� ������ ó��
                if (holdObject != null)
                {
                    holdObject.GetComponent<Item>().owner = null;
                    Rigidbody2D oldRb = holdObject.GetComponent<Rigidbody2D>();
                    oldRb.simulated = true;
                    oldRb.transform.parent = null;
                    oldRb.gravityScale = 2.75f;
                    oldRb.GetComponent<Collider2D>().isTrigger = false;
                    holdObject.GetComponent<Item>().CooldownActive();
                }

                // �� ������ ���
                holdObject = holditem.gameObject;
                holditem.owner = gameObject;
                
                Rigidbody2D rbh = holditem.GetComponent<Rigidbody2D>();
                rbh.simulated = false;
                rbh.transform.parent = holdTransform;
                rbh.gravityScale = 2.75f;
                rbh.GetComponent<Collider2D>().isTrigger = false;
                holditem.transform.localPosition = Vector2.zero;
                
            }
        }
        
    }

    private void Shoot()
    {
        isItemShot = true;
        if (holdObject == null) return;
        Vector2 dir = GetInputDirection();
        Item itemScript = holdObject.GetComponent<Item>();
        itemScript.isshooting = true;
        Rigidbody2D hrb = holdObject.GetComponent<Rigidbody2D>();
        if (charge >= 2.5)
        {
            itemScript.preowner = transform;
            itemScript.shootingdir = Vector2.zero;
            itemScript.Eat();
            holdObject = null;
            return;
        }
        else if (dir == Vector2.zero)
        {
            return;
        }
        
        holdObject.GetComponent<Item>().preowner = transform;
        holdObject.transform.parent = null;
        
        holdObject.transform.position = transform.position + (Vector3)(GetInputDirection() * 1.25f);

        
        itemScript.CooldownActive();
        hrb.simulated = true;
        

        itemScript.shootingdir = dir * charge;

        if (!itemScript.thisisnoforceobject)
        {
            hrb.linearVelocity = Vector2.zero;
            hrb.AddForce(dir * shootPower * charge + (dir.y == 0 ? new Vector2(0, upwardForce) : new Vector2(0, 0)), ForceMode2D.Impulse);
            hrb.angularVelocity += Random.Range(-180f, 180f);
        }
        
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(-dir * playerRecoil, ForceMode2D.Impulse);

        itemScript.Launching();
        holdObject = null;
        
    }
}