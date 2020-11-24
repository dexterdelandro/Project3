﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
public enum MonsterState
{
    Patrolling,
    Chasing,
    Attacking,
    Stunned
}
public class ArriveAtPoint : MonoBehaviour
{
    public Camera cam;
    public NavMeshAgent agent;
    public GameObject light;
    private GameObject player;
    public Vector3 lastKnownPosition;
    public float maximumSightAngle = 70f;
    public LayerMask playerLayer;
    public float playerHiddenDistance = 20f;
    public Transform head;
    public MonsterState curState;
    private float timeSinceNoticed;
    public float maxTimeSinceNoticed = 10f;
    public float attackAngle = 30f;
    public float attackDist = .5f;
    public float maxStunTimer = 20f;
    private float stunTimer;
    public float chaseAwarenessDist = 3f;
    public float maxAttackTimer = .9f;
    private float attackTimer;
    public AudioClip[] footsteps;
    public AudioClip[] snarls;
    public AudioClip hit;
    public float footstepWalkTimer = 1f;
    public float footstepRunTimer = .5f;
    private float footstepTimer;
    public float maxSnarlTime = 15f;
    public float minSnarlTime = 7f;
    private float curSnarlMax;
    private float snarlTimer;
    void Start()
    {
        player = GameObject.Find("Player");
        curState = MonsterState.Patrolling;
        stunTimer = 0f;
        attackTimer = 0f;
        snarlTimer = 0f;
        curSnarlMax = Random.Range(minSnarlTime, maxSnarlTime);
        GetComponent<Animator>().SetInteger("battle", 0);
        GetComponent<Animator>().SetInteger("moving", 0);
    }

    public void NewPosition(Vector3 position)
    {
        if (curState != MonsterState.Stunned && curState != MonsterState.Attacking)
        {
            lastKnownPosition = position;
            curState = MonsterState.Chasing;
            timeSinceNoticed = 0f;
            GetComponent<Animator>().SetInteger("battle", 1);
            GetComponent<Animator>().SetInteger("moving", 2);
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (!Pause.paused)
        {
            Vector3 playerDir = player.transform.position - transform.position;
            float angle = Vector2.Angle(new Vector2(playerDir.x, playerDir.z), new Vector2(transform.forward.x, transform.forward.z));
            float playerDist = Vector3.Distance(player.transform.position, transform.position);
            switch (curState)
            {
                case MonsterState.Patrolling:
                    if(footstepTimer > footstepWalkTimer)
                    {
                        footstepTimer = 0f;
                        AudioSource.PlayClipAtPoint(footsteps[Random.Range(0, footsteps.Length)], transform.position);
                    }
                    footstepTimer += Time.deltaTime;
                    if (playerDist < playerHiddenDistance && angle < maximumSightAngle)
                    {
                        if (Physics.Raycast(head.position, player.transform.position - head.position, playerLayer))
                        {
                            NewPosition(player.transform.position);
                        }
                    }
                    break;
                case MonsterState.Chasing:
                    if (footstepTimer > footstepRunTimer)
                    {
                        footstepTimer = 0f;
                        AudioSource.PlayClipAtPoint(footsteps[Random.Range(0, footsteps.Length)], transform.position);
                    }
                    footstepTimer += Time.deltaTime;
                    if (playerDist < playerHiddenDistance && angle < maximumSightAngle || playerDist < chaseAwarenessDist)
                    {
                        if (Physics.Raycast(head.position, player.transform.position - head.position, playerLayer))
                        {
                            NewPosition(player.transform.position);
                        }
                    }
                    agent.SetDestination(lastKnownPosition);
                    timeSinceNoticed += Time.deltaTime;
                    if (timeSinceNoticed > maxTimeSinceNoticed)
                    {
                        curState = MonsterState.Patrolling;
                        GetComponent<Animator>().SetInteger("battle", 0);
                        GetComponent<Animator>().SetInteger("moving", 0);
                    }
                    if (angle < attackAngle && playerDist < attackDist)
                    {
                        transform.forward = new Vector3(playerDir.x, transform.forward.y, playerDir.z);
                        agent.ResetPath();
                        Debug.Log("Here");
                        GetComponent<Animator>().SetInteger("battle", 1);
                        GetComponent<Animator>().SetInteger("moving", 0);
                        curState = MonsterState.Attacking;
                        AudioSource.PlayClipAtPoint(hit, transform.position);
                        agent.enabled = false;
                    }
                    break;
                case MonsterState.Attacking:
                    attackTimer += Time.deltaTime;
                    if(GetComponent<Animator>().GetInteger("moving") == 0)
                    {
                        GetComponent<Animator>().SetInteger("moving", 6);
                    }
                    if (attackTimer > maxAttackTimer)
                    {
                        attackTimer = 0f;
                        curState = MonsterState.Patrolling;
                        GetComponent<Animator>().SetInteger("battle", 0);
                        GetComponent<Animator>().SetInteger("moving", 0);
                        agent.enabled = true;
                        if (Pause.paused)
                        {
                            Pause.HitPause();
                        }
                        SceneManager.LoadScene("Game Over Scene");
                    }
                    break;
                case MonsterState.Stunned:
                    stunTimer += Time.deltaTime;
                    if (stunTimer > maxStunTimer)
                    {
                        stunTimer = 0f;
                        curState = MonsterState.Chasing;
                        NewPosition(player.transform.position);
                        GetComponent<Animator>().SetInteger("battle", 0);
                        GetComponent<Animator>().SetInteger("moving", 0);
                        agent.enabled = true;
                    }
                    break;
                default:
                    break;
            }
            if(curState != MonsterState.Stunned)
            {
                snarlTimer += Time.deltaTime;
                if(snarlTimer > curSnarlMax)
                {
                    curSnarlMax = Random.Range(minSnarlTime, maxSnarlTime);
                    snarlTimer = 0f;
                    AudioSource.PlayClipAtPoint(snarls[Random.Range(0, snarls.Length)], transform.position);
                }
            }
        }
        //if (Input.GetKeyDown(KeyCode.Mouse1))
        //{
        //    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;
        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        agent.SetDestination(hit.point);
        //        Debug.Log("Here");
        //    }
        //}
        //if (Input.GetKeyDown(KeyCode.Q))
        //{
        //    Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;
        //    if (Physics.Raycast(ray, out hit))
        //    {
        //        Instantiate(light, new Vector3(hit.point.x, hit.point.y + .5f, hit.point.z), Quaternion.identity);
        //    }
        //}
    }
}
