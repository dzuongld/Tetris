using System;
using System.Linq;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    [SerializeField] float speedFactor = 0.1f;

    Rigidbody rb;
    Vector3 fallDirection = new Vector3(0, -1f, 0);
    bool isReady = false;
    GameController gameController;

    enum State
    {
        Free, HitLeft, HitRight, HitBottom
    }
    State state = State.Free;

    // Use this for initialization
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameController = GameObject.Find("SpawnPoint").GetComponent<GameController>();
        InvokeRepeating("Fall", 0, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (state == State.HitBottom)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (state != State.HitLeft && OkToMove("left"))
            {
                transform.position = transform.position + new Vector3(-1, 0, 0);
                if (state == State.HitRight)
                    state = State.Free;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if (state != State.HitRight && OkToMove("right"))
            {
                transform.position = transform.position + new Vector3(1, 0, 0);
                if (state == State.HitLeft)
                    state = State.Free;
            }
        }
        else if (Input.GetKeyDown(KeyCode.W) && OkToMove("rotate"))
        {
            transform.Rotate(Vector3.back, 90f);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            CancelInvoke("Fall");
            InvokeRepeating("Fall", 0, 0.8f);
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            CancelInvoke("Fall");
            InvokeRepeating("Fall", 0, 1f);
        }

        int[] positions = ProcessPosition();
        print(string.Join(" ", positions.Select(x => x.ToString()).ToArray()));

    }

    void OnCollisionEnter(Collision collision)
    {

        //foreach (ContactPoint contact in collision.contacts)
        //{
        //    print("Contact " + contact.point.x + " " + contact.point.y + " " + contact.point.z);
        //}

        //print(HitSide(collision));

        if (collision.gameObject.tag == "Bottom" || (collision.gameObject.tag == "BottomBlock" && !HitSide(collision)))
        {
            //print(this.tag);
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

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "BottomBlock" && !HitSide(collision))
        {
            HandleDrop();
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

    private void Fall()
    {
        if (state != State.HitBottom)
            transform.position = transform.position + fallDirection;
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
            for (int i = 0; i < 4; i++)
            {
                //pick one point
                ContactPoint p1 = collision.contacts[i];
                //print("Point1 " + p1.point.x + " " + p1.point.y + " " + p1.point.z);
                for (int j = 0; j < 4; j++)
                {
                    if (j != i)
                    {
                        ContactPoint p2 = collision.contacts[j];

                        //find the other point on the same x axis that shares y and z coords
                        if (Mathf.Abs(p1.point.y - p2.point.y) < 0.5 &&
                            Mathf.Abs(p1.point.z - p2.point.z) < 0.5)
                        {
                            //print("Point2 " + p2.point.x + " " + p2.point.y + " " + p2.point.z);
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
                        if (Mathf.Abs(p1.point.x - p2.point.x) < 0.5 &&
                            Mathf.Abs(p1.point.z - p2.point.z) < 0.5)
                        {
                            if (Mathf.Abs(p1.point.y - p2.point.y) > 0.5)
                            {
                                //print("Point2 V " + p1.point.x + " " + p1.point.y + " " + p1.point.z);
                                //print("Point2 V " + p2.point.x + " " + p2.point.y + " " + p2.point.z);
                                return true;
                            }
                        }
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

    private bool OkToMove(string side)
    {
        int[] positions = ProcessPosition();
        int leftSpaces = positions[0];
        int rightSpaces = positions[1];
        int distanceToTop = positions[2];
        int distanceToBottom = positions[3];

        switch (side)
        {
            case "left":
                return leftSpaces > 0;
            case "right":
                return rightSpaces > 0;
            case "rotate":
                if ((rightSpaces >= distanceToTop) && (leftSpaces >= distanceToBottom))
                {
                    if (state == State.HitRight && distanceToTop <= rightSpaces)
                        state = State.Free;

                    if (state == State.HitLeft && distanceToBottom <= leftSpaces)
                        state = State.Free;

                    return true;
                }

                if (distanceToTop > rightSpaces)
                {
                    int neededSpaces = distanceToTop - rightSpaces;
                    int availableSpaces = leftSpaces - distanceToBottom;

                    if (availableSpaces >= neededSpaces)
                    {
                        transform.position = transform.position + new Vector3(-1 * neededSpaces, 0, 0);
                        if (state == State.HitRight)
                            state = State.Free;

                        return true;
                    }

                }

                if (distanceToBottom > leftSpaces)
                {
                    int neededSpaces = distanceToBottom - leftSpaces;
                    int availableSpaces = rightSpaces - distanceToTop;

                    if (availableSpaces >= neededSpaces)
                    {
                        transform.position = transform.position + new Vector3(1 * neededSpaces, 0, 0);
                        if (state == State.HitLeft)
                            state = State.Free;

                        return true;
                    }

                }

                return false;
            default:
                return false;
        }
    }

    private int[] ProcessPosition()
    {
        var center = transform.GetChild(0);
        float centerY = center.position.y;

        float bottomY = float.MaxValue, leftX = float.MaxValue;
        float topY = float.MinValue, rightX = float.MinValue;
        float leftY = 0, rightY = 0;

        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            //print(i + " " + child.transform.position.x + " " + child.transform.position.y + " " +
            //child.transform.position.z);

            if (leftX > child.transform.position.x)
            {
                leftX = child.transform.position.x;
                leftY = child.transform.position.y;
            }
            if (rightX < child.transform.position.x)
            {
                rightX = child.transform.position.x;
                rightY = child.transform.position.y;
            }
            if (bottomY > child.transform.position.y)
            {
                bottomY = child.transform.position.y;
            }
            if (topY < child.transform.position.y)
            {
                topY = child.transform.position.y;
            }
        }

        int[,] map = gameController.GetMap();
        int cols = gameController.GetColCount();

        int x = (int)(Round(leftX) + 5.5f);
        int y = (int)(Round(leftY) - 0.5f);
        int index = x - 1;

        int leftSpaces = 0;
        while (index >= 0 && map[y, index] != 1)
        {
            leftSpaces++;
            index--;
        }

        x = (int)(Round(rightX) + 5.5f);
        y = (int)(Round(rightY) - 0.5f);
        index = x + 1;

        int rightSpaces = 0;
        while (index <= cols - 1 && map[y, index] != 1)
        {
            rightSpaces++;
            index++;
        }

        int distanceToTop = (int)Round(topY - centerY);
        int distanceToBottom = (int)Round(centerY - bottomY);

        return new int[] { leftSpaces, rightSpaces, distanceToTop, distanceToBottom };
    }

    public void UpdateMap(int[,] blockMap)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            int column = (int)(Round(child.transform.position.x) + 5.5f);
            int row = (int)(Round(child.transform.position.y) - 0.5f);

            blockMap[row, column] = 1;
        }
    }


}
