using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Content.Items.BuriedArtifacts;
using StarlightRiver.Core.Systems.InoculationSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StarlightRiver.Content.Items.Hell
{
	internal class Misery : CursedAccessory
	{
		private List<NPC> seenNpcs = new();
		private int lastPlayerDefense;
		private float lastPlayerInnoc;

		public override string Texture => AssetDirectory.HellItem + Name;

		public override void Load()
		{
			StarlightPlayer.PostUpdateEquipsEvent += RecordStats;
			StarlightPlayer.PreDrawEvent += DrawAura;
		}

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Misery's Company");
			Tooltip.SetDefault("Nearby enemies have defense and inoculation equal to yours");
		}

		private void RecordStats(StarlightPlayer player)
		{
			if (Equipped(player.Player))
			{
				(GetEquippedInstance(player.Player) as Misery).lastPlayerDefense = player.Player.statDefense;
				(GetEquippedInstance(player.Player) as Misery).lastPlayerInnoc = player.Player.GetModPlayer<InoculationPlayer>().DoTResist;
			}
		}

		public override void SafeUpdateAccessory(Player Player, bool hideVisual)
		{
			foreach(NPC npc in Main.ActiveNPCs)
			{
				if (!npc.friendly && Vector2.Distance(npc.Center, Player.Center) <= 300)
				{
					if (!seenNpcs.Contains(npc))
						seenNpcs.Add(npc);

					npc.defense = lastPlayerDefense;
					npc.GetGlobalNPC<InoculationNPC>().DoTResist = lastPlayerInnoc;
				}
				else if (seenNpcs.Contains(npc))
				{
					npc.defense = npc.defDefense;
					seenNpcs.Remove(npc);
				}
			}

			seenNpcs.RemoveAll(n => n is null || !n.active);
		}

		private void DrawAura(Player player, SpriteBatch spriteBatch)
		{
			if (Equipped(player))
			{
				var tex = Assets.Items.Hell.ChainRing.Value;
				spriteBatch.Draw(tex, player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition, null, new Color(200, 200, 255, 0) * 0.2f, Main.GameUpdateCount * 0.002f, tex.Size() / 2f, 620f / tex.Width, 0, 0);

				var glow = Assets.Keys.GlowAlpha.Value;
				spriteBatch.Draw(glow, player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition, null, new Color(150, 50, 255, 0) * 0.1f, 0f, glow.Size() / 2f, 800f / glow.Width, 0, 0);

				var tex2 = Assets.Misc.DotTell.Value;

				for (int k = 0; k < 6; k++)
				{
					float rot = (k / 5f * 3.14f - Main.GameUpdateCount / 480f * 3.14f) % 3.14f;
					Vector2 pos = player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition + Vector2.UnitY.RotatedBy(rot) * -285;
					spriteBatch.Draw(tex2, pos, null, new Color(150, 50, 255, 0) * 0.3f * (float)Math.Sin(-rot), 0f, tex2.Size() / 2f, 60f / tex2.Width, 0, 0);

					rot = (k / 5f * 3.14f + (Main.GameUpdateCount + 160) / 480f * 3.14f) % 3.14f;
					pos = player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition + Vector2.UnitY.RotatedBy(rot) * -285;
					spriteBatch.Draw(tex2, pos, null, new Color(150, 50, 255, 0) * 0.3f * (float)Math.Sin(rot), 0f, tex2.Size() / 2f, 60f / tex2.Width, 0, 0);
				}

				for (int k = 0; k < 9; k++)
				{
					float rot = (k / 8f * 3.14f - Main.GameUpdateCount / 1020f * 3.14f) % 3.14f;
					Vector2 pos = player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition + Vector2.UnitY.RotatedBy(rot) * -285;
					spriteBatch.Draw(tex2, pos, null, new Color(150, 50, 255, 0) * 0.25f * (float)Math.Sin(-rot), 0f, tex2.Size() / 2f, 40f / tex2.Width, 0, 0);

					rot = (k / 8f * 3.14f + (Main.GameUpdateCount + 560) / 1020f * 3.14f) % 3.14f;
					pos = player.Center + Vector2.UnitY * player.gfxOffY - Main.screenPosition + Vector2.UnitY.RotatedBy(rot) * -285;
					spriteBatch.Draw(tex2, pos, null, new Color(150, 50, 255, 0) * 0.25f * (float)Math.Sin(rot), 0f, tex2.Size() / 2f, 40f / tex2.Width, 0, 0);
				}
			}
		}
	}
}
