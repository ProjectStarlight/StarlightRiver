using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Loaders;

namespace StarlightRiver.Content.CustomHooks
{
	class InventoryParticleDrawing : HookGroup
	{
		//Just drawing some ParticleSystems over the inventory UI. Nothing bad.
		public override void Load()
		{
			if (Main.dedServ)
				return;

			On_Main.DrawInterface_27_Inventory += DrawInventoryParticles;
		}

		private void DrawInventoryParticles(On_Main.orig_DrawInterface_27_Inventory orig, Main self)
		{
			orig(self);
			CursedAccessorySystem.CursedSystem.DrawParticles(Main.spriteBatch);
			CursedAccessorySystem.ShardsSystem.DrawParticles(Main.spriteBatch);
		}
	}
}