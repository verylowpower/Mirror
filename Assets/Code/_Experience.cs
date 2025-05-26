using System;
using UnityEngine;

public class Experience : MonoBehaviour
{
    public int expAmount;
    public float moveSpeed;
    public Transform player;

    void Start()
    {
        if (Character.instance != null)
        {
            player = Character.instance.transform;
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < Character.instance.collectRadious)
        {
            Debug.Log("EXP SCRIPT WORKING FINE");
            transform.position = Vector2.MoveTowards(transform.position, player.position, moveSpeed);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("player"))
        {
            if (Character.instance != null)
            {
                Character.instance.ModifyExp(expAmount);
            }

            Destroy(gameObject);
        }
    }


}
