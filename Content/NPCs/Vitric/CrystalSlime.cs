using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Dusts;
using StarlightRiver.Helpers;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{
	internal class CrystalSlime : ModNPC
	{
		public override string Texture => AssetDirectory.VitricNpc + "CrystalSlime";

		public ref float Shield => ref NPC.ai[1];

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Crystal Slime");
			Main.npcFrameCount[NPC.type] = 2;
		}

		public override void SetDefaults()
		{
			NPC.width = 48;
			NPC.height = 32;
			NPC.damage = 10;
			NPC.defense = 5;
			NPC.lifeMax = 25;
			NPC.HitSound = SoundID.NPCHit42;
			NPC.DeathSound = SoundID.NPCDeath1;
			NPC.value = 10f;
			NPC.knockBackResist = 0.6f;
			NPC.aiStyle = 1;
			NPC.immortal = true;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("[PH] Entry")
			});
		}

		public override Color? GetAlpha(Color drawColor)
		{
			return Lighting.GetColor((int)NPC.position.X / 16, (int)NPC.position.Y / 16) * 0.75f;
		}

		public override void AI()
		{
			NPC.TargetClosest(true);
			Player Player = Main.player[NPC.target];
			AbilityHandler mp = Player.GetHandler();

			if (AbilityHelper.CheckDash(Player, NPC.Hitbox) && Shield == 1)
			{
				Shield = 0;
				NPC.velocity += Player.velocity * 0.5f;

				mp.ActiveAbility?.Deactivate();
				Player.velocity = Vector2.Normalize(Player.velocity) * -10f;

				Player.immune = true;
				Player.immuneTime = 10;

				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, NPC.Center);

				for (int k = 0; k <= 20; k++)
				{
					Dust.NewDust(NPC.position, 48, 32, DustType<GlassGravity>(), Main.rand.Next(-3, 2), -3, 0, default, 1.7f);
				}

				NPC.netUpdate = true;
			}

			if (Shield == 1)
			{
				NPC.immortal = true;
				NPC.HitSound = SoundID.NPCHit42;

				if (Main.rand.NextBool(30))
				{
					if (Main.rand.NextBool())
						Dust.NewDust(NPC.position, NPC.width, NPC.height, DustType<CrystalSparkle>(), 0, 0);
					else
						Dust.NewDust(NPC.position, NPC.width, NPC.height, DustType<CrystalSparkle2>(), 0, 0);
				}
			}
			else
			{
				NPC.immortal = false;
				NPC.HitSound = SoundID.NPCHit1;
			}
		}

		public override bool StrikeNPC(ref double damage, int defense, ref float knockback, int hitDirection, ref bool crit)
		{
			if (Shield == 1)
				damage = 0;

			return base.StrikeNPC(ref damage, defense, ref knockback, hitDirection, ref crit);
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit)
		{
			if (AbilityHelper.CheckDash(target, NPC.Hitbox))
			{
				target.immune = true;
				target.immuneTime = 5;
			}
		}

		public override float SpawnChance(NPCSpawnInfo spawnInfo)
		{
			return 0;
		}

		public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			if (Shield == 1)
			{
				Color color = Helper.IndicatorColor;
				spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/NPCs/Vitric/Crystal").Value, NPC.position - screenPos + new Vector2(-2, -5), Lighting.GetColor((int)NPC.position.X / 16, (int)NPC.position.Y / 16));
				spriteBatch.Draw(Request<Texture2D>("StarlightRiver/Assets/NPCs/Vitric/CrystalGlow").Value, NPC.position - screenPos + new Vector2(-3, -6), color);
			}
		}
	}

	/* TODO: Figure out why banners make the game melt
>>>>>>> master
    internal class CrystalSlimeBanner : ModBanner
    {
        public CrystalSlimeBanner() : base("CrystalSlimeBannerItem", NPCType<CrystalSlime>(), AssetDirectory.VitricNpc) { }
    }

    internal class CrystalSlimeBannerItem : QuickBannerItem
    {
        public CrystalSlimeBannerItem() : base("CrystalSlimeBanner", "Crystal Slime", AssetDirectory.VitricNpc) { }
    }*/
}