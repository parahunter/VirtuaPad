using UnityEngine;
using System.Collections;

public class AvatarPickup : MonoBehaviour 
{
	public Transform shieldPrefab;
	
	private Avatar mAvatar;
	private AvatarShoot avatarShoot;
	private AvatarManager avatarManager;
	
	void Start()
	{
		mAvatar = GetComponent<Avatar>();	
		avatarManager = GameObject.FindGameObjectWithTag("ServerBehaviour").GetComponent<AvatarManager>();
		avatarShoot = GetComponent<AvatarShoot>();
	}
	
	private Shield currentShield;
	public Shield CurrentShield
	{
		get{return currentShield;}	
	}
	
	void OnCollisionEnter(Collision collision)
	{
		//print("Av!");
		//print(collision.collider.tag);
		if(collision.collider.tag == "Pickupable")
		{
			pickup();
						
			Destroy(collision.collider.gameObject);
		}
	}
	
	void pickup()
	{
		avatarManager.VibrateClient(mAvatar.Id, (byte)0);

		int rand = Random.Range(0,2);
		
		switch(rand)
		{
		case 0:
			AddShield();
			break;
		case 1:
			avatarShoot.SetWeapon(AvatarShoot.WeaponType.superBazooka);
			break;
		default:
			break;
		}	
	}
		
	void AddShield()
	{
		print("add shield called");
		
		Transform newShield = (Transform)Instantiate(shieldPrefab,transform.position,transform.FindChild("GraphicsMain").rotation);
				
		newShield.parent = transform;
		
		newShield.renderer.material.color = mAvatar.avatarColor;
		
		if(currentShield != null)
		{
			Destroy(currentShield.gameObject);
		}
		
		currentShield = newShield.GetComponent<Shield>();
	}
}
