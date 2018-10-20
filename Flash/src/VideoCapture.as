package  
{
	import flash.events.Event;
	import flash.events.EventDispatcher;
	import flash.media.*;

	public class VideoCapture implements IVideoCapture
	{
		public function VideoCapture()
		{
		}
		
		private var _camera:Camera;
		public function get camera():Camera { return _camera; }
		
		private var _video:Video;
		public function get video():Video { return _video; }
		
		public function initialize(width:Number = 640, height:Number = 480):void
		{
			_camera = Camera.getCamera();
			_camera.setMode(width, height, 30, false);
			
			_video = new Video(width, height);
		}
		
		public function start():void
		{
			_video.attachCamera(_camera);
		}
		
		public function stop():void
		{
			_video.attachCamera(null);
		}
	}
}
