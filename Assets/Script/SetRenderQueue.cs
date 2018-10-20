/**
*
* Copyright (c) 2015 XZIMG , All Rights Reserved
* No part of this software and related documentation may be used, copied,
* modified, distributed and transmitted, in any form or by any means,
* without the prior written permission of xzimg
*
* The XZIMG company is located at 903 Dannies House, 20 Luard Road, Wanchai, Hong Kong
* contact@xzimg.com, www.xzimg.com
*
*/

/*
	SetRenderQueue.cs
 
	Sets the RenderQueue of an object's materials on Awake. This will instance
	the materials, so the script won't interfere with other renderers that
	reference the same materials.
*/
using UnityEngine;

[AddComponentMenu("Rendering/SetRenderQueue")]

public class SetRenderQueue : MonoBehaviour {
	
	[SerializeField]
	protected int[] m_queues = new int[]{3000};
	
	protected void Awake() {
		Material[] materials = GetComponent<Renderer>().materials;
		for (int i = 0; i < materials.Length && i < m_queues.Length; ++i) {
			materials[i].renderQueue = m_queues[i];
		}
	}
}