package  
{
	import flash.media.Camera;
	import flash.media.Video;
	
	public interface IVideoCapture 
	{
		function get camera():Camera;
		function get video():Video;
	}
}