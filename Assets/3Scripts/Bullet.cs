using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public bool isMelee;
    public bool isRock;

     void OnCollisionEnter(Collision collision)
    {
        if (!isRock && collision.gameObject.tag == "Floor")
        {
            Destroy(gameObject, 3);
        }

        // 총알이 벽이나 다른 오브젝트와 충돌할 때 제거
        if (collision.gameObject.tag != "Player" && collision.gameObject.tag != "Bullet")
        {
            Destroy(gameObject, 3); // 충돌 시 총알 제거
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isMelee && other.gameObject.tag == "Wall")
        {
            Destroy(gameObject); // 벽에 닿으면 총알 제거
        }

        // 다른 오브젝트와의 충돌 시 제거
        if (other.gameObject.tag != "Player" && other.gameObject.tag != "Bullet")
        {
            Destroy(gameObject, 3); // 충돌 시 총알 제거
        }
    }
}
