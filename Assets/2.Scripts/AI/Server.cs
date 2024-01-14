using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MonsterLove.StateMachine;
using System;
public class Server : MonoBehaviour
{
    public enum States //state enum
    {
        Idle,
        Walk,
        Serve
    }

    StateMachine<States, StateDriverUnity> fsm;
    public GameObject foodHolder;
    public float speed=2f;
    public bool IsAvailable { get; private set; } = true; // 서버의 사용 가능 여부
    private bool isThereMenuToServe=false;
    private GameObject menuToServe;
    private Transform placeToMove; //Server bring food place
    private Customer currentCustomer;
    public event Action OnAvailable;

    
    void Awake()
    {
        fsm = new StateMachine<States, StateDriverUnity>(this);
        fsm.ChangeState(States.Idle);
    }

    void Update()
    {
        fsm.Driver.Update.Invoke();
    }

    public void HandleNewServeTask(Transform child)
    {
        isThereMenuToServe=true;
        menuToServe = child.gameObject;
        // 이후 서버 상태를 업데이트 (예: IsAvailable = false)
    }

    void Idle_Enter()
    {
        //playanimation(idle)w
        //Debug.Log("Server Idle Enter");
    }
    void Idle_Update()
    {
        //Debug.Log("Server Idle Enter");
        if (isThereMenuToServe&&IsAvailable)
        {
            placeToMove=menuToServe.transform;
            fsm.ChangeState(States.Walk);
        }
    }
    void Idle_Exit()
    {
        IsAvailable=false;
        //Debug.Log("Server Idle Exit");
    }
    void Walk_Enter()
    {
        //playanimation(Walk)
        //Debug.Log("Server Walk Enter"); 
    }
    void Walk_Update()
    {
        if (Vector2.Distance(transform.position, placeToMove.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, placeToMove.position, speed * Time.deltaTime);
        }
        else{
            fsm.ChangeState(States.Serve);
        }
        //Debug.Log("Server Walk Update");
    }
    void Walk_Exit()
    {
        //Debug.Log("Server Walk Exit");
    }
    
    void Serve_Enter()
    {
        FoodPlace foodPlace = menuToServe.GetComponentInParent<FoodPlace>();
        if (foodPlace != null)
        {
            foodPlace.RemoveChild(menuToServe);
        }
        menuToServe.transform.parent=foodHolder.transform;
        int tableNum=menuToServe.GetComponent<FoodMain>().orderstatus.tableNumber;
        foreach(var chair in CustomerManager.Instance.customerTablePlace)
        {
            if(chair.transform.childCount>0&&chair.transform.GetChild(0).GetComponent<Customer>().tableNumber==tableNum){
                placeToMove=chair.transform.GetChild(0).transform;//guest place
                currentCustomer = placeToMove.gameObject.GetComponent<Customer>();
                break;
            }
        }
        //playanimation(Serve)
        //Debug.Log("Server Serve Enter");
    }
    void Serve_Update()
    {
        //Debug.Log(menuToServe);
        if (Vector2.Distance(transform.position, placeToMove.position) > 1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, placeToMove.position, speed * Time.deltaTime);
        }
        else
        {
            if (currentCustomer == null)
            {
                //Debug.LogError("Customer component not found");
                return; // 이후 코드 실행을 중단
            }
            currentCustomer.GetMenu(menuToServe);
            fsm.ChangeState(States.Idle);
        }
        //Debug.Log("Server Serve Update");
    }
    void Serve_Exit()
    {
        SetAvailable();
        //Debug.Log("Server Serve Exit");
    }
    // 서버 상태를 사용 가능으로 변경하는 메소드
    public void SetAvailable()
    {
        IsAvailable = true;
        isThereMenuToServe=false;
        OnAvailable?.Invoke();
    }
}