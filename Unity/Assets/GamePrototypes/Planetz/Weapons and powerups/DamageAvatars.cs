using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct DamageInfo
{
	public byte id;
	public int damage;
	
	public DamageInfo(byte id, int damage)
	{
		this.id = id;
		this.damage = damage;
	}
}

public class DamageAvatars : MonoBehaviour 
{
	public int damage = 1337;
	
	void OnCollisionEnter(Collision collision)
	{
		if(collision.collider.tag == "Avatar")
		{
			collision.collider.GetComponent<Avatar>().TakeDamage(damage, GetComponent<ShootByAvatar>().id);
		}
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Avatar")
		{
			other.GetComponent<Avatar>().TakeDamage(damage, GetComponent<ShootByAvatar>().id);
		}
	}
}
