using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSpawnBullet : TickBehaviour
{
    public BulletTest bulletPrefab;
    PoolComponent<BulletTest> mPool;
    private void Start()
    {
        mPool = new PoolComponent<BulletTest>(bulletPrefab, transform, 10);
    }

    [Button]
    void TestSpawn()
    {
        var item = mPool.Spawn();
        item.name = "Test " + item.transform.GetSiblingIndex();
    }
    [Button]
    void TestRecycle()
    {
        var bullet = transform.GetComponentInChildren<BulletTest>();
        mPool.Recycle(bullet);
    }
    [Button]
    void TestRecycleAll()
    {

        mPool.RecycleAll();
    }
}
