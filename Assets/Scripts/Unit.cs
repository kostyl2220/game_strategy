﻿using Assets.Scripts;
using Assets.Scripts.FSM;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : Item {
    public Projector projector;
    public Animator animator;
    public Animation Animation;
    public String IdleAnim = "idle";
    public String RunAnim = "Run";
    public String AttackAnim;
    public String DeadAnim;

    private float rotation_speed = 100f;
    private float move_speed = 2f;
    private float attack;
    private float reload_time;

    private bool Selected = false;

    private Vector3 destination;
    private CharacterController _charController;
    private Grid grid;

    private Item target;

    private bool Hit;
    private float LastReload = -1000f;

    public float DamageTimeKoef = 0.8f;
    private List<Node> movePoints;

    private Coroutine attackCoroutine;

    private UnitInfo unit_info;

    private Quaternion EndDirection;
    private State PerformState;
    // Use this for initialization
    void Start () {
        Hit = false;
        destination = transform.position;
        PerformState = new IdleState(this);
    }

    public void SetState(State State)
    {
        PerformState = State;
    } 
	
	// Update is called once per frame
	void FixedUpdate() {
        PerformState.Perform();
	}

    protected override void RemoveFromManager()
    {
        Managers.Units.RemoveUnit(this);
        PerformState = new IdleState(this);
        target = null;
    }

    public void SetEndTarget(Item item)
    {
        Hit = false;
        target = item;
    }

    public Item GetTarget()
    {
        return target;
    }

    public void SetGrid(Grid outer_grid)
    {
        grid = outer_grid;
    }

    public void InitUnit()
    {
        Dictionary<int, double> chars = Managers.Units.GetUnitChars(itemId, level);
        move_speed = (float)chars[6];
        attack = (float)chars[5];
        reload_time = (float)chars[2];
    }

    private void SetNextMovePoint()
    {
        if (movePoints.Count > 1)
        {
            int x = (int)movePoints[0].GetPosition().x + 1;
            int z = (int)movePoints[0].GetPosition().y + 1;
            destination = grid.GetPositionByXZ(x, z, 0);
        }
        else
        {
            Vector2 EndPos = movePoints[0].GetPosition();
            destination = new Vector3(EndPos.x, grid.transform.position.y, EndPos.y);
        }
        movePoints.RemoveAt(0);
    }

    public void MoveByPoints(List<Node> Nodes)
    {
        movePoints = Nodes;
        SetNextMovePoint();

        if (Animation)
        {
            Animation.Stop();
            Animation.Play(RunAnim);
        }

        PerformState = new MoveState(this);
    }

    public void Select(bool select)
    {
        Selected = select;
        projector.gameObject.SetActive(select);
    }

    public void SetEndDirection(Vector3 EndDir)
    {
        EndDirection = Quaternion.LookRotation(EndDir);
    }

    public override Vector2 GetPosition()
    {
        return new Vector2(transform.position.x, transform.position.z);
    }

    public override void SetInfoData(GameObject parent)
    {
        unit_info = parent.GetComponentInChildren<UnitInfo>(true);
        unit_info.gameObject.SetActive(true);
        SetInfoData();
    }

    public override void SetInfoData()
    {
        unit_info.SetChars(Managers.Units.GetUnitChars(itemId, level));
        unit_info.SetHiddenChars(Managers.Units.GetUnitChars(itemId, level + 1));
    }

    public override void RemoveData()
    {
        HideInfo();
        unit_info.gameObject.SetActive(false);
    }

    public override void ShowInfoUpgrade(bool show)
    {
        unit_info.ShowUpgrade(show);
    }

    //Attack
    public bool Attack()
    {
        if (target)
        {
            if (IsFriend(target))
            {
                target = null;
                return false;
            }

            if (Time.time > LastReload + reload_time)
            {
                if (animator)
                {
                    animator.SetTrigger(AttackAnim);
                }
                if (Animation)
                {
                    Animation.Stop();
                    Animation.Play(AttackAnim);
                }
                //ATTACK ANIMATION;
                Hit = true;
                LastReload = Time.time;
            }
            if (Time.time > LastReload + reload_time * DamageTimeKoef && Hit)
            {
                if (!target.GetDamage(attack))
                {
                    target = null;
                    if (Animation)
                    {
                        Animation.Stop();
                        Animation.Play(IdleAnim);
                    }
                }
                Hit = false;
            }
            return true;
        }
        return false;
    }

    public Vector3 GetMoveDirection()
    {
        if (Vector3.Distance(destination, transform.position) < 0.05f)
            return new Vector3(0, 0, 0);
        return (destination - transform.position).normalized * move_speed;
    }

    //Move method
    public bool Move()
    {
        Vector3 direction = destination - transform.position;
        direction.y = 0;

        if (direction.magnitude < 0.1f)
        {
            if (movePoints.Count == 0)
            {
                if (animator)
                {
                    animator.SetFloat("speedv", 0.0f);
                    animator.transform.localPosition = new Vector3(0, 0, 0);
                }
                if (Animation)
                {
                    Animation.Stop();
                    Animation.Play(IdleAnim);
                }
                return false;
            }

            SetNextMovePoint();
            direction = destination - transform.position;
            direction.y = 0;
        }

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotation_speed * Time.deltaTime);

        Vector3 forward = transform.TransformDirection(Vector3.forward);

        float speedModifer = Vector3.Dot(forward, direction.normalized);

        speedModifer = Mathf.Clamp01(speedModifer);

        float speed = move_speed * speedModifer;
        direction = forward * speed * Time.deltaTime;
        //Передаем движение в управление персонажем

        transform.position += direction;

        //Сообщаем анимации о нашей скорости
        if (animator)
            animator.SetFloat("speedv", move_speed * speedModifer);

        if (Animation)
        {
            Animation[RunAnim].speed = speed;
        }

        return true;
    }

    //Rotate
    public bool Rotate()
    {
        if (Quaternion.Angle(transform.rotation, EndDirection) < 1.0f) 
            return false;

        //Debug.Log("Rotate");
        transform.rotation = Quaternion.RotateTowards(transform.rotation, EndDirection, Time.deltaTime * 200);
        return true;
    }

    void Update()
    {

    }
}
