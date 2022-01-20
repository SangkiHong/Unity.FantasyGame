
namespace Sangki
{
    public interface IInteractable
    {
        void Interact();
    }

    public interface IDamageable
    {
        void Damage(int damageAmount);
    }

    public interface IHealable
    {
        void Heal(int healAmount);
    }
}
