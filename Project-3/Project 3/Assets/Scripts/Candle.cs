﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Candle : MonoBehaviour
{
    public float maxTime = 30f;
    private float curTime;
    private bool lit;
    public bool held;
    private GameObject player;
    public bool justPickedUp;
    private float startYLength;
    private float lastHeight;
    public Camera cam;
    private GameObject monster;
    public float stunDist = 1f;
    public float playerMaxUnstunNoticeDist = 3f; //Mouthful i know
    private Transform light;
    private Transform top;
    private Transform holder;
    public float heightAboveFloor = 0f;
    public LayerMask floor;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        lit = false;
        held = false;
        curTime = maxTime;
        justPickedUp = false;
        startYLength = transform.localScale.y;
        lastHeight = transform.localScale.y;
        monster = GameObject.Find("Monster");
        light = transform.parent.GetChild(1);
        top = transform.parent.GetChild(2);
        holder = transform.parent.GetChild(3);
    }

    // Update is called once per frame
    void Update()
    {
        if (!Pause.paused)
        {
            if (held)
            {
                transform.parent.position = cam.transform.position + cam.transform.forward * 8f;
                if (Input.GetKeyDown(KeyCode.E) && !justPickedUp)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(new Vector3(transform.parent.position.x, transform.position.y-5f, transform.position.z), Vector3.down, out hit, 50f, floor))
                    {
                        transform.parent.position = new Vector3(hit.point.x, hit.point.y + heightAboveFloor, hit.point.z);
                    }
                    held = false;
                    player.GetComponent<PickUpCandle>().holding = false;
                    player.GetComponent<PickUpCandle>().justDropped = true;
                }
                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (curTime > 0f)
                    {
                        lit = !lit;
                        if (lit)
                        {
                            light.gameObject.SetActive(true);
                        }
                        else
                        {
                            light.gameObject.SetActive(false);
                        }
                    }
                }
            }
            if (lit)
            {
                if (curTime > 0f)
                {
                    if (Vector3.Distance(monster.transform.position, transform.position) < stunDist)
                    {
                        monster.GetComponent<ArriveAtPoint>().curState = MonsterState.Stunned;
                        monster.GetComponent<NavMeshAgent>().enabled = false;
                        monster.GetComponent<Animator>().SetInteger("battle", 3);
                        monster.GetComponent<Animator>().SetInteger("moving", 0);
                        Debug.Log("Stunned");
                    }
                    else if (monster.GetComponent<ArriveAtPoint>().curState == MonsterState.Stunned)
                    {
                        UnStun();
                    }
                    curTime -= Time.deltaTime;
                    if (curTime <= 0f)
                    {
                        if (monster.GetComponent<ArriveAtPoint>().curState == MonsterState.Stunned)
                        {
                            UnStun();
                        }
                        curTime = 0f;
                        lit = false;
                        light.gameObject.SetActive(false);
                        Destroy(gameObject);
                    }
                    float height = startYLength * (curTime / maxTime);
                    transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);
                    transform.position = new Vector3(transform.position.x, transform.parent.GetChild(4).position.y + height, transform.position.z);
                    top.position = new Vector3(transform.position.x, transform.GetChild(0).position.y+(top.GetChild(0).GetComponent<BoxCollider>().size.y*top.GetChild(0).localScale.y/2), transform.position.z);
                    light.position = new Vector3(transform.position.x, top.position.y + .2f, transform.position.z);
                    lastHeight = height;
                }
            }
            else if (monster.GetComponent<ArriveAtPoint>().curState == MonsterState.Stunned)
            {
                UnStun();
            }
            justPickedUp = false;
        }
    }

    private void UnStun()
    {
        monster.GetComponent<ArriveAtPoint>().curState = MonsterState.Patrolling;
        monster.GetComponent<NavMeshAgent>().enabled = true;
        monster.GetComponent<ArriveAtPoint>().ChooseNewPosition();
        if (Vector3.Distance(player.transform.position, monster.transform.position) < playerMaxUnstunNoticeDist)
        {
            monster.GetComponent<ArriveAtPoint>().NewPosition(player.transform.position);
        }
        else
        {
            monster.GetComponent<Animator>().SetInteger("battle", 0);
            monster.GetComponent<Animator>().SetInteger("moving", 1);
        }
    }
}
