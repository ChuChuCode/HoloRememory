public interface IHealth
{
    int maxHealth { get; set; }
    int currentHealth { get; set; }
    bool isDead { get; set; }
    void InitialHealth();
    void GetDamage(int damage);
    void Death();
}
