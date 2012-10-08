using UnityEngine;
using System.Collections;

public class CreateExplosionOnImpact : MonoBehaviour 
{
	public Transform explosionPrefab;
	
	private bool hasSpawnedExplosion = false;
	
	void OnCollisionEnter(Collision col)
	{
		if(hasSpawnedExplosion)
			return;
		
		hasSpawnedExplosion = true;
		
		Transform explosion = (Transform)Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		
		explosion.FindChild("Plane").renderer.material.color = transform.FindChild("Plane").renderer.material.color;
		
		explosion.Find("Plane").GetComponent<ShootByAvatar>().id = GetComponent<ShootByAvatar>().id;
		
		Destroy(gameObject);
	}
}
