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

using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections;
using System.Text;

public enum xmgVideoPlaneFittingMode
{
    FitHorizontally,
    FitVertically,
};


public class xmgAugmentedFace : xmgAugmentedFaceBase
{
    private WebCamTexture m_WebcamTexture;
	private xmgWebCamTexture myWebCamEngine;
	private RigidFaceTrackingBridge.xmgImage image;
	
	private RigidFaceTrackingBridge.xmgTrackingParams trackingParams;
	private RigidFaceTrackingBridge.xmgRigidFaceData rigidData; 
	private GameObject m_facePivot, m_faceObject, m_faceMask;

	private RigidFaceTrackingBridge.xmgVideoCaptureOptions videoOptions;

    // -------------------------------------------------------------------------------------------------------------

    void Awake()
    {
        CheckeParameters();
        double fovx = (double)CameraFOVX * 3.1415 / 180.0;
        if (!useNativeCapture)
		{
			if (myWebCamEngine == null)
			{
				myWebCamEngine = (xmgWebCamTexture)gameObject.AddComponent(typeof(xmgWebCamTexture));
				myWebCamEngine.CaptureWidth = GetVideoCaptureWidth();
				myWebCamEngine.CaptureHeight = GetVideoCaptureHeight();
				myWebCamEngine.MirrorVideo = MirrorVideo;
				myWebCamEngine.CameraFOVX = CameraFOVX;
				myWebCamEngine.UseFrontal = UseFrontal;
				m_WebcamTexture = myWebCamEngine.CreateVideoCapturePlane(1.0f, videoPlaneFittingMode, videoCaptureIndex);
            }
            if (m_WebcamTexture)
            {
                int captureWidth = m_WebcamTexture.width, captureHeight = m_WebcamTexture.height;
                if (captureWidth < 100)
                {
                    // Unity BUG MACOSX
                    captureWidth = m_WebcamTexture.requestedWidth;
                    captureHeight = m_WebcamTexture.requestedHeight;
                }

                // Image has the size of obtained video capture resolution
                image.m_width = captureWidth;
                image.m_height = captureHeight;
                image.m_colorType = 3; 
				image.m_type = 0;
				image.m_flippedH = true;
				
				RigidFaceTrackingBridge.xzimgInitializeRigidTracking(new StringBuilder(), new StringBuilder(), new StringBuilder());
				trackingParams.m_size.w = GetProcessingWidth();
				trackingParams.m_size.h = GetProcessingHeight();
                trackingParams.m_rotate_mode = (int)captureDeviceOrientation;
				RigidFaceTrackingBridge.xzimgSetCalibration(fovx, GetProcessingWidth(image.m_width), GetProcessingHeight(image.m_height), 0, trackingParams.m_rotate_mode);
                trackingParams.m_detect_without_eyes = 1;
			}
			else
				Debug.Log("No camera detected with Unity webcamTexture!");
		}
        else
        {
            // __Prepare Video Capture
            videoOptions.resolution_mode = videoCaptureMode;
            videoOptions.frontal = UseFrontal ? 1 : 0;
            videoOptions.focus_mode = 1;
            videoOptions.exposure_mode = 1;
            videoOptions.while_balance_mode = 1;

            // Create a plane and get back its texture ID
            PrepareBackgroundPlane();
            trackingParams.m_detect_without_eyes = 1;

#if UNITY_ANDROID
            // Create the texture for video stream
            imgTexture = new Texture2D(GetVideoCaptureWidth(), GetVideoCaptureHeight(), TextureFormat.RGB24, false);
            gameObject.GetComponent<Renderer>().material.mainTexture = imgTexture;
            videoOptions.texture_ptr = imgTexture.GetNativeTexturePtr();

            int rotateMode = 1;
            if (Screen.orientation == ScreenOrientation.LandscapeRight) rotateMode = 2;
            else if (Screen.orientation == ScreenOrientation.Portrait) rotateMode = UseFrontal?1:3;
            else if (Screen.orientation == ScreenOrientation.LandscapeLeft) rotateMode = 0;
            else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown) rotateMode = UseFrontal?3:1;
            currentDeviceOrientation = (DeviceOrientation)Screen.orientation;
		
		    RigidFaceTrackingBridge.StartCameraAndInitialize(videoOptions.resolution_mode, UseFrontal, fovx, rotateMode, false);
#elif UNITY_IOS
            // Create the texture for video stream
		    imgTexture = new Texture2D(GetVideoCaptureWidth(), GetVideoCaptureHeight(), TextureFormat.BGRA32, false);
		    gameObject.GetComponent<Renderer>().material.mainTexture = imgTexture;
		    videoOptions.texture_ptr = imgTexture.GetNativeTexturePtr();
		
		    int status = RigidFaceTrackingBridge.xzimgFaceApiInitializeRigidTracking(ref videoOptions);
		    if (status != 1) Debug.Log("Initialization is impossible!");
		    trackingParams.m_size.w = GetProcessingWidth();
		    trackingParams.m_size.h = GetProcessingHeight();
		    trackingParams.m_rotate_mode = 0;
    
		    trackingParams.m_rotate_mode = GetScreenOrientationIndex();
		    //Debug.Log(trackingParams.m_rotate_mode);
            
		    RigidFaceTrackingBridge.xzimgSetCalibration(fovx, GetProcessingWidth(), GetProcessingHeight(), trackingParams.m_rotate_mode, trackingParams.m_rotate_mode);
		    trackingParams.m_detect_without_eyes = 1;
		    //trackingParams.m_non_rigid_init_quality = 0;

		    RigidFaceTrackingBridge.xzimgFaceApiReleaseRigidTracking();
		    status = RigidFaceTrackingBridge.xzimgFaceApiInitializeRigidTracking(ref videoOptions);
		    RigidFaceTrackingBridge.xzimgSetCalibration(fovx, GetProcessingWidth(), GetProcessingHeight(), trackingParams.m_rotate_mode, trackingParams.m_rotate_mode);
#endif
        }
    }

    // -------------------------------------------------------------------------------------------------------------

    void OnDisable()
    {
#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL)
        RigidFaceTrackingBridge.xzimgReleaseRigidTracking();
		myWebCamEngine.ReleaseVideoCapturePlane();
#elif UNITY_ANDROID
		RigidFaceTrackingBridge.Release();
#elif UNITY_IOS
		RigidFaceTrackingBridge.xzimgFaceApiReleaseRigidTracking();
#endif
        DisposeObjects();
    }

    // -------------------------------------------------------------------------------------------------------------

    void Update ()
    {
        int detectedStatus = 0;
        if (!useNativeCapture)
        {
            if (myWebCamEngine == null || !myWebCamEngine.GetData()) return;
            image.m_imageData = myWebCamEngine.m_PixelsHandle.AddrOfPinnedObject();
            detectedStatus = RigidFaceTrackingBridge.xzimgRigidTracking(ref image, ref trackingParams, ref rigidData);

            myWebCamEngine.ApplyTexture();
        }
        else
        {
#if UNITY_ANDROID
		    float []vctPose = new float[7];
		    vctPose = RigidFaceTrackingBridge.xzimgAugmentedFaceDetect(videoOptions.texture_ptr);
            detectedStatus = (int)vctPose[0];
		    rigidData.m_position = new Vector3(vctPose[1], vctPose[2], vctPose[3]);
		    rigidData.m_euler = new Vector3(vctPose[4], vctPose[5], vctPose[6]);

            //PrintDeviceOrientation();
            if (detectedStatus == 0)
            {
                 DeviceOrientation idxOrientation = Input.deviceOrientation;
               // Debug.Log("Orientation");
                if (idxOrientation != currentDeviceOrientation &&  
                    idxOrientation != DeviceOrientation.Unknown && 
                    idxOrientation != DeviceOrientation.FaceUp && 
                    idxOrientation != DeviceOrientation.FaceDown )
                {
                    int rotateMode = 1;
                    if (idxOrientation == DeviceOrientation.Portrait) rotateMode = UseFrontal?1:3;
                    else if (idxOrientation == DeviceOrientation.LandscapeRight) rotateMode = 2;
                    else if (idxOrientation == DeviceOrientation.LandscapeLeft) rotateMode = 0;
                    else if (idxOrientation == DeviceOrientation.PortraitUpsideDown) rotateMode = UseFrontal?3:1;
                    RigidFaceTrackingBridge.SetNewDeviceOrientation(rotateMode);


                    currentDeviceOrientation = idxOrientation;
                }

            }
#elif UNITY_IOS
		    trackingParams.texturePtr = imgTexture.GetNativeTexturePtr();
			detectedStatus = RigidFaceTrackingBridge.xzimgFaceApiRigidTracking(ref trackingParams, ref rigidData);
        
            // to prevent Unity bug on iOS
            UpdateBackgroundPlaneOrientation();

			if (detectedStatus == 0)
		    {
			    DeviceOrientation idxOrientation = Input.deviceOrientation;
			    // Debug.Log("Orientation");
                if (idxOrientation != currentDeviceOrientation &&  
                    idxOrientation != DeviceOrientation.Unknown && 
                    idxOrientation != DeviceOrientation.FaceUp && 
                    idxOrientation != DeviceOrientation.FaceDown )
			    {
				    // Debug.Log("****************Orientation changed********************");				
				    if (idxOrientation == DeviceOrientation.LandscapeRight) trackingParams.m_rotate_mode = 0;	// Anticlockwize
				    else if (idxOrientation == DeviceOrientation.Portrait) trackingParams.m_rotate_mode = 3;
				    else if (idxOrientation == DeviceOrientation.LandscapeLeft)  trackingParams.m_rotate_mode = 2;
				    else if (idxOrientation == DeviceOrientation.PortraitUpsideDown) trackingParams.m_rotate_mode = 1;
				    currentDeviceOrientation = idxOrientation;
			    }
			
		    }
#endif
        }

        if (detectedStatus > 0)
		{
			ShowObject(true);
			UpdateObjectPosition();
		}
		
		if (detectedStatus <= 0)
		{
			ShowObject(false);
		}
		
		UpdateDebugDisplay(detectedStatus);
	}

    // -------------------------------------------------------------------------------------------------------------

    void OnGUI()
    {
        if (ScreenDebug)
        {
#if (UNITY_STANDALONE || UNITY_EDITOR)
            if (m_WebcamTexture != null)
                GUILayout.Label("Screen: " + Screen.width + "x" + Screen.height + " - " + m_WebcamTexture.requestedWidth + "x" + m_WebcamTexture.requestedHeight);
#endif

            GUILayout.Label(m_debugStatus);
			GUILayout.Label("Screen Orientation = " + Screen.orientation);
			GUILayout.Label("Device Orientation = " + Input.deviceOrientation);
        }
    }

    // -------------------------------------------------------------------------------------------------------------

    void ShowObject(bool state)
    {
        CreateObjects();
        Renderer[] renderers = m_facePivot.GetComponentsInChildren<Renderer>();
        foreach (Renderer r in renderers) r.enabled = state;
    }

    // -------------------------------------------------------------------------------------------------------------

    void CreateObjects()
	{
		if (m_facePivot == null)
		{
			m_facePivot = new GameObject("Face Pivot");
			
			GameObject[] gos = GameObject.FindObjectsOfType(typeof(GameObject)) as GameObject[];
			foreach (GameObject g in gos)
			{
				if (g.name == "faceObject")
				{
					m_faceObject = g;
					m_facePivot.transform.parent = m_faceObject.transform.parent;
					m_faceObject.transform.parent = m_facePivot.transform;
					Renderer[] renderers = m_faceObject.GetComponentsInChildren<Renderer>();
					foreach (Renderer r in renderers)
					{
						r.material.renderQueue = 3020;
					}
				}
				if (g.name == "faceMask")
				{
					m_faceMask = g;
					m_faceMask.transform.parent = m_facePivot.transform;
				}
			}
		}
	}

    // -------------------------------------------------------------------------------------------------------------

    void UpdateObjectPosition()
	{
		CreateObjects();
		if (m_facePivot)
		{
			Vector3 position = rigidData.m_position; position.y = -position.y;
			Vector3 euler = rigidData.m_euler;
			Quaternion quat = Quaternion.Euler(euler);

#if (UNITY_STANDALONE || UNITY_EDITOR || UNITY_WEBGL || UNITY_ANDROID)
            if (MirrorVideo)
#elif UNITY_IOS
			if ((MirrorVideo && UseFrontal) || (!MirrorVideo && !UseFrontal))
#endif
            {
				quat.y = -quat.y;
				quat.z = -quat.z;   
				position.x = -position.x;
			}
            m_facePivot.transform.position = position;
            m_facePivot.transform.rotation = quat;
		}
	}

    // -------------------------------------------------------------------------------------------------------------

    void DisposeObjects()
	{
		Destroy(m_facePivot.gameObject);
	}


#if (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
    void PrintDeviceOrientation()
    { 
        Debug.Log("Orientation = " + Input.deviceOrientation);
    }
    void PrintDeviceCurrentOrientation()
    { 
        Debug.Log("Orientation = " + currentDeviceOrientation);
    }
#endif
}
