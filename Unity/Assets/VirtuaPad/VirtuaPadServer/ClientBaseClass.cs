using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class VPClientBaseClass
{
	/// <summary>
	/// The states the clients can be in
	/// </summary>
	public enum ClientConnectionStates{disconnected, tryingToConnect, connected, tryingToJoin, joined};

	/// <summary>
	/// The ID of the client stored as a byte
	/// </summary>
	public byte ID;
	/// <summary>
	/// stae of the client
	/// </summary>
	public ClientConnectionStates connectionState;
	/// <summary>
	/// NOT IMPLEMENTED YET - should signify when we last got a package from the client
	/// </summary>
	public long lastPacketTime;
	/// <summary>
	/// data from the client
	/// </summary>
	public Color color;
	public TcpClient tcpClient;
	public Queue<byte[]> messagesToSend = new Queue<byte[]>();
	
	//used to block tcpWriteThread
	public AutoResetEvent autoEvent = new AutoResetEvent(true);
	
	public virtual void UnpackUdpData(byte[] bytes)
	{
		
	}
	
	public virtual void Close ()
	{
		this.tcpClient.Close();
		this.connectionState = VPClientBaseClass.ClientConnectionStates.disconnected;
	}
}
