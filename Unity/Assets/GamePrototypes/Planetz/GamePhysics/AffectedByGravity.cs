using UnityEngine;
using System.Collections;

public class AffectedByGravity : MonoBehaviour 
{
	
	private GravityWell well;
	private Transform wellTransform;
	
	
	
	private bool inWell = false;
	public bool InWell
	{
		get{return inWell;}	
	}
	
	private Vector3 gravity;
	public Vector3 Gravity
	{
		get{return gravity;}	
	}
	
	public float inWellDrag = 0.0f;
	public float inSpaceDrag = 0.0F;
	
	//private Gravity gravityScript;
	
	public void SetWell(GravityWell well)
	{
		inWell = true;
		this.wellTransform = well.transform;
		this.well = well;
		rigidbody.drag = inWellDrag;
	}
	
	public void SetNotInWell()
	{
		//this.well = null;
		
		inWell = false;
		rigidbody.drag = inSpaceDrag;
	}
	
	public void Start()
	{
		rigidbody.drag = inSpaceDrag;
		//gravityScript = GameObject.Find("GravityManager").GetComponent<Gravity>();	
	}
	
	void OnTriggerEnter(Collider other)
	{
		if(other.tag == "GravityWell")
		{
			SetWell( other.GetComponent<GravityWell>() );	
		}
	}
	
	void OnTriggerStay(Collider other)
	{
		if(other.tag == "GravityWell")
		{
			SetWell( other.GetComponent<GravityWell>() );	
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		if(other.tag == "GravityWell")
		{
			SetNotInWell();
		}
	}
	
	public void OnLevelWasLoaded()
	{
		SetNotInWell();
	}	
	
	public void FixedUpdate()
	{
		if(!inWell)
			return;
		
		Vector3 vecToWell = wellTransform.position - transform.position;
		
		gravity = vecToWell.normalized*well.acceleration;// / shortestVecToWell.sqrMagnitude;
		
		Debug.DrawRay(transform.position, gravity);
		
		//apply it
		rigidbody.AddForce(gravity,ForceMode.Acceleration);
		
		
	}
}
