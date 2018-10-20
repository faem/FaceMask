package  
{
	import away3d.textures.BitmapTexture;
	import flash.display.BitmapData;
	import flash.display.Sprite;
	import flash.display3D.textures.TextureBase;
	import flash.events.Event;
	import flash.geom.Matrix;
	import flash.media.Video;

	public class CameraTexture extends BitmapTexture
	{
		public function CameraTexture() 
		{
			super(new BitmapData(1024, 1024, false, 0));
		}
		
		private var _video:Video;
		private var _matrix:Matrix;
		
		public function initialize(videoCapture:IVideoCapture, renderTarget:Sprite):void
		{
			_video = videoCapture.video;
			if (Event['VIDEO_FRAME'] != null)
			{
				videoCapture.camera.addEventListener(Event.VIDEO_FRAME, cameraFrameHandler);
			}
			else
			{
				renderTarget.addEventListener(Event.ENTER_FRAME, cameraFrameHandler);
			}

			_matrix = new Matrix();
			_matrix.scale(1024 / _video.width, 1024 / _video.height);
			
			_matrix.a = -1 * _matrix.a;
			_matrix.a > 0 ? _matrix.tx = _video.x - _video.width * Math.abs(_matrix.a) : _matrix.tx = _video.width * Math.abs(_matrix.a) +  _video.x;
		}
		
		override protected function uploadContent(texture:TextureBase):void 
		{
			super.uploadContent(texture);
		}
		
		private function cameraFrameHandler(event:Event):void
		{
			bitmapData.lock();
			bitmapData.fillRect(bitmapData.rect, 0);
			bitmapData.draw(_video, _matrix, null, null, bitmapData.rect, true);
			bitmapData.unlock();
			invalidateContent();
		}
	}
}