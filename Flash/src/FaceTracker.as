package  
{
	import flash.display.BitmapData;
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.geom.Matrix;
	import flash.geom.Matrix3D;
	import flash.media.Camera;
	import flash.media.Video;
	import flash.utils.ByteArray;
	import stampeo.face.*;

	public class FaceTracker
	{		
		public function FaceTracker() 
		{
		}
		
		private var m_pFramePtr:uint;
		private var m_pFaceRectangle:uint;
		private var m_pRightEyePosition:uint;
		private var m_pLeftEyePosition:uint;
		private var m_pMouthPosition:uint;
		private var m_pRotTransMatrix:uint;

		private var imageVideo:BitmapData = new BitmapData(640,480); 		
		private static var m_imageVideoPixels:ByteArray = null; 
		
		private var _camera:Camera;
		private var _video:Video;
		private var _frameGrabber:Function;
		private var _facePose:Matrix3D;
		private var _isTracked:Boolean;

		public function initialize(videoCapture:IVideoCapture, renderTarget:Sprite):void 
		{
			_camera = videoCapture.camera;
			_video = videoCapture.video;
			_facePose = new Matrix3D();
			
			CModule.startAsync(this);
			
			var capWidth:uint = 640;
			var capHeight:uint = 480;
			dtpInitializeTracker(850.*capWidth/640, 850.*capHeight/480, 320.*capWidth/640, 240.*capHeight/480, capWidth, capHeight);
			m_pFramePtr = dtpPtrGetFrame();
			m_pRotTransMatrix = dtpGetRotTransMatrix();
			
			if (Event['VIDEO_FRAME'] != null)
			{
				_camera.addEventListener(Event.VIDEO_FRAME, onVideoFrame);
			}
			else
			{
				renderTarget.addEventListener(Event.ENTER_FRAME, onVideoFrame);
			}
			
			var funcCheck:Boolean;
			try
			{
				funcCheck = _camera['drawToBitmapData'] != null;
				if (funcCheck)
				{
					_frameGrabber = grabFrameFromCamera;
				}
				else
				{
					_frameGrabber = grabFrameFromVideo;
				}
			}
			catch (error:Error)
			{
				_frameGrabber = grabFrameFromVideo;
			}
		}
		
		public function getFacePose():Matrix3D 
		{
			return _facePose;
		}
		public function getFaceTracked():Boolean 
		{
			return _isTracked;
		}

		private function grabFrameFromCamera():void
		{
			_camera.drawToBitmapData(imageVideo);
		}
		
		private function grabFrameFromVideo():void
		{
			imageVideo.draw(_video);
		}
		
		private function onVideoFrame(event:Event):void
		{
			_frameGrabber();
			m_imageVideoPixels = imageVideo.getPixels(imageVideo.rect);
			m_imageVideoPixels.position = 0;
			CModule.writeBytes(m_pFramePtr, m_imageVideoPixels.length, m_imageVideoPixels);
			
			var iFaceTracked:int = dtpProcessTracking(true, false, false);
						 
			if (iFaceTracked > 0)
			{
				var matRotTrans:Vector.<Number> = new Vector.<Number>(16);
				for (var i:int = 0; i < 16; i++) 
				{ 
					matRotTrans[i] = CModule.readDouble(m_pRotTransMatrix + 8 * i);
				}
				_facePose.rawData = matRotTrans;
			}			
			else
			{
				 _facePose.identity();
			}
		}		
	}
}