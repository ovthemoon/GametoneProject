using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
public class StageMissionManager : MonoBehaviour,IManagerInterface
{
    
    public GameObject missionPanel;
    public TMP_Text WhenAllMissionCompleted;
    public int accumulatedCustomer{get;private set;}=0;
    public int accumulatedSales { get; private set; } = 0;
    public int currentCompletedMission { get; private set; } = 0;
    
    private List<ScriptableMission> currentStageMissions = new List<ScriptableMission>();
    public List<MissionBox> missions{get;private set;}= new List<MissionBox>();
    private MissionDataList missionDataList;
    private GameObject missionBoxPrefab;
    public int stageProgress{get;private set;}=0;
    private static StageMissionManager instance;
    public static StageMissionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<StageMissionManager>();
            }
            return instance;
        }
    }
    public void Awake()
    {
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    void Start(){
        
    }
    public void CalculateProgress(){
        stageProgress = 0;

        foreach (var mission in missions)
        {
            if (mission.isCleared)
            {
                stageProgress += mission.missionData.progress;
            }
        }
        Debug.Log(currentCompletedMission);
        if (stageProgress == 100)
        {
            Debug.Log("Stage Clear!!!!");
        }
        UIManager.Instance.UpdateProgress();
    }
    public void MissionInit(ScriptableMission currentStageMission)
    {
        Debug.Log("MIssion Box Created");
        GameObject missionBox = Instantiate(missionBoxPrefab);
        missionBox.transform.SetParent(missionPanel.transform, false);
        MissionBox mission = missionBox.GetComponent<MissionBox>();
        mission.missionData = currentStageMission;
        missions.Add(missionBox.GetComponent<MissionBox>());
        mission.InitMissionBox();
    }
    public void IncreaseAccumulatedCustomer(){
        accumulatedCustomer++;
        UpdateMissionStatus();
    }
    public void IncreaseAccumulatedSales(int money){
        accumulatedSales+=money;
        UpdateMissionStatus();
    }
    public void LevelCheck(){
        UpdateMissionStatus();
    }
    public void ActivatedCheck(){
        UpdateMissionStatus();
    }
    public void CostCheck(){
        UpdateMissionStatus();
    }
    private void UpdateMissionStatus()
    {
        foreach (var mission in missions)
        {
            if(mission.isCleared)continue;
            switch (mission.missionContent)
            {
                case MissionContent.CustomerCheck:
                    if (accumulatedCustomer >= mission.missionData.criteria)
                    {
                        mission.button.interactable=true;
                    }
                    break;
                case MissionContent.SalesCheck:
                    mission.button.interactable = accumulatedSales >= mission.missionData.criteria;
                    break;
                case MissionContent.LevelCheck:
                    if (mission.obj is Food food)
                    {
                        mission.button.interactable=food.currentLevel>= mission.missionData.criteria;
                    }
                    else if (mission.obj is Machine machine)
                    {
                        mission.button.interactable = machine.currentLevel >= mission.missionData.criteria;
                    }
                    break;
                case MissionContent.ActivatedCheck:
                    if (mission.obj is Machine machineActivatedCheck)
                    {
                        mission.button.interactable = machineActivatedCheck.gameObject.activeInHierarchy;
                    }
                    break;
                case MissionContent.MachineNumberCheck:
                    if (DataManager.Instance.activeMachines.Count >= mission.missionData.criteria)
                    {
                        mission.button.interactable = true;
                    }
                    break;
                default :
                    if (BusinessGameManager.Instance.money > mission.missionData.cost)
                    {
                        mission.button.interactable = true;
                    }
                    else mission.button.interactable = false;
                    break;

            }
            mission.SetUI();
            mission.SetActiveByStatus();
        }
    }
    public void SetData(BusinessData data)
    {
        missionDataList = Resources.Load<MissionDataList>("MissionDataList");
        missionBoxPrefab = Resources.Load<GameObject>("MissionBox");
        WhenAllMissionCompleted.gameObject.SetActive(false);
        accumulatedCustomer = data.accumulatedCustomer;
        accumulatedSales = data.accumulatedSales;
        Debug.Log("StageMission manager started");
        foreach (var currentStageMission in missionDataList.missionDataList)
        {
            if (currentStageMission.stageToAppear == BusinessGameManager.Instance.currentBusinessStage)
            {
                currentStageMissions.Add(currentStageMission);
                MissionInit(currentStageMission);
            }
        }
        
        foreach (var missionData in data.currentMissions)
        {
            var missionBox = missions.Find(m => m.missionData.index == missionData.missionIndex);
            if (missionBox != null)
            {
                missionBox.SetData(missionData.isUnlocked, missionData.isCleared);
                missionBox.CheckClearNUnlockStatus();
            }
        }
        UpdateMissionStatus();
        CalculateProgress();
    }
    public void AddDataToBusinessData(BusinessData data)
    {
        List <MissionData> tmpList=new List<MissionData>();
        foreach (var mission in missions){
            tmpList.Add(mission.GetData());
        }
        data.currentMissions=tmpList;
        data.accumulatedCustomer=accumulatedCustomer;
        data.accumulatedSales=accumulatedSales;
    }
}