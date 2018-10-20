package
{
	import away3d.arcane;
	import away3d.materials.SinglePassMaterialBase;
	
	use namespace arcane;

	public class GhostMaterial extends SinglePassMaterialBase
	{
		public function GhostMaterial() 
		{
			super();
			_screenPass = new GhostMaterialPass(this);
			_uniqueId = 0;
			_renderOrderId = 0;
			alphaPremultiplied = false;
		}
	}
}