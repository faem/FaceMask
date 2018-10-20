package 
{
	import away3d.cameras.Camera3D;
	import away3d.cameras.lenses.PerspectiveLens;
	import away3d.containers.ObjectContainer3D;
	import away3d.containers.View3D;
	import away3d.debug.AwayStats;
	import away3d.entities.Mesh;
	import away3d.events.AssetEvent;
	import away3d.library.AssetLibrary;
	import away3d.library.assets.AssetType;
	import away3d.loaders.Loader3D;
	import away3d.loaders.parsers.AWDParser;
	import flash.display.Sprite;
	import flash.events.Event;
	import flash.geom.Matrix3D;
	import flash.geom.Vector3D;
	
	public class Main extends Sprite 
	{
		public function Main():void 
		{
			if (stage) init();
			else addEventListener(Event.ADDED_TO_STAGE, init);
		}
		
		private var _videoCapture:VideoCapture;
		private var _faceTracker:FaceTracker;
		private var _view3D:View3D;
		private var _container:ObjectContainer3D;
		private var _cameraTexture:CameraTexture;
		
		[Embed(source='../embed/faceMask.awd', mimeType='application/octet-stream')]
		private var FaceMask:Class;

		[Embed(source='../embed/glass.awd', mimeType='application/octet-stream')]
		private var Glasses:Class;
		
		private function init(e:Event = null):void 
		{
			removeEventListener(Event.ADDED_TO_STAGE, init);
			
			_videoCapture = new VideoCapture();
			_faceTracker = new FaceTracker();
			_cameraTexture = new CameraTexture();
			
			_videoCapture.initialize();
			_faceTracker.initialize(_videoCapture, this);
			_cameraTexture.initialize(_videoCapture, this);
			
			setupScene();
		}

		private function setupScene():void
		{
			_view3D = new View3D();
			addChild(_view3D);
		//	addChild(new AwayStats(_view3D));	// to display statistics
						
			var lens:PerspectiveLens = new PerspectiveLens(41.25 * 480 / 640);
			lens.near = 1;
			_view3D.camera = new Camera3D(lens);			
			_view3D.camera.z = 0;
			_view3D.background = _cameraTexture;
			
			_container = new ObjectContainer3D();
			_view3D.scene.addChild(_container);
						
			Loader3D.enableParser(AWDParser);
			
			var maskLoader:Loader3D = new Loader3D();
			maskLoader.addEventListener(AssetEvent.ASSET_COMPLETE, onMaskComplete);
			maskLoader.loadData(new FaceMask());
		}
		
		private function onMaskComplete(event:AssetEvent):void
		{
			if (event.asset.assetType == AssetType.MESH)
			{
				var m:Mesh = event.asset as Mesh;
				m.material = new GhostMaterial();
				_container.addChild(m);
				m.scale(0.12);
				m.transform.appendTranslation(0, -18, 30);

				var glassesLoader:Loader3D = new Loader3D();
				glassesLoader.addEventListener(AssetEvent.ASSET_COMPLETE, onAssetComplete);
				glassesLoader.loadData(new Glasses());
			}
		}
		
		private function onAssetComplete(event:AssetEvent):void
		{
			if (event.asset.assetType == AssetType.MESH)
			{
				var m:Mesh = event.asset as Mesh;
				_container.addChild(m);

				
				m.scale(0.12);
				m.transform.appendTranslation(0, -18, 30);
			
				addEventListener(Event.ENTER_FRAME, onViewFrameEnter);
				_videoCapture.start();
			}
		}
				
		private function onViewFrameEnter(event:Event):void
		{
			_container.transform = _faceTracker.getFacePose();
			_view3D.render();
		}
	}
}