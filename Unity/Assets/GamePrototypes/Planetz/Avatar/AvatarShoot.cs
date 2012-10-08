using UnityEngine;
using System.Collections;

public class AvatarShoot : MonoBehaviour 
{
	public enum WeaponType{bazooka, superBazooka, lazor,lightsaber};
	private WeaponType currentWeapon = WeaponType.bazooka;
	
	public Rigidbody BazookaBullet;
	public Rigidbody superBazookaBullet;
	public int numOfSuperShootsOnPickup = 3;
	public float BazookaShootForce;
	public float SuperBazookaShootForce = 200;
	public int id;
	
	private AvatarPickup mAvatarPickup;
	private Avatar mAvatar;
	
	private Collider avatarCollider;
	
	void Start()
	{
		mAvatarPickup = GetComponent<AvatarPickup>();
		mAvatar = GetComponent<Avatar>();
		
		avatarCollider = transform.Find("GraphicsMain").collider;
	}
	
	public void SetWeapon(WeaponType weapon)
	{
		currentWeapon = weapon;
		
		switch(weapon)
		{
		case WeaponType.bazooka:
			break;
		case WeaponType.superBazooka:
			numOFSuperBazookaShots += numOfSuperShootsOnPickup;
			break;
		default:
			break;
		}
	}
	
	public void Shoot(Vector3 direction)
	{
		if(direction.sqrMagnitude <= 0.1f)
			return;
		
		switch(currentWeapon)
		{
		case WeaponType.bazooka:
			bazookaShoot(direction);
			break;
		case WeaponType.superBazooka:
			superBazookaShoot(direction);
			break;
		default:
			break;
			
		}
	}
	
	//methods, members and coroutines for the bazooka and super bazooka
	private bool bazookaCanShoot = true;
	public float BazookaShootDelay = 1.0f;
	
	private int numOFSuperBazookaShots = 0;
	
	private void bazookaShoot(Vector3 direction)
	{
		if(bazookaCanShoot)
			StartCoroutine(bazookaDelayCoroutine());
		else
			return;
		
		mAvatar.MakeVolnurable();
		
		Rigidbody newBullet;
		
		newBullet = (Rigidbody)Instantiate(BazookaBullet, transform.position + direction.normalized*4, Quaternion.identity);
		
		newBullet.GetComponent<ShootByAvatar>().id = id;
		
		if(mAvatarPickup.CurrentShield != null)
			Physics.IgnoreCollision(newBullet.collider, mAvatarPickup.CurrentShield.collider);
		
		Physics.IgnoreCollision(newBullet.collider, avatarCollider);
			
		newBullet.transform.FindChild("Plane").renderer.material.color = mAvatar.avatarColor;
		
		newBullet.AddForce(direction.normalized*BazookaShootForce, ForceMode.VelocityChange);	
	}
	
	private void superBazookaShoot(Vector3 direction)
	{
		if(bazookaCanShoot)
			StartCoroutine(bazookaDelayCoroutine());
		else
			return;
		
		numOFSuperBazookaShots--;
		
		mAvatar.MakeVolnurable();
		
		Rigidbody newBullet = (Rigidbody)Instantiate(superBazookaBullet, transform.position + direction.normalized*4, Quaternion.identity);
	
		newBullet.GetComponent<ShootByAvatar>().id = id;
		
		if(mAvatarPickup.CurrentShield != null)
			Physics.IgnoreCollision(newBullet.collider, mAvatarPickup.CurrentShield.collider);
		
		Physics.IgnoreCollision(newBullet.collider, collider);
			
		newBullet.transform.FindChild("Plane").renderer.material.color = mAvatar.avatarColor;
		
		newBullet.AddForce(direction.normalized*SuperBazookaShootForce, ForceMode.VelocityChange);	
		
		if(numOFSuperBazookaShots <= 0)
			SetWeapon(WeaponType.bazooka);
	}
	
	IEnumerator bazookaDelayCoroutine()
	{
		bazookaCanShoot = false;
		yield return new WaitForSeconds(BazookaShootDelay);
		bazookaCanShoot = true;
	}
	
	
}
