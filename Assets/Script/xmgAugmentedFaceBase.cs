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

public enum xmgOrientationMode
{
    LandscapeLeft,
    Portrait,
    LandscapeRight,
    PortraitUpsideDown,
};

/**
 * This class contains the interface with the plugin for different platforms
 */
public class xmgAugmentedFaceBase : MonoBehaviour
{
    [Tooltip("Use Native Capture or Unity WebCameraTexture class")]
    public bool useNativeCapture = true;

    [Tooltip("Video capture index (choose your camera) for Desktop only")]
    public int videoCaptureIndex = -1;

    [Tooltip("Video capture mode \n 1 is VGA (640x480) \n 2 is 720p \n 3 is 1080p")]
    public int videoCaptureMode = 1;

    [Tooltip("Use frontal camera (for mobiles only)")]
    public bool UseFrontal = false;

    [Tooltip("Mirror the video")]
    public bool MirrorVideo = true;

    [Tooltip("Choose if the video plane should fit  horizontally or vertically the screen (only relevent in case screen aspect ratio is different from video capture aspect ratio)")]
    public xmgVideoPlaneFittingMode videoPlaneFittingMode = xmgVideoPlaneFittingMode.FitHorizontally;

    [Tooltip("Camera horizontal FOV \nThis value will change the main camera vertical FOV")]
    public float CameraFOVX = 60f;

    [Tooltip("Default Orientation of the capture device for PC/Windows ")]
    public xmgOrientationMode captureDeviceOrientation = xmgOrientationMode.LandscapeLeft;

    [Tooltip("Debug data")]
    public bool ScreenDebug = true;

#if UNITY_IOS
	[Tooltip("Orientation Mode for iOS - this is a temporary parameter to prevent a Unity bug - value must correspond to the Default Orientation field in the Player Settings panel)\n")]
	public ScreenOrientation ScreenOrientationIOS = ScreenOrientation.Portrait;


	public int GetScreenOrientationIndex()
	{
		int ret = 3;
		if (ScreenOrientationIOS == ScreenOrientation.LandscapeRight) ret = 0;
		else if (ScreenOrientationIOS == ScreenOrientation.Portrait) ret = 3;
		else if (ScreenOrientationIOS == ScreenOrientation.LandscapeLeft) ret = 2;
		else if (ScreenOrientationIOS == ScreenOrientation.PortraitUpsideDown) ret = 1;
		return ret;
    }
#endif
    protected string m_debugStatus = "";

 
	protected Texture2D imgTexture;
    protected Texture2D uvTexture;
    protected DeviceOrientation currentDeviceOrientation;

    // -------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------


    public void CheckeParameters()
    {
#if (UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL)
        if (useNativeCapture)
            Debug.Log("xmgVideoCaptureParameters (useNativeCapture) - Video Capture cannot be set to native for PC/MAC platforms => forcing to FALSE");
        if (UseFrontal)
            Debug.Log("xmgVideoCaptureParameters (UseFrontal) - Frontal mode option is not available for PC/MAC platforms - Use camera index edit box instead => forcing to FALSE");
        useNativeCapture = false;
        UseFrontal = false;
#endif

#if (!UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS))
    captureDeviceOrientation = xmgOrientationMode.LandscapeLeft;
    if (UseFrontal && !MirrorVideo)
    {
        MirrorVideo = true;
        Debug.Log("xmgVideoCaptureParameters (MirrorVideo) - Mirror mode is forced on mobiles when using frontal camera => forcing to TRUE");       
    }
    if (!UseFrontal && MirrorVideo)
    {
        MirrorVideo = false;
        Debug.Log("xmgVideoCaptureParameters (MirrorVideo) - Mirror mode is deactivate on mobiles when using back camera => forcing to FALSE");       
    }
#endif
    }

    // -------------------------------------------------------------------------------------------------------------

    /**
    * Create a planar mesh and texture coordinates adapted for landscapeRight mode 
    */
    public Mesh createPlanarMesh()
    {
        Vector3[] Vertices = new Vector3[] { new Vector3(-1, 1, 0), new Vector3(1, 1, 0), new Vector3(1, -1, 0), new Vector3(-1, -1, 0) };
        //Vector2[] UV = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1) };
        Vector2[] UV = new Vector2[] { new Vector2(0, 1), new Vector2(1, 1), new Vector2(1, 0), new Vector2(0, 0) };
        int[] Triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        Mesh mesh = new Mesh();
        mesh.vertices = Vertices;
        mesh.triangles = Triangles;
        mesh.uv = UV;
        return mesh;
    }

    // -------------------------------------------------------------------------------------------------------------

    public void UpdateBackgroundPlaneOrientation()
    {
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        if (Screen.orientation == ScreenOrientation.Portrait)
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
		else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
		
#if UNITY_IOS
		if (UseFrontal)
		{
			
			transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
			if (Screen.orientation == ScreenOrientation.Portrait) 
				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);
			else if (Screen.orientation == ScreenOrientation.LandscapeRight)
				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
			else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
				gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
		}
#endif

        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            Camera.main.fieldOfView = (float)GetPortraitMainCameraFovV();

    }

    // -------------------------------------------------------------------------------------------------------------

    public void PrepareBackgroundPlane()
    {
        // Reset camera rotation and position
        Camera.main.transform.position = new Vector3(0, 0, 0);
        Camera.main.transform.rotation = Quaternion.Euler(0, 0, 0);

        // Create a mesh to apply video texture
        Mesh mesh = createPlanarMesh();
        gameObject.AddComponent<MeshFilter>().mesh = mesh;

        // Rotate the mesh according to current screen orientation
        transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        if (Screen.orientation == ScreenOrientation.Portrait)
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 90.0f);
        else if (Screen.orientation == ScreenOrientation.LandscapeLeft)
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 180.0f);
        else if (Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, -90.0f);


        // Prepare ratios and camera fov
        Camera.main.fieldOfView = (float)GetMainCameraFovV();
        if (Screen.orientation == ScreenOrientation.Portrait || Screen.orientation == ScreenOrientation.PortraitUpsideDown)
            Camera.main.fieldOfView = (float)GetPortraitMainCameraFovV();

        // Modify Game Object's position & orientation
        double VideoPlaneDistance = 750;
        gameObject.transform.position = new Vector3(0, 0, (float)VideoPlaneDistance);
        double[] scale = GetVideoPlaneScale(VideoPlaneDistance);

        if (MirrorVideo)
            transform.localScale = new Vector3((float)scale[0], (float)scale[1], (float)1);
        else
            transform.localScale = new Vector3((float)-scale[0], (float)scale[1], (float)1);
        //transform.localScale *= VideoPlaneScale;

        // __ Assign video texture to the renderer
        if (!GetComponent<Renderer>())
            gameObject.AddComponent<MeshRenderer>();
        
        gameObject.GetComponent<Renderer>().material = new Material( Shader.Find("Custom/VideoShader"));

    }

    // -------------------------------------------------------------------------------------------------------------
    /**
    *   Update debug display string
    */
    protected void UpdateDebugDisplay(int iDetected)
    {
        if (iDetected > 0)
        {
            m_debugStatus = "Face Detected";
        }
        else if (iDetected == -11)
            m_debugStatus = "Protection Alert - Please reload the plugin";
        else
            m_debugStatus = "Face Not Detected";
    }


    // -------------------------------------------------------------------------------------------------------------
    // -------------------------------------------------------------------------------------------------------------

    protected int GetVideoCaptureWidth()
    {
        if (videoCaptureMode == 0) return 320;
        if (videoCaptureMode == 2) return 1280;
        if (videoCaptureMode == 3) return 1920;
        return 640;
    }

    // -------------------------------------------------------------------------------------------------------------

    protected int GetVideoCaptureHeight()
    {
        if (videoCaptureMode == 0) return 240;
        if (videoCaptureMode == 2) return 720;
        if (videoCaptureMode == 3) return 1080;
        return 480;
    }

    // -------------------------------------------------------------------------------------------------------------

    protected int GetProcessingWidth()
    {
        if (videoCaptureMode == 0) return 320;
        if (videoCaptureMode == 2) return 640;
        if (videoCaptureMode == 3) return 480;
        return 320;
    }

    // -------------------------------------------------------------------------------------------------------------

    protected int GetProcessingHeight()
    {
        if (videoCaptureMode == 0) return 240;
        if (videoCaptureMode == 2) return 360;
        if (videoCaptureMode == 3) return 270;
        return 240;
    }

    // -------------------------------------------------------------------------------------------------------------

    public int GetProcessingWidth(int videoCaptureWidth)
    {
        if (videoCaptureWidth > 640)
            return videoCaptureWidth / 4;
        else if (videoCaptureWidth > 320)
            return videoCaptureWidth / 2;
        return videoCaptureWidth;
    }

    // -------------------------------------------------------------------------------------------------------------

    public int GetProcessingHeight(int videoCaptureHeight)
    {
        if (videoCaptureHeight > 640)
            return videoCaptureHeight / 4;
        else if (videoCaptureHeight > 320)
            return videoCaptureHeight / 2;
        return videoCaptureHeight;
    }

    // -------------------------------------------------------------------------------------------------------------

    protected double GetVideoAspectRatio()
    {
        return (double)GetVideoCaptureWidth() / (double)GetVideoCaptureHeight();
    }


    // -------------------------------------------------------------------------------------------------------------

    protected float GetScreenAspectRatio()
    {
        float screen_AR = (float)Screen.width / (float)Screen.height;
        if (Screen.width < Screen.height)
            screen_AR = 1.0f / screen_AR;
        return screen_AR;

    }

    // -------------------------------------------------------------------------------------------------------------

    protected double GetMainCameraFovV()
    {
        float video_AR = (float)GetVideoAspectRatio();
        float screen_AR = GetScreenAspectRatio();
        double trackingCamera_fovh_radian = xmgTools.ConvertToRadian((double)CameraFOVX);
        double trackingCamera_fovv_radian;
        if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
            trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)screen_AR);
        else
            trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
        return xmgTools.ConvertToDegree(trackingCamera_fovv_radian);
    }

    // -------------------------------------------------------------------------------------------------------------

    // Usefull for portrait and reverse protraits modes
    protected double GetPortraitMainCameraFovV()
    {
        float video_AR = (float)GetVideoAspectRatio();
        float screen_AR = GetScreenAspectRatio();

        double trackingCamera_fovh_radian = xmgTools.ConvertToRadian((double)CameraFOVX);
        double trackingCamera_fovv_radian;
        if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
            trackingCamera_fovv_radian = trackingCamera_fovh_radian;
        else
        {
            trackingCamera_fovv_radian = xmgTools.ConvertHorizontalFovToVerticalFov(trackingCamera_fovh_radian, (double)video_AR);
            trackingCamera_fovv_radian = xmgTools.ConvertVerticalFovToHorizontalFov(trackingCamera_fovv_radian, (double)screen_AR);
        }

        return xmgTools.ConvertToDegree(trackingCamera_fovv_radian);
    }

    // -------------------------------------------------------------------------------------------------------------


    protected double[] GetVideoPlaneScale(double videoPlaneDistance)
    {
        double[] ret = new double[2];

        float video_AR = (float)GetVideoAspectRatio();
        float screen_AR = GetScreenAspectRatio();
        double scale_u, scale_v;

        if (videoPlaneFittingMode == xmgVideoPlaneFittingMode.FitHorizontally)
        {
            double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)GetMainCameraFovV());
            double mainCamera_fovh_radian = xmgTools.ConvertVerticalFovToHorizontalFov(mainCamera_fovv_radian, (double)screen_AR);
            scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0));
            scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovh_radian / 2.0) * 1.0 / video_AR);
        }
        else
        {
            double mainCamera_fovv_radian = xmgTools.ConvertToRadian((double)GetMainCameraFovV());
            scale_u = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0) * video_AR);
            scale_v = (videoPlaneDistance * Math.Tan(mainCamera_fovv_radian / 2.0));
        }
        ret[0] = scale_u;
        ret[1] = scale_v;
        return ret;
    }

};

