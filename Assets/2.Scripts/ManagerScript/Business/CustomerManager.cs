using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CustomerManager : MonoBehaviour,IBusinessManagerInterface
{
    private static CustomerManager instance;
    public List<GameObject> customerTable;
    public List<GameObject> customerChair{get;private set;}
    public GameObject customerBackPlace;
    public List<bool> customerChairPresent { get; private set; }
    public CustomerSpawner customerSpawner;
    [HideInInspector]
    public int currentEnabledTable=1;
    public bool isRewardActivated { get; private set; }

    public delegate void RewardTimeCheckDelegate(float timeLeft);
    public event RewardTimeCheckDelegate OnRewardTimeCheckDelegate;
    public bool isSpeedRewardActivated{get;private set;} = false;

    public static CustomerManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<CustomerManager>();
            }
            return instance;
        }
    }
    void OnEnable()
    {
        if (DataManager.Instance != null) DataManager.Instance.OnRewardActivatedDelegate += FeverTime;
    }
    void OnDisable()
    {
        if (DataManager.Instance != null) DataManager.Instance.OnRewardActivatedDelegate -= FeverTime;
    }
    private void FeverTime(bool isActivated)
    {
        isSpeedRewardActivated = isActivated;
    }
    public void AddOneTable(){
        foreach (var table in customerTable)
        {
            // check is server disableds
            if (!table.gameObject.activeSelf)
            {
                // if find server to enable, exit the function
                table.gameObject.SetActive(true);
                currentEnabledTable++;
                AddChairs(table);
                return;
            }
        }
    }
    
    public void AddChairs(GameObject table){
        for (int i = 0; i < table.transform.GetChild(0).childCount; i++)
        {
            GameObject chair = table.transform.GetChild(0).GetChild(i).gameObject;
            if (!customerChair.Contains(chair))
            {
                customerChair.Add(chair);
                customerChairPresent.Add(false);
            }
        }
    }
    // Determine if the guest chairs are fully occupied
    public bool IsCustomerFull() {
        return customerChairPresent.All(p => p);
    }
    public IEnumerator SetIsRewardActivated(float time)
    {
        isRewardActivated = true;
        float timeLeft = time;
        while (timeLeft > 0)
        {
            OnRewardTimeCheckDelegate?.Invoke(timeLeft);
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }
        OnRewardTimeCheckDelegate?.Invoke(0);
        isRewardActivated = false;
    }
    public void SetData(BusinessData data){
        currentEnabledTable=data.enabledTables;
        customerChair = new List<GameObject>();
        customerChairPresent = new List<bool>(new bool[customerChair.Count]);
        //enable chef amount of currentEnabledChef
        for (int i = 0; i < customerTable.Count; i++)
        {
            if (i < currentEnabledTable)
            {
                customerTable[i].gameObject.SetActive(true);
                AddChairs(customerTable[i]);
            }
            else
            {
                customerTable[i].gameObject.SetActive(false);
            }
        }
        customerSpawner = FindObjectOfType<CustomerSpawner>();
        StartCoroutine(customerSpawner.CreateCustomerEveryFewSeconds());
    }
    public void AddDataToBusinessData(BusinessData data)
    {
        data.enabledTables=currentEnabledTable;
    }
}
