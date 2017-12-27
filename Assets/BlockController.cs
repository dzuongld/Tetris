using System;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] int speedFactor = 1;

    Rigidbody rb;
    Vector3 fallDirection = new Vector3(0, -0.01f, 0);
    bool isReady = false;

    enum State
    {
        Free, HitLeft, HitRight, HitBottom
    }
    State state = State.Free;

    //public Transform spawnPoint;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
        Fall();
    }

    void HandleInput()
    {
        if (state == State.HitBottom)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (state == State.HitRight)
                state = State.Free;
            if (state != State.HitLeft)
                transform.position = transform.position + new Vector3(-1, 0, 0);
            //CheckChildren();

        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (state == State.HitLeft)
                state = State.Free;
            if (state != State.HitRight)
                transform.position = transform.position + new Vector3(1, 0, 0);
            //CheckChildren();

        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            transform.Rotate(Vector3.back, 90f);
            //CheckChildren();
        }
        else if (Input.GetKey(KeyCode.S))
        {
            speedFactor = 10;
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            speedFactor = 1;
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        //print(ReturnDirection(collision.gameObject, this.gameObject) + this.gameObject.tag);
        print(HitSide(collision));

        foreach (ContactPoint contact in collision.contacts)
        {
            print("Contact " + contact.point.x + " " + contact.point.y + " " + contact.point.z);
        }

        if (collision.gameObject.tag == "Bottom" || (collision.gameObject.tag == "BottomBlock" && !HitSide(collision)))
        {
            HandleDrop();
            return;
        }
        if (collision.gameObject.tag == "Left")
        {
            state = State.HitLeft;
        }
        else if (collision.gameObject.tag == "Right")
        {
            state = State.HitRight;
        }

    }

    public bool IsReady()
    {
        if (isReady)
        {
            isReady = false;
            return true;
        }
        return false;
    }

    private void HandleDrop()
    {
        if (this.tag == "BottomBlock")
            return;

        state = State.HitBottom;
        this.tag = "BottomBlock";
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.isKinematic = true;
        FixPosition();
        isReady = true;

        ClearRows();
    }

    private void Fall()
    {
        if (state != State.HitBottom)
        {
            //rb.isKinematic = true;
            transform.position = transform.position + fallDirection * speedFactor;
            //rb.isKinematic = false;
        }
        //transform.GetChild(0).
    }

    //determine if current block has hit the side of a dropped block
    private bool HitSide(Collision collision)
    {
        //perfect collision, rarely happens
        if (collision.contacts.Length == 2)
            return true;

        //regular collision
        if (collision.contacts.Length == 4)
        {
            //pick one point
            ContactPoint p1 = collision.contacts[0];
            //print("Point " + p1.point.x + " " + p1.point.y + " " + p1.point.z);
            for (int i = 1; i < 4; i++)
            {
                ContactPoint p2 = collision.contacts[i];

                //find the other point on the same x axis that shares y and z coords
                if (Mathf.Abs(p1.point.y - p2.point.y) <= Mathf.Epsilon && Mathf.Abs(p1.point.z - p2.point.z) <= Mathf.Epsilon)
                {
                    //print("Point " + p2.point.x + " " + p2.point.y + " " + p2.point.z);
                    //ideally should work with < 1 
                    if (Mathf.Abs(p1.point.x - p2.point.x) < 0.5)
                    {
                        //print(p1.point.z - p2.point.z);
                        //print("Point1 H " + p1.point.x + " " + p1.point.y + " " + p1.point.z);
                        //print("Point2 H " + p2.point.x + " " + p2.point.y + " " + p2.point.z);
                        return true;
                    }
                }

                //vertical collision
                if (Mathf.Abs(p1.point.x - p2.point.x) <= Mathf.Epsilon && Mathf.Abs(p1.point.z - p2.point.z) <= Mathf.Epsilon)
                {
                    if (Mathf.Abs(p1.point.y - p2.point.y) > 0)
                    {
                        //print("Point2 V " + p1.point.x + " " + p1.point.y + " " + p1.point.z);
                        //print("Point2 V " + p2.point.x + " " + p2.point.y + " " + p2.point.z);
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private float Round(float num)
    {
        float res = (float)Math.Round(num * 2, MidpointRounding.AwayFromZero) / 2;
        //print(res);
        return res;
    }

    private void FixPosition()
    {
        float x = Round(transform.position.x);
        float y = Round(transform.position.y);
        transform.position = new Vector3(x, y, 0);
    }

    private void CheckChildren()
    {
        float minX = float.MaxValue, minY = float.MaxValue;
        float maxX = float.MinValue, maxY = float.MinValue;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            print(i + " " + child.transform.position.x + " " + child.transform.position.y + " " + child.transform.position.z);

            if (minX < child.transform.position.x)
                minX = child.transform.position.x;
            if (maxX > child.transform.position.x)
                maxX = child.transform.position.x;
            if (minY < child.transform.position.y)
                minY = child.transform.position.y;
            if (maxY < child.transform.position.y)
                maxY = child.transform.position.y;
        }
    }

    private void ClearRows()
    {
        print(transform.position.y);
        DestroyObject(transform.GetChild(0).gameObject);
        DestroyObject(transform.GetChild(3).gameObject);
        //transform.GetChild(1).gameObject.transform.position = transform.GetChild(1).gameObject.transform.position + new Vector3(0, -0.5f, 0);
        //transform.GetChild(2).gameObject.transform.position = transform.GetChild(2).gameObject.transform.position + new Vector3(0, -0.5f, 0);
        transform.position = transform.position + new Vector3(0, -1f, 0);
        print(transform.position.y);
    }
}
