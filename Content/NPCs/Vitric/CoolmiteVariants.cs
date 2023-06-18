using StarlightRiver.Content.Abilities;
using System;
using System.IO;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.NPCs.Vitric
{

	internal class CoolmiteSmol : CoolmitePassive, IHintable
	{
		protected override int Offset => 4;
		protected override float Size => 0.8f;
		public override int MagmaTransformToNPC => NPCType<MagmiteSmol>();
		public override string Texture => AssetDirectory.VitricNpc + "CoolmiteSmol";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coolmini");
			Main.npcCatchable[Type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.catchItem = ItemType<CoolmiteSmolItem>();
			NPC.width = 16;
			NPC.height = 16;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("Through the power of Shimmer, this form of Magmite has been cooled into a crystalized shard of sentient magma. Even though this leaves the magmite harmless to other creatures, this rigid form actually hinders its endless curiosity as it can no longer fit itself into small spaces.")
			});
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				frameCounter++;
				NPC.frame.X = 36;
				NPC.frame.Y = (int)(frameCounter / 5 % 5) * 36;
			}
			else if (ActionState == 0)
			{
				if (NPC.velocity.Y != 0)
				{
					NPC.frame.X = 0;
					NPC.frame.Y = 0;
				}
				else
				{
					NPC.frame.X = 36;
					NPC.frame.Y = (int)(ActionTimer / 5 % 5) * 36;
				}
			}
			else if (ActionState == 1)
			{
				NPC.frame.X = 72;
				NPC.frame.Y = (int)(ActionTimer / 60f * 9) * 36;
			}

			NPC.frame.Width = 36;
			NPC.frame.Height = 36;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 pos = NPC.Center - screenPos + new Vector2(0, -12);

			if (ActionState == 1)
			{
				pos += new Vector2(8 * NPC.spriteDirection, -4);

				if (NPC.spriteDirection == -1)
					pos.X += 4;
			}

			int originX = 12;

			if (NPC.spriteDirection == -1)
				originX = 24;

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos, NPC.frame, Color.White * (1 - NPC.shimmerTransparency), 0, new Vector2(originX, 16), 1, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		new public string GetHint()
		{
			return "Even smoller in crystal!";
		}
	}

	internal class CoolmiteLarge : CoolmitePassive, IHintable
	{
		protected override int Offset => 10;
		protected override float Size => 1.2f;
		public override int MagmaTransformToNPC => NPCType<MagmiteLarge>();
		public override string Texture => AssetDirectory.VitricNpc + "CoolmiteLarge";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Coolmismer");
			Main.npcCatchable[Type] = true;
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.catchItem = ItemType<CoolmiteLargeItem>();
			NPC.width = 28;
			NPC.height = 28;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("Even through the power of Shimmer, this form of Magmite is so hot that its heat is partially retained. Although its rotund crystal body remains mesmerizing to all that behold it, it seems quite perturbed that it has lost its magnificent swirls.")
			});
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				frameCounter++;
				NPC.frame.X = 60;
				NPC.frame.Y = (int)(frameCounter / 5 % 5) * 54;
			}
			else if (ActionState == 0)
			{
				if (NPC.velocity.Y != 0)
				{
					NPC.frame.X = 0;
					NPC.frame.Y = 0;
				}
				else
				{
					NPC.frame.X = 60;
					NPC.frame.Y = (int)(ActionTimer / 5 % 5) * 54;
				}
			}
			else if (ActionState == 1)
			{
				NPC.frame.X = 120;
				NPC.frame.Y = (int)(ActionTimer / 60f * 9) * 54;
			}

			NPC.frame.Width = 60;
			NPC.frame.Height = 54;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
		{
			Vector2 pos = NPC.Center - screenPos + new Vector2(0, -12);

			if (ActionState == 1)
			{
				pos += new Vector2(8 * NPC.spriteDirection, -4);

				if (NPC.spriteDirection == -1)
					pos.X += 4;
			}

			int originX = 18;

			if (NPC.spriteDirection == -1)
				originX = 42;

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos, NPC.frame, Color.White * (1 - NPC.shimmerTransparency), 0, new Vector2(originX, 28), 1, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		public override void OnKill()
		{
			int count = Main.rand.Next(0, 4);

			for (int i = 0; i < count; i++)
			{
				NPC magmite = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.position.X, (int)NPC.position.Y, NPCType<CoolmiteSmol>(), 0, NPC.ai[0], NPC.ai[1], NPC.ai[2], NPC.ai[3], NPC.target);
				magmite.velocity.X = Main.rand.NextFloat(-2, 2);
				magmite.velocity.Y = -4;
				if (magmite.ModNPC is MagmitePassive modMagmite)
				{
					modMagmite.maxLifeTime = 600;
					modMagmite.Lifetime = 0;
				}
			}

			base.OnKill();
		}

		new public string GetHint()
		{
			return "Even more mesmerizing in crystal!";
		}
	}

	internal class CoolmiteSmolItem : QuickCritterItem
	{
		public CoolmiteSmolItem() : base("Coolmini", "Sharp edges! Watch your fingers.", Item.sellPrice(silver: 5), ItemRarityID.Orange, NPCType<CoolmiteSmol>(), AssetDirectory.VitricItem) { }
	}

	internal class CoolmiteLargeItem : QuickCritterItem
	{
		public CoolmiteLargeItem() : base("Coolmismer", "High intensity beauty! Avoid eye and skin exposure.", Item.sellPrice(silver: 25), ItemRarityID.Orange, NPCType<CoolmiteLarge>(), AssetDirectory.VitricItem) { }
	}
}