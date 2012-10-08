using UnityEngine;
using System.Collections;
using System;

public static class CollaborateOrDieClientDataFactory
{
	#region ClientDataFactory implementation
	public static CollaborateOrDieClientData UnpackData (byte[] packet)
	{
		CollaborateOrDieClientData accData = new CollaborateOrDieClientData();

		accData.xAcc = EndianBitConverter.Big.ToSingle(packet,1);
		accData.yAcc = EndianBitConverter.Big.ToSingle(packet,5);
		accData.zAcc = EndianBitConverter.Big.ToSingle(packet,9);
		accData.shooting = Convert.ToBoolean(packet[13]);
		accData.shaking = Convert.ToBoolean(packet[14]);
		
		return accData;
	}
	#endregion
}
