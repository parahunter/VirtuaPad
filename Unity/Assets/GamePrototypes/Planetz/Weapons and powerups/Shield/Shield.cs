using UnityEngine;
using System.Collections;

public class Shield : MonoBehaviour 
{
	
	public int durability = 1000;
	
	private Transform mAvatarGraphicsTrans;
	public float zModifier = 5.0f;
	
	private SphereCollider mSphereCollider;
	
	void Start () 
	{
		//print(transform.name);
		mAvatarGraphicsTrans = transform.parent.FindChild("GraphicsMain");
		mSphereCollider = GetComponent<SphereCollider>();
	}
	
	// Update is called once per frame
	void Update () 
	{
		transform.localScale = mAvatarGraphicsTrans.localScale*1.5f;
		mSphereCollider.radius = transform.localScale.x*5.8f;
	}
		
	void OnLevelWasLoaded()
	{
		Destroy(gameObject);	
	}
	
	public void TakeShieldDamage(int damage, int fromWhom)
	{
		if(durability - damage > 0)
		{
			durability -= damage;		
		}
		else
		{
			transform.parent.GetComponent<Avatar>().TakeDamage( damage - durability , fromWhom);
			Destroy(gameObject);
		}	
		
	}
	
}
