using UnityEngine;
using System.Collections;

public class BazookaExplosion : MonoBehaviour 
{
	
	public float explodeDuration = 0.3f;
	public float stayDuration = 0.1f;
	public float growthFactor = 1.05f;
	public float amountOfColorToRemove = 0.2f;
	
	// Use this for initialization
	void Start () 
	{
		StartCoroutine(Explode());	
	}
		
	void OnTriggerEnter(Collider collider)
	{
		if(collider.tag == "PlanetChunk")
		{
			Color chunkColor = collider.renderer.material.color;
			chunkColor.g -= amountOfColorToRemove;
			
			if(chunkColor.g < amountOfColorToRemove)
				Destroy(collider.gameObject);
			else
			{
				//chunkColor.g -= amountOfColorToRemove;
				chunkColor.b -= amountOfColorToRemove;
				collider.renderer.material.color = chunkColor;
			}
			
		}
		
	}
	
	IEnumerator Explode()
	{
		GetComponent<AudioShufler>().Play();
		
		float startTime = Time.time;
		
		while(startTime + explodeDuration > Time.time)
		{
			Vector3 scale = transform.localScale;
			
			float additionToScale = growthFactor*Time.deltaTime;
			
			scale.x += additionToScale;
			scale.y += additionToScale;
			scale.z += additionToScale;
						
			transform.localScale = scale;
			yield return 0;
		}
		
		yield return new WaitForSeconds(stayDuration);
		
		Destroy(gameObject);
	}
}
