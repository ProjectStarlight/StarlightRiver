using StarlightRiver.Core;

namespace StarlightRiver.Content.Abilities
{
    public abstract class CooldownAbility : Ability
    {
        public int Cooldown { get; private set; } = -1;
        public int CooldownBonus { get; set; }
        public abstract int CooldownMax { get; }

        public override bool Available => base.Available && Cooldown <= 0;

        public override void OnActivate()
        {
            StartCooldown();
            base.OnActivate();
        }

        public void StartCooldown()
        {
            Cooldown = CooldownMax + CooldownBonus;
            CooldownBonus = 0;
        }

        public override void UpdateFixed()
        {
            base.UpdateFixed();

            if (!Active)
                Cooldown--;

            if (Cooldown == 0)
                CooldownFinish();
        }

        public virtual void CooldownFinish() { }
    }
}
