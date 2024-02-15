using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.DataStructures;
using Terraria.ModLoader.IO;

namespace StarlightRiver.Core.Systems.PlayableCharacterSystem
{
	internal struct PlayableCharacterSlot
	{
		public PlayableCharacter playableCharacter;
		public Player player;		
	}

	/// <summary>
	/// This is the ModPlayer which manages the PlayableCharacter system.
	/// </summary>
	internal class PlayableCharacterPlayer : ModPlayer
	{
		/// <summary>
		/// If the player this is attached to is the 'main' player character or an alternative player character.
		/// </summary>
		public bool isMainCharacter;

		/// <summary>
		/// If the player is not the main character, this is the main character they are tied to.
		/// </summary>
		public Player owner;

		/// <summary>
		/// If the player is not the main character, this is the PlayableCharacter instance they are controlling.
		/// </summary>
		public PlayableCharacter playingAs;

		/// <summary>
		/// If the player IS the main character, this holds the save data of their alternative characters.
		/// </summary>
		public List<TagCompound> altData;

		/// <summary>
		/// If the player IS the main character, this holds instances for all of their alternative characters.
		/// note, you probably shouldn't ever access this directly. Use the Character property instead!
		/// </summary>
		private Dictionary<Type, PlayableCharacterSlot> characters;

		/// <summary>
		/// Convenience property for checking if a valid alternate character is in use
		/// </summary>
		public bool ValidAlt => !isMainCharacter && playingAs != null;

		/// <summary>
		/// Convenience property to get the character roster for the current real-life player
		/// </summary>
		public Dictionary<Type, PlayableCharacterSlot> Characters => isMainCharacter ? characters : owner.GetModPlayer<PlayableCharacterPlayer>().characters;

		public override void Load()
		{
			Terraria.On_Player.JumpMovement += ChangeJump;
		}

		/// <summary>
		/// Swaps the player to the specified playable character
		/// </summary>
		/// <param name="to"></param>
		public void Swap<T>() where T : PlayableCharacter
		{
			var slot = Characters[typeof(T)];
			slot.player.Center = Player.Center;
			slot.player.velocity = Player.velocity;
			slot.player.active = true;
			Player.active = false;
			Main.player[Player.whoAmI] = slot.player;
		}

		/// <summary>
		/// Swaps back to the main playable character
		/// </summary>
		public void SwapToMain()
		{
			Main.player[Player.whoAmI] = owner;
			owner.Center = Player.Center;
			owner.velocity = Player.velocity;
			Player.active = false;
			owner.active = true;
		}

		/// <summary>
		/// Here we autoload and initialize all playable characters for this player profile
		/// </summary>
		public override void OnEnterWorld()
		{
			isMainCharacter = true;

			characters = new Dictionary<Type, PlayableCharacterSlot>();

			foreach (Type type in Mod.Code.GetTypes())
			{
				if (!type.IsAbstract && type.IsSubclassOf(typeof(PlayableCharacter)))
				{
					var slot = new PlayableCharacterSlot();

					slot.player = new Player();
					slot.player.GetModPlayer<PlayableCharacterPlayer>().owner = Player;
					//TODO: PlayerLoader.Load via reflection

					slot.playableCharacter = Activator.CreateInstance(type) as PlayableCharacter;
					slot.playableCharacter.player = slot.player;
					slot.playableCharacter.Setup();

					slot.player.GetModPlayer<PlayableCharacterPlayer>().playingAs = slot.playableCharacter;

					characters[type] = slot;
				}
			}
		}

		private void ChangeJump(On_Player.orig_JumpMovement orig, Player self)
		{
			if (self.GetModPlayer<PlayableCharacterPlayer>().ValidAlt)
			{
				if (self.GetModPlayer<PlayableCharacterPlayer>().playingAs.OnJumpInput())
					orig(self);
			}
			else
			{
				orig(self);
			}
		}

		public override void PreUpdateBuffs()
		{
			if (ValidAlt)
				playingAs.PreUpdate();
		}

		public override void PreUpdateMovement()
		{
			if (ValidAlt)
				playingAs.UpdatePhysics();
				// TODO: Cancel vanilla physics based on return value somehow?
		}

		public override void HideDrawLayers(PlayerDrawSet drawInfo)
		{
			if (ValidAlt)
			{
				foreach (PlayerDrawLayer layer in PlayerDrawLayerLoader.Layers)
				{
					layer.Hide();
				}
			}
		}

		public override void ModifyDrawInfo(ref PlayerDrawSet drawInfo)
		{
			if (ValidAlt)
				playingAs.ModifyDrawInfo(ref drawInfo);
		}
	}
}
