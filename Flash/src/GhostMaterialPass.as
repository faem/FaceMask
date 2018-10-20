package 
{
	import away3d.arcane;
	import away3d.cameras.Camera3D;
	import away3d.core.managers.Stage3DProxy;
	import away3d.materials.MaterialBase;
	import away3d.materials.methods.BasicDiffuseMethod;
	import away3d.materials.passes.SuperShaderPass;
	
	use namespace arcane;

	public class GhostMaterialPass extends SuperShaderPass 
	{
		public function GhostMaterialPass(material:MaterialBase) 
		{
			super(material);
			diffuseMethod = new BasicDiffuseMethod();
		}

		arcane override function activate(stage3DProxy:Stage3DProxy, camera:Camera3D):void 
		{
			super.activate(stage3DProxy, camera);
			
			stage3DProxy.context3D.setColorMask(false, false, false, false);
		}

		arcane override function deactivate(stage3DProxy : Stage3DProxy):void
		{
			stage3DProxy.context3D.setColorMask(true, true, true, true);

			super.deactivate(stage3DProxy);
		}
	}
}