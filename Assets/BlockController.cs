using System;
using UnityEngine;

public class BlockController : MonoBehaviour
{
    //[SerializeField] float speedFactor = 0.1f;

    Rigidbody rb;
    Vector3 fallDirection = new Vector3(0, -1f, 0); //block should fall down vertically 1 unit at a time
    bool isReady = false;
    GameController gameController;
    AudioSource audioSource;

    //possible states of a block
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
        audioSource = gameController.GetAudioSource();

        InvokeRepeating("Fall", 0, 1f);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        HandleInput();
    }

    void HandleInput()
    {
        if (state == State.HitBottom)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.A)) //move left
        {
            if (state != State.HitLeft && OkToMove("left"))
            {
                transform.position = transform.position + new Vector3(-1, 0, 0);
                if (state == State.HitRight)
                    state = State.Free;
            }
        }
        else if (Input.GetKeyDown(KeyCode.D)) //move right
        {
            if (state != State.HitRight && OkToMove("right"))
            {
                transform.position = transform.position + new Vector3(1, 0, 0);
                if (state == State.HitLeft)
                    state = State.Free;
            }
        }
        else if (this.tag != "Square" && Input.GetKeyDown(KeyCode.W) && OkToMove("rotate")) //rotate clockwise, except the squared block
        {
            if (!audioSource.isPlaying)
                gameController.PlayRotateSound();

            transform.Rotate(Vector3.back, 90f);
        }
        else if (Input.GetKey(KeyCode.S)) //speed up
        {
            CancelInvoke("Fall");
            //InvokeRepeating("Fall", 0, 0.5f);
            Fall();
        }
        else if (Input.GetKeyUp(KeyCode.S))
        {
            //CancelInvoke("Fall");
            InvokeRepeating("Fall", 0, 1f);
        }
        else
        {
            audioSource.Stop();
        }


    }

    void OnCollisionEnter(Collision collision)
    {

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

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.tag == "BottomBlock" && !HitSide(collision))
        {
            HandleDrop();
        }
    }

    //dropped and ready for the next block to spawn
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

    //when block has hit the base or other dropped blocks
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

                float y1 = p1.thisCollider.transform.gameObject.transform.position.y;
                float y2 = p1.otherCollider.transform.gameObject.transform.position.y;

                //hit underneath of a dropped block
                if (y2 - y1 > 0.5)
                    return true;


                for (int j = 0; j < 4; j++)
                {
                    if (j != i)
                    {
                        ContactPoint p2 = collision.contacts[j];

                        //find the other point on the same x axis that shares y and z coords
                        if (Mathf.Abs(p1.point.y - p2.point.y) < 0.5 &&
                            Mathf.Abs(p1.point.z - p2.point.z) < 0.5)
                        {
                            //ideally should work with < 1 
                            if (Mathf.Abs(p1.point.x - p2.point.x) < 0.5)
                                return true;

                        }

                        //vertical collision
                        if (Mathf.Abs(p1.point.x - p2.point.x) < 0.5 &&
                            Mathf.Abs(p1.point.z - p2.point.z) < 0.5)
                        {
                            if (Mathf.Abs(p1.point.y - p2.point.y) > 0.5)
                                return true;

                        }
                    }
                }
            }
        }

        return false;
    }

    //round a float to the nearest 0.5
    private float Round(float num)
    {
        float res = (float)Math.Round(num * 2, MidpointRounding.AwayFromZero) / 2;
        //print(res);
        return res;
    }

    //adjust coordinates due to imprecision after dropping
    private void FixPosition()
    {
        float x = Round(transform.position.x);
        float y = Round(transform.position.y);
        transform.position = new Vector3(x, y, 0);
    }

    //check if ok to perform specific movements
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
                //enough space, do regular rotation
                if ((rightSpaces >= distanceToTop) && (leftSpaces >= distanceToBottom))
                {
                    if (state == State.HitRight && distanceToTop <= rightSpaces)
                        state = State.Free;

                    if (state == State.HitLeft && distanceToBottom <= leftSpaces)
                        state = State.Free;

                    return true;
                }

                //not enough space on right, check if ok to move left before rotation
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

                //not enough space on left, check if ok to move rigth before rotation
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

    //process positions of child blocks
    private int[] ProcessPosition()
    {
        var center = transform.GetChild(0);
        float centerY = center.position.y;

        float bottomY = float.MaxValue;
        float topY = float.MinValue;

        int[,] children = new int[transform.childCount, 2];
        var childCount = transform.childCount;

        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(i);
            float x = child.transform.position.x;
            float y = child.transform.position.y;
            int xIndex = (int)(Round(x) + 5.5f);
            int yIndex = (int)(Round(y) - 0.5f);
            children[i, 0] = xIndex;
            children[i, 1] = yIndex;

            if (bottomY > y)
            {
                bottomY = y;
            }
            if (topY < y)
            {
                topY = y;
            }
        }


        int[,] map = gameController.GetMap();
        int cols = gameController.GetColCount();

        int leftSpaces = Int32.MaxValue; //available spaces on the left of the block
        int rightSpaces = Int32.MaxValue; //available spaces on the right of the block

        for (int i = 0; i < childCount; i++)
        {
            int x = children[i, 0];
            int y = children[i, 1];

            int leftCount = 0;
            int rightCount = 0;
            for (int j = x + 1; j < cols; j++)
            {
                if (map[y, j] == 1)
                    break;

                rightCount++;
            }
            if (rightSpaces > rightCount)
                rightSpaces = rightCount;

            for (int j = x - 1; j >= 0; j--)
            {
                if (map[y, j] == 1)
                    break;

                leftCount++;
            }
            if (leftSpaces > leftCount)
                leftSpaces = leftCount;
        }

        int distanceToTop = (int)(Round(topY) - Round(centerY)); //between center block and highest block
        int distanceToBottom = (int)(Round(centerY) - Round(bottomY)); //between center block and lowest block

        return new int[] { leftSpaces, rightSpaces, distanceToTop, distanceToBottom };
    }

    //record positions of child blocks on the map
    public void UpdateMap(int[,] blockMap)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i);
            int column = (int)(Round(child.transform.position.x) + 5.5f);
            int row = (int)(Round(child.transform.position.y) - 0.5f);

            //prevent the program from crashing
            try
            {
                blockMap[row, column] = 1;
            }
            catch (IndexOutOfRangeException e)
            {
                print(e.Message);
            }
        }
    }


}
