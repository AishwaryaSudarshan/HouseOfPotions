using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StoreAction : MonoBehaviour 
{
    public List<GameObject> objectsToStore = new List<GameObject>();
    public GameObject objectToStore;
    public int inventoryCapacity = 3;

    private Coroutine inventoryFullCoroutine;

    public GameObject inventoryPanel;

    private string bButton;
    void Start()
    {
        #if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
            bButton = "js10";
        #elif UNITY_ANDROID
            bButton = "js5";
        #else
            bButton = "js10"; 
        #endif
    }
    void Update() 
    {
        if (Input.GetButtonDown(bButton)) 
        {
            Store();
        }
    }

    public void Store() 
    {
        if (objectToStore == null) 
        {
            return;
        }

        CloseObjectMenu(objectToStore);

        if (objectsToStore.Count >= inventoryCapacity) 
        {
            Debug.Log("Inventory is Full!");
            objectToStore = null;

            if (inventoryFullCoroutine != null)
                StopCoroutine(inventoryFullCoroutine);

            if (inventoryPanel == null || !inventoryPanel.activeSelf)
            {
                inventoryFullCoroutine = StartCoroutine(DisplayInventoryFullMessage());
            }
            return; 
        }

        objectsToStore.Add(objectToStore);
        Disable(objectToStore);
        objectToStore = null;
    }

    public void Disable(GameObject obj) 
    {
        if (obj == null) 
        {
            return;
        }
        obj.SetActive(false);
    }

    private void CloseObjectMenu(GameObject obj) 
    {
        if (obj != null && obj.transform.childCount > 0) 
        {
            GameObject menu = obj.transform.GetChild(0).gameObject;
            if (menu.activeSelf) 
            {
                menu.SetActive(false);
            }
        }
    }
    private IEnumerator DisplayInventoryFullMessage() 
    {
        GameObject inventoryFullMessage = transform.GetChild(0).gameObject;

        inventoryFullMessage.SetActive(true);
        yield return new WaitForSeconds(2f);
        inventoryFullMessage.SetActive(false);
        inventoryFullCoroutine = null;
    }
}
