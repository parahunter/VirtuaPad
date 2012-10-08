using UnityEngine;
using System.Collections;

public interface ServerTcpEventSubscriber
{
	void Recieve(byte id, byte[] packet, int lengthOfPacket);
}
