using StarlightRiver.Content.Items.BaseTypes;
using StarlightRiver.Core.Loaders.UILoading;
using System.Collections.Generic;
using Terraria.Audio;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.UI;

namespace StarlightRiver.Content.Items.Misc
{
	public class RhythmicResonator : SmartAccessory
	{
		public bool equipped;
		public int RhythmTimer;
		public int MaxRhythmTimer; //just used visually for the UI
		public int RhythmStacks;
		public int ResetTimer;
		public int flashTimer; // also for UI
		public bool bufferedInput;

		public override string Texture => AssetDirectory.MiscItem + Name;

		public RhythmicResonator() : base("Rhythmic Resonator", "Attack in rhythm with your weapon to gradually increase damage and knockback\nDisables autoswing") { }

		public override void Load()
		{
			StarlightItem.CanAutoReuseItemEvent += PreventAutoReuse;
			StarlightItem.UseItemEvent += UseItemEffects;

			StarlightPlayer.ModifyHitNPCEvent += BoostDamage;
			StarlightPlayer.ModifyHitNPCWithProjEvent += BoostDamageProj;

			StarlightPlayer.ResetEffectsEvent += ResetEffects;
		}

		public override void Unload()
		{
			StarlightItem.CanAutoReuseItemEvent -= PreventAutoReuse;
			StarlightItem.UseItemEvent -= UseItemEffects;
		}

		public override void SafeSetDefaults()
		{
			Item.value = Item.sellPrice(gold: 1);
			Item.rare = ItemRarityID.Green;
		}

		private bool? PreventAutoReuse(Item Item, Player Player)
		{
			if (Equipped(Player))
				return false;

			return null;
		}

		private bool? UseItemEffects(Item Item, Player Player)
		{
			if (Equipped(Player))
			{
				var instance = GetEquippedInstance(Player) as RhythmicResonator;

				// five frame window plus buffer
				if (instance.RhythmTimer > -2 && instance.RhythmTimer < 3 || instance.bufferedInput)
				{
					instance.bufferedInput = false;
					instance.flashTimer = 20;
					instance.RhythmStacks++;

					//set the reset timer for the stacks to triple the items use time, clamped at 3 seconds
					instance.ResetTimer = Utils.Clamp((int)(Item.useTime * (1f - (Player.GetTotalAttackSpeed(Item.DamageType) - 1f)) * 2f), 30, 180);
					SoundEngine.PlaySound(SoundID.MenuTick with { Volume = 1.15f, Pitch = -0.2f, PitchVariance = 0.15f }, Player.Center);

					//only spawn dust on client
					if (Main.myPlayer == Player.whoAmI)
					{
						for (int i = 0; i < 15; i++)
						{
							Vector2 pos = Main.MouseWorld + Vector2.One * 10 + Main.rand.NextVector2Circular(2.5f, 2.5f);
							Dust.NewDustPerfect(pos, ModContent.DustType<Dusts.GlowFastDecelerate>(), Main.rand.NextVector2Circular(1.75f, 1.75f), 0, Color.White, 0.7f);
						}
					}
				}

				//one tick of leeway
				float speed = CombinedHooks.TotalUseTime(Item.useTime, Player, Item) + 1;
				instance.RhythmTimer = (int)speed;
				instance.MaxRhythmTimer = (int)speed;
			}

			return null;
		}

		public void BoostDamage(Player player, Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
		{
			var instance = GetEquippedInstance(player) as RhythmicResonator;

			if (Equipped(player) && instance.RhythmStacks > 0)
			{
				damage = (int)(damage * (1f + 0.05f * instance.RhythmStacks)); //5% increase up to 25%
				knockback *= 1f + 0.1f * instance.RhythmStacks; // 10% yadada
			}
		}

		public void BoostDamageProj(Player player, Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
		{
			var instance = GetEquippedInstance(player) as RhythmicResonator;

			if (Equipped(player) && instance.RhythmStacks > 0)
			{
				damage = (int)(damage * (1f + 0.05f * instance.RhythmStacks));
				knockback *= 1f + 0.1f * instance.RhythmStacks;
			}
		}

		private void ResetEffects(StarlightPlayer Player)
		{
			if (Equipped(Player.Player))
			{
				var instance = GetEquippedInstance(Player.Player) as RhythmicResonator;

				instance.RhythmStacks = Utils.Clamp(instance.RhythmStacks, 0, 5);

				if (instance.ResetTimer > 0)
					instance.ResetTimer--;
				else
					instance.RhythmStacks = 0;

				if (instance.RhythmTimer > -2)
					instance.RhythmTimer--;
				else
					instance.bufferedInput = false;

				if (instance.flashTimer > 0)
					instance.flashTimer--;
			}
		}

		public override void UpdateAccessory(Player player, bool hideVisual)
		{
			if (Main.mouseLeft && Main.mouseLeftRelease && RhythmTimer > -2 && RhythmTimer < 3)
				bufferedInput = true;
		}
	}

	public class RhythmicResonatorUIState : SmartUIState
	{
		public static RhythmicResonator Instance => SmartAccessory.GetEquippedInstance(Main.LocalPlayer, ModContent.ItemType<RhythmicResonator>()) as RhythmicResonator;

		public override bool Visible => !Main.playerInventory && Instance != null &&
			(!Main.player[Main.myPlayer].dead && !Main.player[Main.myPlayer].ghost && !Main.gameMenu || !PlayerInput.InvisibleGamepadInMenus);

		public override int InsertionIndex(List<GameInterfaceLayer> layers)
		{
			return layers.FindIndex(n => n.Name == "Vanilla: Mouse Text");
		}

		public override void Draw(SpriteBatch spriteBatch)
		{
			Texture2D tex = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUIBig").Value;
			Texture2D texOutline = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUIBig_Outline").Value;

			Texture2D texSmall = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUISmall").Value;
			Texture2D texSmallOutline = ModContent.Request<Texture2D>(AssetDirectory.GUI + "RhythmicResonatorUISmall_Outline").Value;

			Vector2 mouse = Main.MouseWorld;

			Color border = Main.MouseBorderColor;
			Color inside = Main.cursorColor;

			if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
				inside = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f, byte.MaxValue);

			if (Instance.flashTimer > 0)
			{
				Color color = Main.cursorColor;

				if (!Main.gameMenu && Main.LocalPlayer.hasRainbowCursor)
					color = Main.hslToRgb(Main.GlobalTimeWrappedHourly * 0.25f % 1f, 1f, 0.5f, byte.MaxValue);

				border = Color.Lerp(Color.White, Main.MouseBorderColor, 1f - Instance.flashTimer / 20f);
				inside = Color.Lerp(Color.White, color, 1f - Instance.flashTimer / 20f);
			}

			if (Instance.RhythmTimer > 0)
			{
				float progress = 1f - Instance.RhythmTimer / (float)Instance.MaxRhythmTimer;
				float alpha = MathHelper.Lerp(0f, 1f, progress * 2);

				int start = Utils.Clamp(Instance.MaxRhythmTimer * 3, 30, 90);
				var offset = Vector2.Lerp(new Vector2(-start, 2), new Vector2(6, 2), 1f - Instance.RhythmTimer / (float)Instance.MaxRhythmTimer);

				Main.spriteBatch.Draw(texSmallOutline, mouse + Vector2.One * 9 + offset - Main.screenPosition, null, border * alpha, 0f, texOutline.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
				Main.spriteBatch.Draw(texSmall, mouse + Vector2.One * 9 + offset - Main.screenPosition, null, inside * alpha, 0f, tex.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);

				offset = Vector2.Lerp(new Vector2(start + 46, 2), new Vector2(42, 2), 1f - Instance.RhythmTimer / (float)Instance.MaxRhythmTimer);

				Main.spriteBatch.Draw(texSmallOutline, mouse + Vector2.One * 9 + offset - Main.screenPosition, null, border * alpha, 0f, texOutline.Size() / 2f, Main.cursorScale, SpriteEffects.FlipHorizontally, 0f);
				Main.spriteBatch.Draw(texSmall, mouse + Vector2.One * 9 + offset - Main.screenPosition, null, inside * alpha, 0f, tex.Size() / 2f, Main.cursorScale, SpriteEffects.FlipHorizontally, 0f);
			}

			Main.spriteBatch.Draw(texOutline, mouse + Vector2.One * 9 - Main.screenPosition, null, border, 0f, texOutline.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
			Main.spriteBatch.Draw(tex, mouse + Vector2.One * 9 - Main.screenPosition, null, inside, 0f, tex.Size() / 2f, Main.cursorScale, SpriteEffects.None, 0f);
		}
	}
}
