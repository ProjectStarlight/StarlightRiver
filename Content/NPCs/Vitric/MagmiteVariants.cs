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

	internal class MagmiteSmol : MagmitePassive, IHintable
	{
		protected override int Offset => 4;
		protected override float Size => 0.8f;
		public override string Texture => AssetDirectory.VitricNpc + "MagmiteSmol";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmini");
			Main.npcCatchable[Type] = true;
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCType<CoolmiteSmol>();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.catchItem = ItemType<MagmiteSmolItem>();
			NPC.width = 16;
			NPC.height = 16;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("A tiny sentient drop of magma freshly born from the pools of molten glass. Harbors endless curiosity towards everything and everyone it comes across, but it doesn't yet realize that its body is hundreds of degrees hotter than most living things can bear.")
			});
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				frameCounter++;
				NPC.frame.X = 32;
				NPC.frame.Y = (int)(frameCounter / 5 % 5) * 28;
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
					NPC.frame.X = 32;
					NPC.frame.Y = (int)(ActionTimer / 5 % 5) * 28;
				}
			}
			else if (ActionState == 1)
			{
				NPC.frame.X = 64;
				NPC.frame.Y = (int)(ActionTimer / 60f * 9) * 28;
			}

			NPC.frame.Width = 32;
			NPC.frame.Height = 28;
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

			int originX = 10;

			if (NPC.spriteDirection == -1)
				originX = 22;

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos, NPC.frame, Color.White * (1 - NPC.shimmerTransparency), 0, new Vector2(originX, 10), 1, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		new public string GetHint()
		{
			return "Smol!";
		}
	}


	internal class MagmiteLarge : MagmitePassive, IHintable
	{
		protected override int Offset => 10;
		protected override float Size => 1.25f;
		public override string Texture => AssetDirectory.VitricNpc + "MagmiteLarge";

		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Magmificent");
			Main.npcCatchable[Type] = true;
			NPCID.Sets.ShimmerTransformToNPC[NPC.type] = NPCType<CoolmiteLarge>();
		}

		public override void SetDefaults()
		{
			base.SetDefaults();
			NPC.catchItem = ItemType<MagmiteLargeItem>();
			NPC.width = 28;
			NPC.height = 28;
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
		{
			bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
			{
				Bestiary.SLRSpawnConditions.VitricDesert,
				new FlavorTextBestiaryInfoElement("A matured magmite, covered with intricate patterns and swirls from the slow cooling of its body. Most creatures dare not approach to admire its magnificence, as it still still brilliantly burns thousands of degrees.")
			});
		}

		public override void FindFrame(int frameHeight)
		{
			if (NPC.IsABestiaryIconDummy)
			{
				frameCounter++;
				NPC.frame.X = 58;
				NPC.frame.Y = (int)(frameCounter / 5 % 5) * 50;
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
					NPC.frame.X = 58;
					NPC.frame.Y = (int)(ActionTimer / 5 % 5) * 50;
				}
			}
			else if (ActionState == 1)
			{
				NPC.frame.X = 116;
				NPC.frame.Y = (int)(ActionTimer / 60f * 9) * 50;
			}

			NPC.frame.Width = 58;
			NPC.frame.Height = 50;
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
				originX = 40;

			spriteBatch.Draw(Request<Texture2D>(Texture).Value, pos, NPC.frame, Color.White * (1 - NPC.shimmerTransparency), 0, new Vector2(originX, 24), 1, NPC.spriteDirection == -1 ? 0 : SpriteEffects.FlipHorizontally, 0);
			return false;
		}

		public override void OnKill()
		{
			int count = Main.rand.Next(0, 4);

			for (int i = 0; i < count; i++)
			{
				NPC magmite = NPC.NewNPCDirect(NPC.GetSource_FromThis(), (int)NPC.position.X, (int)NPC.position.Y, NPCType<MagmiteSmol>(), 0, NPC.ai[0], NPC.ai[1], NPC.ai[2], NPC.ai[3], NPC.target);
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
			return "Magnificent!";
		}
	}

	internal class MagmiteSmolItem : QuickCritterItem
	{
		public MagmiteSmolItem() : base("Magmini", "Nurture him!", Item.sellPrice(silver: 5), ItemRarityID.Orange, NPCType<MagmiteSmol>(), AssetDirectory.VitricItem) { }
	}

	internal class MagmiteLargeItem : QuickCritterItem
	{
		public MagmiteLargeItem() : base("Magmificent", "Admire him!", Item.sellPrice(silver: 25), ItemRarityID.Orange, NPCType<MagmiteLarge>(), AssetDirectory.VitricItem) { }
	}
}