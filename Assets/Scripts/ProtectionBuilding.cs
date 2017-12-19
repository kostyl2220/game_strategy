using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProtectionBuilding : Item {

    public GameObject TowerTop;
    public Transform Round;
    public Transform ShootPoint;
    public float minShootAngle = 5.0f;

    private float rotate_speed = 20f;
    private float reload_time;
    private float bullet_speed;
    private float radius;
    private List<int> bullets;

    private ProtectionInfo protection_info;
    private Quaternion destQuat;
    private Quaternion startQuat;
    private bool Right = false;
    private int bulletNum = 0;
    private GameObject target;

    private bool init = false;
    private bool isTarget = false;

    private float lastReload = -1;

    // Use this for initialization
    void Start () {
        Round.gameObject.SetActive(false);
    }

    void SetNewDestination()
    {
        destQuat = Quaternion.Euler(0, 60 * (Right ? 1 : -1), 0) * startQuat;
        Right = !Right;
    }

    public void InitTower()
    {
        SetParams();
        startQuat = transform.rotation;
        SetNewDestination();
        init = true;
    }

    void TryShot()
    {
        if (isTarget & Time.time > lastReload + reload_time)
        {
            if (!TowerTop || Quaternion.Angle(TowerTop.transform.rotation, destQuat) < minShootAngle)
            {
                //shoot
                if (!TowerTop)
                {
                    ShootPoint.transform.rotation = destQuat;
                }

                float height = ShootPoint.transform.position.y - target.transform.position.y;
                Vector3 shotXY = ShootPoint.transform.position;
                shotXY.y = 0;
                Vector3 targetXY = target.transform.position;
                targetXY.y = 0;
                float distance = Vector3.Distance(shotXY, targetXY);
                float g = Physics.gravity.magnitude;
                float D = 1 - Mathf.Pow(g * distance / Mathf.Pow(bullet_speed, 2), 2) + 2 * g * height / Mathf.Pow(bullet_speed, 2);
                if (D > 0)
                {
                    float tan = Mathf.Pow(bullet_speed, 2) / (g * distance) * (1 - Mathf.Sqrt(D));
                    float angle = Mathf.Atan2(tan, 1) * 180 / Mathf.PI;

                    TowerBullet tower_bullet = TowerManager.GetBullet(bullets[bulletNum]);
                    Quaternion rot = Quaternion.AngleAxis(-1 * angle, ShootPoint.transform.right);
                    Bullet bullet = Instantiate(tower_bullet.Bullet, ShootPoint.transform.position, rot * ShootPoint.rotation);
                    bullet.transform.position = ShootPoint.transform.position;

                    Rigidbody bulletRigid = bullet.GetComponent<Rigidbody>();
                    bulletRigid.velocity = rot * ShootPoint.transform.forward * bullet_speed;
                    Physics.IgnoreCollision(bulletRigid.GetComponent<Collider>(), transform.GetComponent<Collider>());
                    lastReload = Time.time;
                }
            }
        }
    }
    
    void PassiveRotation()
    {
        isTarget = false;
        if (TowerTop)
        {
            if (TowerTop.transform.rotation == destQuat)
            {
                SetNewDestination();
            }
        }
    }

    void SetEnemyLook()
    {
        int layer = LayerMask.GetMask("Enemy");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layer);
        if (hitColliders.Length == 0)
        {
            PassiveRotation();
            return;
        }
        isTarget = true;

        float minDistance = radius * radius * 2;
        Collider minColl = null;
        foreach (var collider in hitColliders)
        {
            Vector3 dir = collider.transform.position - transform.position;
            Ray ray = new Ray(transform.position, dir);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider != collider)
                    continue;
            }

            float distance = (transform.position - collider.transform.position).sqrMagnitude;
            if (distance < minDistance)
            {
                minDistance = distance;
                minColl = collider;
            }
        }

        if (minColl == null)
        {
            PassiveRotation();
            return;
        }
        target = minColl.gameObject;

        destQuat = Quaternion.LookRotation(minColl.transform.position - transform.position, transform.up);
        destQuat.eulerAngles = new Vector3(0, destQuat.eulerAngles.y, 0);

    }
    // Update is called once per frame
    void Update () {
        if (init)
        {
            SetEnemyLook();
            TryShot();
        }
        if (TowerTop)
        {
            TowerTop.transform.rotation = Quaternion.RotateTowards(TowerTop.transform.rotation, destQuat, Time.deltaTime * rotate_speed);
        }
    }

    void SetParams()
    {
        Dictionary<int, double> data = TowerManager.GetLevelChars(itemId, level);
        bullets = TowerManager.GetLevelBullets(itemId, level);
        if (data.ContainsKey(1))
        {
            rotate_speed = (float)data[1];
        }
        reload_time = (float)data[2];
        bullet_speed = (float)data[3];
        radius = (float)data[4] / 5;
        bulletNum = 0;
        Round.transform.localScale = new Vector3(radius, radius, radius);
        radius *= 5;
    }

    protected override void UpdateAfterLevel()
    {
        SetParams();
    }

    public override void SetInfoData(GameObject parent)
    {
        protection_info = parent.GetComponentInChildren<ProtectionInfo>(true);
        protection_info.gameObject.SetActive(true);
        Round.gameObject.SetActive(true);
        SetInfoData();
    }

    public override void SetInfoData()
    {
        protection_info.SetChars(TowerManager.GetLevelChars(itemId, level));
        protection_info.SetHiddenChars(TowerManager.GetLevelChars(itemId, level + 1));
        protection_info.SetBullets(TowerManager.GetLevelBullets(itemId, level));
        protection_info.SetHiddenBullets(TowerManager.GetLevelBullets(itemId, level + 1));
    }

    public override void RemoveData()
    {
        HideInfo();
        protection_info.gameObject.SetActive(false);
    }

    public override void ShowInfoUpgrade(bool show)
    {
        protection_info.ShowUpgrade(show);
    }

    public override void HideInfo()
    {
        Round.gameObject.SetActive(false);
    }
}
