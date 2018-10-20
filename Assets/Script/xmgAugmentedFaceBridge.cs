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
using System.Collections;
using System.Text;
using System;
using System.Runtime.InteropServices;

/**
 * This class contains the interface with the plugin for different platforms
 */
public class RigidFaceTrackingBridge {
	
#region Data Structures
	[StructLayout(LayoutKind.Sequential)]
	public struct xmgImage
	{
		public int  m_width;
		public int  m_height;
		
		public IntPtr m_imageData;
		
		/** 0: Black and White, 1: Color RGB, 2: Color BGR, 3: Color RGBA, 4: Color ARGB */
		public int m_colorType;
		
		/** 0: unsigned char, 1: float, 2: double */
		public int m_type;
		
		/** Has the image to be flipped horinzontally */
		public bool m_flippedH;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct xmgPoint2
	{
		public double x, y;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct xmgPoint3
	{
		public double x, y, z;
	}
	
	[StructLayout(LayoutKind.Sequential)]
	public struct xmgSize
	{
		public int w, h;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct xmgTrackingParams
	{
		public xmgSize m_size;
		public int m_rotate_mode;
		public int m_detect_without_eyes;
		public System.IntPtr texturePtr;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct xmgRigidFaceData
	{
		public double m_FaceCorner_x;
		public double m_FaceCorner_y;
		public double m_FaceSize_w;
		public double m_FaceSize_h;
		
		public int m_iPoseDetected;
		public Vector3 m_position;
		public Vector3 m_euler;
		public Quaternion m_quatRot;
		
		public xmgPoint3 m_leftEye;
		public xmgPoint3 m_rightEye;
	}
	[StructLayout(LayoutKind.Sequential)]
	public struct xmgVideoCaptureOptions
	{
		public int resolution_mode;	// 0 is 320x240; 1, is 640x480; 2 is 720p
		public int frontal;			// 0 is frontal; 1 is back
		public int focus_mode;			// 0 auto-focus now; 1 auto-focus continually; 2 locked; 3; focus to point
		public int exposure_mode;		// same
		public int while_balance_mode;	//
		public System.IntPtr texture_ptr;
        public System.IntPtr uvTexture_ptr;
    }
    #endregion

#if ((UNITY_STANDALONE || UNITY_EDITOR || UNITY_ANDROID) && !UNITY_IOS)

    // Import marker detection functions
    [DllImport("xzimgAugmentedFace", CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public static extern void xzimgInitializeRigidTracking(StringBuilder pathPCA, StringBuilder pathSVM, StringBuilder pathEyesSVM);
	
	[DllImport("xzimgAugmentedFace")]
	public static extern int xzimgRigidTracking([In][Out] ref xmgImage imageIn, [In][Out] ref xmgTrackingParams trackingData, [In][Out] ref xmgRigidFaceData rigidData);
	
	[DllImport("xzimgAugmentedFace")]
	public static extern void xzimgSetCalibration(double fovx, int processingWidth, int processingHeight, int idxScreenOrientation, int idxDeviceOrientation);
	
	[DllImport("xzimgAugmentedFace")]
	public static extern void xzimgReleaseRigidTracking();
#elif (UNITY_WEBGL && !UNITY_IOS)
    [DllImport("__Internal")]
    public static extern void xzimgInitializeRigidTracking(StringBuilder pathPCA, StringBuilder pathSVM, StringBuilder pathEyesSVM);
    [DllImport("__Internal")]
    public static extern int xzimgRigidTracking([In][Out] ref xmgImage imageIn, [In][Out] ref xmgTrackingParams trackingData, [In][Out] ref xmgRigidFaceData rigidData);
    [DllImport("__Internal")]
    public static extern void xzimgSetCalibration(double fovx, int processingWidth, int processingHeight, int idxScreenOrientation, int idxDeviceOrientation);
    [DllImport("__Internal")]
    public static extern void xzimgReleaseRigidTracking();
#endif

#if UNITY_IOS
    [DllImport("__Internal")]
    public static extern void xzimgInitializeRigidTracking(StringBuilder pathPCA, StringBuilder pathSVM, StringBuilder pathEyesSVM);
    [DllImport("__Internal")]
    public static extern int xzimgRigidTracking([In][Out] ref xmgImage imageIn, [In][Out] ref xmgTrackingParams trackingData, [In][Out] ref xmgRigidFaceData rigidData);
    [DllImport("__Internal")]
    public static extern void xzimgReleaseRigidTracking();

    [DllImport ("__Internal")] 
	public static extern int xzimgFaceApiInitializeRigidTracking([In][Out] ref xmgVideoCaptureOptions videoOptions);
	
	[DllImport ("__Internal")] 
	public static extern int xzimgFaceApiRigidTracking([In][Out] ref xmgTrackingParams trackingData, [In][Out] ref xmgRigidFaceData rigidData);
	
	[DllImport ("__Internal")] 
	public static extern void xzimgFaceApiReleaseRigidTracking();
	
	[DllImport ("__Internal")] 
	public static extern void xzimgSetCalibration(double fovx, int processingWidth, int processingHeight, int idxScreenOrientation, int idxDeviceOrientation);

#endif


    public static void StartCameraAndInitialize(int cameraMode, bool isFrontal, double fovx, int idxScreenOrientation, bool highQuality)
	{
		
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("StartCameraAndInitialize", cameraMode, isFrontal, fovx, idxScreenOrientation, highQuality);
#endif
    }

    public static float[] xzimgAugmentedFaceDetect(System.IntPtr textureID)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		return jo.Call<float[]>("xzimgAugmentedFaceDetect", textureID.ToInt32());
#else
        return null;
#endif
    }

    public static void SetGLTextureID_RGB(int textureID, int uvTextureID)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("SetGLTextureID_RGB", textureID, uvTextureID);
#endif
    }

    public static void SetNewDeviceOrientation(int idxOrientation)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("SetNewDeviceOrientation", idxOrientation);
#endif
    }

	public static float[] GetPose()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		return jo.Call<float[]>("GetPose");
#else
		return null;
#endif
	}

	public static int GetDetect()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		return jo.Call<int>("GetDetect");
#else
		return -1;
#endif
	}

	public static void Release()
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
		jo.Call("Release");
#endif
	}

}


public class xmgTools
{
    static public float ConvertToRadian(float degreeAngle)
    {
        return (degreeAngle * ((float)Math.PI / 180.0f));
    }
    static public double ConvertToRadian(double degreeAngle)
    {
        return (degreeAngle * (Math.PI / 180.0f));
    }
    static public float ConvertToDegree(float degreeAngle)
    {
        return (degreeAngle * (180.0f / (float)Math.PI));
    }
    static public double ConvertToDegree(double degreeAngle)
    {
        return (degreeAngle * (180.0f / Math.PI));
    }
    static public double ConvertHorizontalFovToVerticalFov(double radianAngle, double aspectRatio)
    {
        return (Math.Atan(1.0 / aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
    }

    static public double ConvertVerticalFovToHorizontalFov(double radianAngle, double aspectRatio)
    {
        return (Math.Atan(aspectRatio * Math.Tan(radianAngle / 2.0)) * 2.0);
    }
}