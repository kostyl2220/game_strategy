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
    private Unit target;

    private bool init = false;

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
        if (TowerTop)
            TowerTop.transform.rotation = transform.rotation;
        Round.gameObject.SetActive(false);
        SetParams();
        startQuat = transform.rotation;
        SetNewDestination();
        init = true;
    }

    void TryShot()
    {
        if (target != null & Time.time > lastReload + reload_time)
        {
            if (!TowerTop || Quaternion.Angle(TowerTop.transform.rotation, destQuat) < minShootAngle)
            {
                //shoot
                if (!TowerTop)
                {
                    ShootPoint.transform.rotation = destQuat;
                }

                float angle = GetShootAngle();
                if (angle != -1000)
                {
                    TowerBullet tower_bullet = TowerManager.GetBullet(bullets[bulletNum]);
                    Quaternion rot = Quaternion.AngleAxis(-1 * angle, ShootPoint.transform.right);
                    Bullet bullet = Instantiate(tower_bullet.Bullet, ShootPoint.transform.position, rot * ShootPoint.rotation);
                    bullet.transform.position = ShootPoint.transform.position;
                    bullet.SetDamage(tower_bullet.Damage);
                    bullet.SetPlayerName(PlayerName);

                    Rigidbody bulletRigid = bullet.GetComponent<Rigidbody>();
                    bulletRigid.velocity = rot * ShootPoint.transform.forward * bullet_speed;
                    Physics.IgnoreCollision(bulletRigid.GetComponent<Collider>(), transform.GetComponent<Collider>());
                    lastReload = Time.time;
                }
            }
        }
    }
    
    float GetShootAngle()
    {
        float g = Physics.gravity.magnitude;
        float height = ShootPoint.transform.position.y - target.transform.position.y;
        Vector3 shotXY = ShootPoint.transform.position;
        shotXY.y = 0;
        Vector3 InitTargetXY = target.transform.position + new Vector3(0, 0.5f, 0);
        InitTargetXY.y = 0;

        float time = 0;
        float angle = -1000;
        for (int i = 0; i < 3; ++i)
        {
            Vector3 targetXY = InitTargetXY + target.GetMoveDirection() * time;
            float distance = Vector3.Distance(shotXY, targetXY);
            float D = 1 - Mathf.Pow(g * distance / Mathf.Pow(bullet_speed, 2), 2) + 2 * g * height / Mathf.Pow(bullet_speed, 2);
            if (D > 0)
            {
                float tan = Mathf.Pow(bullet_speed, 2) / (g * distance) * (1 - Mathf.Sqrt(D));
                angle = Mathf.Atan2(tan, 1);
                time = distance / (bullet_speed * Mathf.Cos(angle));
                angle *= 180 / Mathf.PI;
            }
            else return angle;
        }
        return angle;
    }
    
    void PassiveRotation()
    {
        if (TowerTop)
        {
            if (TowerTop.transform.rotation == destQuat)
            {
                SetNewDestination();
            }
        }
    }

    protected override void RemoveFromManager()
    {
        init = false;
        Managers.Items.RemoveItem(this);
    }

    void SetEnemyLook()
    {
        int layer = LayerMask.GetMask("Item");
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, radius, layer);
        if (hitColliders.Length == 0)
        {
            PassiveRotation();
            return;
        }

        target = null;
        float minValue = 1000000;
        Collider minColl = null;
        foreach (var collider in hitColliders)
        {
            Item item = collider.GetComponent<Item>();
            if (item && IsFriend(item))
                continue;

            Vector3 direction = collider.transform.position - transform.position;

            float Value = direction.sqrMagnitude;
            if (TowerTop)
            {
                Value = Mathf.Abs(Quaternion.Angle(Quaternion.LookRotation(direction), TowerTop.transform.rotation));
            }
            if (Value < minValue)
            {
                minValue = Value;
                minColl = collider;
            }
        }

        if (minColl == null)
        {
            PassiveRotation();
            return;
        }
        target = minColl.GetComponent<Unit>();

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
