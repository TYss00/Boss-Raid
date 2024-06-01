using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range}; //Melee가 근접  Range가 원거리
    public Type type;
    public int damage; // 뎀지
    public float rate; //공속
    public BoxCollider meleeArea; // 공격 범위
    public TrailRenderer trailEffect;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
    }


    //코루틴(co-Routine함수)
    IEnumerator Swing()
    {
        //1
        yield return new WaitForSeconds(0.2f);
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        //2
        yield return new WaitForSeconds(0.3f);
        meleeArea.enabled = false;
        //3
        yield return new WaitForSeconds(0.3f);
        trailEffect.enabled = false;
    }
}
