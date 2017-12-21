using UnityEngine;

public class BlockController : MonoBehaviour
{

    Rigidbody rb;

    enum State
    {
        Free, HitLeft, HitRight, HitBottom
    }
    State state = State.Free;

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
            this.tag = "Bottom";
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (state == State.HitRight)
                state = State.Free;
            if (state != State.HitLeft)
                transform.position = transform.position + new Vector3(-1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (state == State.HitLeft)
                state = State.Free;
            if (state != State.HitRight)
                transform.position = transform.position + new Vector3(1, 0, 0);
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            transform.Rotate(Vector3.back, 90f);
        }
    }

    private void Fall()
    {
        if (state != State.HitBottom)
        {
            print("falling");
            transform.position = transform.position + new Vector3(0, -0.05f, 0);
        }

    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Bottom")
        {
            print("bottom");
            state = State.HitBottom;
            return;
        }
        if (collision.gameObject.tag == "Left")
        {
            print("left");
            state = State.HitLeft;
        }
        if (collision.gameObject.tag == "Right")
        {
            state = State.HitRight;
        }

    }
}
