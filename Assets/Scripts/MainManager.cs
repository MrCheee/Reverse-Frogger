using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainManager : MonoBehaviour
{
    [SerializeField] private GameObject soldierPrefab;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine("SpawnTwoSoldiers");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator SpawnTwoSoldiers()
    {
        for (int i = 0; i < 2; i++)
        {
            GameObject unit = Instantiate(soldierPrefab, Vector3.zero, soldierPrefab.transform.rotation);
            unit.GetComponent<Soldier>().SetCurrentGridPosition(new GridCoord(0, 0));
            yield return new WaitForSeconds(5.0f);
        }
    }
}
