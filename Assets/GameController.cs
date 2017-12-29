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
    Vector3 demoPoint = new Vector3(15, 15, 0);

    int maxRows = 26;
    int maxCols = 12;
    int[,] blockMap;
    List<GameObject> blockList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        blockMap = new int[maxRows, maxCols];
        currentBlock = (GameObject)Instantiate(prefabs[0], transform.position, transform.rotation);
        script = currentBlock.GetComponent<BlockController>();

        index = Random.Range(0, 6);
        comingBlock = (GameObject)Instantiate(prefabs[index], demoPoint, Quaternion.identity);
        comingBlock.GetComponent<BlockController>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameOver())
        {
            return;
        }
        if (script.IsReady())
        {
            script.UpdateMap(blockMap);
            PrintMap();
            ClearRows();

            currentBlock = (GameObject)Instantiate(prefabs[index], transform.position, transform.rotation);
            script = currentBlock.GetComponent<BlockController>();
            blockList.Add(currentBlock);

            if (comingBlock != null)
                Destroy(comingBlock);
            index = Random.Range(0, 6);
            comingBlock = (GameObject)Instantiate(prefabs[index], demoPoint, Quaternion.identity);
            comingBlock.GetComponent<BlockController>().enabled = false;
        }

    }

    private bool GameOver()
    {
        var mid = maxCols % 2 == 1 ? (maxCols - 1) / 2 : maxCols / 2;
        return blockMap[maxRows - 1, mid] == 1;
    }

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

    private void ClearRows()
    {
        //print(transform.position.y);
        //DestroyObject(transform.GetChild(0).gameObject);
        //DestroyObject(transform.GetChild(3).gameObject);
        //transform.GetChild(1).gameObject.transform.position = transform.GetChild(1).gameObject.transform.position + new Vector3(0, -0.5f, 0);
        //transform.GetChild(2).gameObject.transform.position = transform.GetChild(2).gameObject.transform.position + new Vector3(0, -0.5f, 0);
        //transform.position = transform.position + new Vector3(0, -1f, 0);
        //print(transform.position.y);

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

            }
        }
    }

}
