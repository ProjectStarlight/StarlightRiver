using System.Collections.Generic;
using System.Linq;
using Terraria.Graphics.Effects;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Content.Items.Vitric
{
	class GlassReplica : GlobalItem
	{
		public override bool InstancePerEntity => true;

		public bool isReplica;
		private bool firstTime = true;

		public override GlobalItem Clone(Item item, Item itemClone)
		{
			return item.TryGetGlobalItem<GlassReplica>(out GlassReplica gi) ? gi : base.Clone(item, itemClone);
		}

		public override void SaveData(Item item, TagCompound tag)
		{
			if (item.GetGlobalItem<GlassReplica>().isReplica)
				tag["isReplica"] = item.GetGlobalItem<GlassReplica>().isReplica;
		}

		public override void LoadData(Item item, TagCompound tag)
		{
			item.GetGlobalItem<GlassReplica>().isReplica = tag.GetBool("isReplica");
			item.GetGlobalItem<GlassReplica>().firstTime = false;
		}

		public override bool PreDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (item.GetGlobalItem<GlassReplica>().isReplica)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Filters.Scene["VitricReplicaItem"].GetShader().Shader, Main.UIScaleMatrix);
				Filters.Scene["VitricReplicaItem"].GetShader().Shader.Parameters["uTime"].SetValue(StarlightWorld.visualTimer);
			}

			return true;
		}

		public override void PostDrawInInventory(Item item, SpriteBatch spriteBatch, Vector2 position, Rectangle frame, Color drawColor, Color ItemColor, Vector2 origin, float scale)
		{
			if (item.GetGlobalItem<GlassReplica>().isReplica)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.UIScaleMatrix);
			}
		}

		public override bool PreDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, ref float rotation, ref float scale, int whoAmI)
		{
			if (item.GetGlobalItem<GlassReplica>().isReplica)
			{
				if (item.GetGlobalItem<GlassReplica>().firstTime)
				{
					spriteBatch.End();
					spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);

					Texture2D tex = ModContent.Request<Texture2D>("StarlightRiver/Assets/RiftCrafting/Glow1").Value;
					float scale1 = Terraria.GameContent.TextureAssets.Item[item.type].Size().Length() / tex.Size().Length();
					var color = new Color(180, 240, 255);

					spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, color * 0.5f, StarlightWorld.visualTimer, tex.Size() / 2, 2 * scale1, 0, 0);
					spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, color * 0.3f, -StarlightWorld.visualTimer, tex.Size() / 2, 2.5f * scale1, 0, 0);
					spriteBatch.Draw(tex, item.Center - Main.screenPosition, null, color * 0.8f, StarlightWorld.visualTimer * 2, tex.Size() / 2, 1.2f * scale1, 0, 0);
				}

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, Filters.Scene["VitricReplicaItem"].GetShader().Shader, Main.GameViewMatrix.ZoomMatrix);
				Filters.Scene["VitricReplicaItem"].GetShader().Shader.Parameters["uTime"].SetValue(StarlightWorld.visualTimer);
			}

			return true;
		}

		public override void PostDrawInWorld(Item item, SpriteBatch spriteBatch, Color lightColor, Color alphaColor, float rotation, float scale, int whoAmI)
		{
			if (item.GetGlobalItem<GlassReplica>().isReplica)
			{
				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default, default, Main.GameViewMatrix.ZoomMatrix);
			}
		}

		public override void ModifyTooltips(Item item, List<TooltipLine> tooltips)
		{
			if (item.GetGlobalItem<GlassReplica>().isReplica)
				tooltips.FirstOrDefault(n => n.Name == "ItemName" && n.Mod == "Terraria").Text = "Replica " + item.Name;
		}

		public override bool OnPickup(Item item, Player Player)
		{
			item.GetGlobalItem<GlassReplica>().firstTime = false;
			return true;
		}
	}
}