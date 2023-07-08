using StarlightRiver.Content.NPCs.Actors;
using Terraria.ID;

namespace StarlightRiver.Content.Items.Infernal
{
	internal class InfernalCatalyst : QuickMaterial
	{
		public override string Texture => AssetDirectory.Debug;

		public InfernalCatalyst() : base("Infernal Catalyst", "Primes the lavas of hell for transmutation", 9999, 0, ItemRarityID.Orange) { }

		public override void Update(ref float gravity, ref float maxFallSpeed)
		{
			if (Item.lavaWet && Item.Center.Y > (Main.maxTilesY - 200) * 16)
			{
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
	}
}
