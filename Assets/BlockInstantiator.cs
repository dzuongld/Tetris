using UnityEngine;

public class BlockInstantiator : MonoBehaviour
{

    public GameObject[] prefabs;
    GameObject currentBlock;
    BlockController script;

    // Use this for initialization
    void Start()
    {
        //Instantiate(prefab, transform.position, transform.rotation);
        currentBlock = (GameObject)Instantiate(prefabs[0], transform.position, transform.rotation);
        script = currentBlock.GetComponent<BlockController>();

    }

    // Update is called once per frame
    void Update()
    {
        if (script.IsReady())
        {
            var index = Random.Range(0, 6);
            currentBlock = (GameObject)Instantiate(prefabs[index], transform.position, transform.rotation);
            script = currentBlock.GetComponent<BlockController>();
        }

    }
}
