using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameController : MonoBehaviour
{

    public GameObject[] prefabs;
    GameObject comingBlock;
    GameObject currentBlock;
    BlockController script;
    int index;
    Vector3 demoPoint = new Vector3(10, 15, 0); //poition to show the upcoming block

    //dimensions of the play area
    int maxRows = 26;
    int maxCols = 12;
    int[,] blockMap;

    List<GameObject> blockList = new List<GameObject>(); //all blocks in the scene
    List<GameObject> clearingList = new List<GameObject>(); //blocks to be moved after row clearence
    List<int> rowsToClear = new List<int>(); //full rows after a block is dropped

    Queue<GameObject> redundantList = new Queue<GameObject>(); //blocks with no children, to be removed from the scene
    Queue<Transform> children = new Queue<Transform>(); //children of a block that belong a full row

    [SerializeField] AudioClip rotateSound;
    AudioSource blockAudioSource;
    AudioSource mainAudioSource;
    AudioSource clearingAudioSource;

    // Use this for initialization
    void Start()
    {
        mainAudioSource = GameObject.Find("BackgroundMusic").GetComponent<AudioSource>();
        clearingAudioSource = GameObject.Find("ClearingAudio").GetComponent<AudioSource>();
        blockAudioSource = GetComponent<AudioSource>();
        blockMap = new int[maxRows, maxCols];

        //1st block to spawn is a square
        currentBlock = (GameObject)Instantiate(prefabs[0], transform.position, transform.rotation);
        script = currentBlock.GetComponent<BlockController>();
        blockList.Add(currentBlock);

        //show upcoming block
        index = Random.Range(0, 6);
        comingBlock = (GameObject)Instantiate(prefabs[index], demoPoint, Quaternion.identity);
        comingBlock.GetComponent<BlockController>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameOver()) //stop the game
        {
            mainAudioSource.Stop();
            return;
        }
        if (script.IsReady()) //when a block is dropped
        {
            script.UpdateMap(blockMap); //record its position
            //print("before");
            //PrintMap();
            ClearRows(); //clear row(s) if applicable
            //print("after");
            //PrintMap();

            //spawn next block with randomized index
            currentBlock = (GameObject)Instantiate(prefabs[index], transform.position, transform.rotation);
            script = currentBlock.GetComponent<BlockController>();
            blockList.Add(currentBlock);

            //replace upcoming block
            if (comingBlock != null)
                Destroy(comingBlock);
            index = Random.Range(0, 6);
            comingBlock = (GameObject)Instantiate(prefabs[index], demoPoint, Quaternion.identity);
            comingBlock.GetComponent<BlockController>().enabled = false;
        }

    }

    //if there is already a block at the middle of top row
    private bool GameOver()
    {
        var mid = maxCols % 2 == 1 ? (maxCols - 1) / 2 : maxCols / 2;
        return blockMap[maxRows - 1, mid] == 1;
    }

    //public methods to be accessed by block controller
    public int[,] GetMap()
    {
        return blockMap;
    }
    public int GetRowCount()
    {
        return maxRows;
    }
    public int GetColCount()
    {
        return maxCols;
    }
    public AudioSource GetAudioSource()
    {
        return blockAudioSource;
    }
    public void PlayRotateSound()
    {
        if (!blockAudioSource.isPlaying)
            blockAudioSource.PlayOneShot(rotateSound);
    }

    //print the map, used for debugging
    private void PrintMap()
    {
        for (int i = maxRows - 1; i >= 0; i--)
        {
            string row = "";
            for (int j = 0; j < maxCols; j++)
            {
                row += blockMap[i, j] + " ";
            }
            print(row);
        }
    }

    //round a float to the nearest 0.5
    private float Round(float num)
    {
        float res = (float)Math.Round(num * 2, MidpointRounding.AwayFromZero) / 2;
        //print(res);
        return res;
    }

    //perform row clearence
    private void ClearRows()
    {

        for (int i = 0; i < maxRows; i++)
        {
            bool rowFull = true;
            for (int j = 0; j < maxCols; j++)
            {
                if (blockMap[i, j] == 0)
                {
                    rowFull = false;
                    break;
                }
            }
            if (rowFull)
            {
                rowsToClear.Add(i);
            }
        }

        //if 1 or more rows are full
        if (rowsToClear.Count > 0)
        {
            clearingAudioSource.Stop();
            clearingAudioSource.Play();
            rowsToClear.Sort();
        }
        else
        {
            return;
        }

        RemoveRows();
        UpdateMapAfterClearing();

        //remove empty blocks
        while (redundantList.Count > 0)
        {
            GameObject block = redundantList.Dequeue();
            blockList.Remove(block);
            Destroy(block);
        }

        clearingList.Clear();
        rowsToClear.Clear();
    }

    private void RemoveRows()
    {
        DestroyChildBlocks();
        MoveRemnants();
    }

    private void UpdateMapAfterClearing()
    {

        Array.Clear(blockMap, 0, blockMap.Length);
        foreach (var block in blockList)
        {
            int childCount = block.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                var child = block.transform.GetChild(i);
                int column = (int)(Round(child.transform.position.x) + 5.5f);
                int row = (int)(Round(child.transform.position.y) - 0.5f);

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

    private void DestroyChildBlocks()
    {
        foreach (var block in blockList)
        {
            int childCount = block.transform.childCount;

            //check for empty blocks
            if (childCount == 0)
            {
                redundantList.Enqueue(block);
                continue;
            }

            //if a block is above the lowest full row, it should be moved too
            bool isAbove = true;

            for (int i = 0; i < childCount; i++)
            {
                var child = block.transform.GetChild(i);
                int y = (int)(Round(child.transform.position.y) - 0.5f);

                if (y <= rowsToClear[0])
                    isAbove = false;

                if (rowsToClear.Contains(y))
                {
                    if (!clearingList.Contains(block))
                        clearingList.Add(block);
                    children.Enqueue(child);
                }
            }

            //destroy child blocks on full rows
            while (children.Count > 0)
            {
                var childTransform = children.Dequeue();
                childTransform.parent = null;
                Destroy(childTransform.gameObject);
            }

            if (isAbove && !clearingList.Contains(block))
                clearingList.Add(block);
        }
    }

    //move down child blocks as necessary
    private void MoveRemnants()
    {
        foreach (var block in clearingList)
        {
            int childCount = block.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = block.transform.GetChild(i);
                int y = (int)(Round(child.transform.position.y) - 0.5f);

                int rowsToMove = 0;
                foreach (int r in rowsToClear)
                {
                    if (y > r)
                        rowsToMove++;

                }


                child.transform.position = child.transform.position + new Vector3(0, -1f * rowsToMove, 0);

            }
        }
    }


}
