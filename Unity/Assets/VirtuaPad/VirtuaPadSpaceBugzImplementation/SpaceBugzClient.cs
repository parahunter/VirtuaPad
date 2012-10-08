using UnityEngine;
using System.Collections;
using System;

public class SpaceBugzClient : VPClientBaseClass, IComparable<SpaceBugzClient>
{
	public SpaceBugzClient()
	{
			
	}

	#region IComparable[PlanetClient] implementation
	int IComparable<SpaceBugzClient>.CompareTo (SpaceBugzClient other)
	{
		return this.score - other.score;
	}
	
	#endregion

	
	public Vector3 accelerationVector;
	public Vector2 aimVector;
	public bool isJumping = false;
	public bool isShooting = false;
	public bool isAiming = false;
	public bool InputDisabled = false;
	private byte score = 0;
	public byte Score
	{
		get{return score;}
	}
	
	public void ResetScore()
	{
		score = 0;
	}
	
	public void AddScore()
	{
		score++;
	}
	
	public override void UnpackUdpData (byte[] packet)
	{
		if(InputDisabled)
		{
			isJumping = false;
			isAiming = false;
			isShooting = false;	
			return;
		}
		
		accelerationVector.y = -EndianBitConverter.Big.ToSingle(packet,1);
		accelerationVector.x = EndianBitConverter.Big.ToSingle(packet,5);
		accelerationVector.z = EndianBitConverter.Big.ToSingle(packet,9);
		
		isJumping = EndianBitConverter.Big.ToBoolean(packet,13);
		
		isShooting = EndianBitConverter.Big.ToBoolean(packet,14);
		
		aimVector.x = -EndianBitConverter.Big.ToSingle(packet,15);
		aimVector.y = EndianBitConverter.Big.ToSingle(packet,19);
		
		if(isShooting)
			isAiming = true;
		
		//Debug.Log(accelerationVector);
			//Debug.Log("is jumping " + isJumping);
		
		//Debug.Log("is shooting" + isShooting);
		
		//throw new System.NotImplementedException ();
	}
	
	public bool shouldShoot()
	{	
		if(isAiming == true && isShooting == false)
		{
			isAiming = false;
			
			return true;
		}
		
		return false;
	}

	public override void Close ()
	{
		this.connectionState = VPClientBaseClass.ClientConnectionStates.disconnected;
		this.tcpClient.Client.Close();
		this.tcpClient.Close();
	}

	

}
