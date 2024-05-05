﻿using StarlightRiver.Content.Abilities;
using StarlightRiver.Content.Items.Misc;
using StarlightRiver.Core.Systems;
using StarlightRiver.Core.Systems.DummyTileSystem;
using StarlightRiver.Helpers;
using Terraria.DataStructures;
using Terraria.ID;
using static Terraria.ModLoader.ModContent;

namespace StarlightRiver.Content.Tiles.UndergroundTemple
{
	class JarTall : DummyTile, IHintable
	{
		public override int DummyType => DummySystem.DummyType<JarDummy>();

		public override string Texture => AssetDirectory.UndergroundTempleTile + Name;

		public override void SetStaticDefaults()
		{
			this.QuickSetFurniture(2, 4, DustID.Glass, SoundID.Shatter, false, new Color(91, 211, 233), false, false, "Stamina Jar");
			MinPick = int.MaxValue;
			TileID.Sets.PreventsTileRemovalIfOnTopOfIt[Type] = true;
		}

		public override bool CanExplode(int i, int j)
		{
			return false;
		}

		public override void MouseOver(int i, int j)
		{
			Player Player = Main.LocalPlayer;
			Player.cursorItemIconID = ModContent.ItemType<Items.Hovers.WindsHover>();
			Player.noThrow = 2;
			Player.cursorItemIconEnabled = true;
		}

		public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
		{
			Lighting.AddLight(new Vector2(i, j) * 16, new Vector3(0.9f, 2.1f, 2.3f) * 0.3f);
		}

		public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
		{
			if (Main.tile[i, j].TileFrameX == 0 && Main.tile[i, j].TileFrameY == 0)
			{
				Dummy dummy = Dummy(i, j);

				if (dummy is null)
					return;

				Texture2D tex = Assets.Tiles.UndergroundTemple.JarTallGlow.Value;
				Texture2D tex2 = Assets.Tiles.UndergroundTemple.JarTallGlow2.Value;

				spriteBatch.End();
				spriteBatch.Begin(default, BlendState.Additive, SamplerState.PointClamp, default, default);

				spriteBatch.End();
				spriteBatch.Begin(default, default, SamplerState.PointClamp, default, default);

				spriteBatch.Draw(tex, (Helper.TileAdj + new Vector2(i, j)) * 16 - Main.screenPosition, Color.White);
				spriteBatch.Draw(tex2, (Helper.TileAdj + new Vector2(i, j)) * 16 + new Vector2(-2, 0) - Main.screenPosition, Helper.IndicatorColorProximity(150, 300, dummy.Center));

			}
		}

		public override bool CanDrop(int i, int j)
		{
			return false;
		}

		public string GetHint()
		{
			return "A huge vial of pure starlight -- It's reinforced the glass itself over the centuries. Maybe a powerful starlight force could shatter it.";
		}
	}

	internal class JarDummy : Dummy, IDrawAdditive
	{
		public override bool DoesCollision => true;

		public JarDummy() : base(TileType<JarTall>(), 32, 64) { }

		public override void Update()
		{
			if (Main.rand.NextBool(15))
			{
				Vector2 pos = position + new Vector2(Main.rand.Next(width), Main.rand.Next(height));
				pos += Vector2.UnitY * 8;

				if (Main.rand.NextBool(4))
				{
					var d = Dust.NewDustPerfect(pos, DustType<Dusts.Aurora>(), new Vector2(0, -Main.rand.NextFloat()), 0, new Color(91, 211, 233), 1);
					d.customData = Main.rand.NextFloat(0.7f, 1.1f);
				}
				else
				{
					Dust.NewDustPerfect(pos, DustType<Dusts.Cinder>(), new Vector2(0, -Main.rand.NextFloat()), 0, new Color(91, 151, 233), 1);
				}
			}
		}

		public override void Collision(Player Player)
		{
			if (AbilityHelper.CheckDash(Player, Hitbox))
			{
				WorldGen.KillTile(ParentX, ParentY);
				NetMessage.SendTileSquare(Player.whoAmI, (int)(position.X / 16f), (int)(position.Y / 16f), 2, 4, TileChangeType.None);

				Item.NewItem(null, Hitbox, ModContent.ItemType<StaminaGel>(), Main.rand.Next(3, 12));
				Terraria.Audio.SoundEngine.PlaySound(SoundID.Shatter, Center);
			}
		}

		public void DrawAdditive(SpriteBatch spriteBatch)
		{
			Texture2D tex = Assets.Keys.Glow.Value;
			spriteBatch.Draw(tex, Center - Main.screenPosition + Vector2.UnitY * 16, tex.Frame(), new Color(91, 211, 233) * 0.7f, 0, tex.Size() / 2, 0.8f, 0, 0);
		}
	}

	[SLRDebug]
	public class JarTallItem : QuickTileItem
	{
		public override string Texture => AssetDirectory.Debug;

		public JarTallItem() : base("Stamina Jar Placer (Tall)", "{{Debug}} Item", "JarTall", -12) { }
	}
}