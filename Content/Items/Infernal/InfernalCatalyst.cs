using StarlightRiver.Content.NPCs.Actors;
using System;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Infernal
{
	internal class InfernalCatalyst : QuickMaterial
	{
		bool canTransform = true;

		public override string Texture => "StarlightRiver/Assets/Items/Infernal/InfernalCatalyst";

		public InfernalCatalyst() : base("Infernal Catalyst", "Primes the lavas of hell for transmutation\nRequires a large body of lava\nItems that can be transmuted will glow in your inventory", 9999, 0, ItemRarityID.Orange) { }

		public override void Load()
		{
			StarlightNPC.ModifyNPCLootEvent += DropCatalyst;
		}

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			if (canTransform && Item.lavaWet && Item.Center.Y > (Main.maxTilesY - 200) * 16)
			{
				Tile tile = Framing.GetTileSafely((int)Item.Center.X / 16, (int)Item.Center.Y / 16);

				for (int k = 0; k <= 7; k++)
				{
					Tile check = Framing.GetTileSafely((int)Item.Center.X / 16 + k, (int)Item.Center.Y / 16);

					if (check.HasTile || check.LiquidType != LiquidID.Lava)
						Item.position.X -= (7 - k) * 16;
				}

				for (int k = -7; k <= 0; k++)
				{
					Tile check = Framing.GetTileSafely((int)Item.Center.X / 16 + k, (int)Item.Center.Y / 16);

					if (check.HasTile || check.LiquidType != LiquidID.Lava)
						Item.position.X += (7 - -k) * 16;
				}

				for (int k = -6; k <= 6; k++)
				{
					Tile check = Framing.GetTileSafely((int)Item.Center.X / 16 + k, (int)Item.Center.Y / 16);

					if (check.HasTile || check.LiquidType != LiquidID.Lava)
					{
						canTransform = false;
						return;
					}
				}

				NPC.NewNPC(Item.GetSource_FromThis(), (int)Item.Center.X, (int)Item.Center.Y, ModContent.NPCType<HellLavaActor>());

				for (int k = 0; k < 20; k++)
				{
					float rand = Main.rand.NextFloat();
					Dust.NewDustPerfect(Item.Center + Vector2.UnitX * Main.rand.NextFloat(-100, 100), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * rand * -4, 0, new Color(255, (int)((1 - rand) * 200), 0), Main.rand.NextFloat(1f, 2f));
					Dust.NewDustPerfect(Item.Center + Vector2.UnitX * Main.rand.NextFloat(-100, 100), ModContent.DustType<Dusts.Cinder>(), Vector2.UnitY * rand * -4, 0, new Color(255, (int)((1 - rand) * 200), 0), Main.rand.NextFloat(0.5f, 1.5f));
				}

				Projectile.NewProjectile(Item.GetSource_FromThis(), Item.Center + new Vector2(0, 20), Vector2.Zero, ModContent.ProjectileType<FirePillar>(), 0, 0, Main.myPlayer, 1.5f);

				Helpers.Helper.PlayPitched("Magic/FireHit", 1, -0.5f, Item.Center);
				Helpers.Helper.PlayPitched("Magic/FireHit", 0.5f, 0.5f, Item.Center);

				Item.TurnToAir();
			}
		}

		private void DropCatalyst(NPC npc, NPCLoot npcloot)
		{
			if (npc.type == NPCID.Demon)
				npcloot.Add(ItemDropRule.Common(Item.type, 4, 1, 1));
		}
	}
}