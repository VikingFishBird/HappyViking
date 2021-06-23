public class Weapon
{
    public enum WeaponType { Blunt, Sharp, Ranged };

    public WeaponType type;
    public string name;
    public int damage;
    public int range;
    public int recoveryTime;
    public float accuracy;
    public float moveSpeed;

    public Weapon(WeaponType type, string name, int damage, int range, 
        int recoveryTime, float accuracy, float moveSpeed) {

        this.type = type;
        this.name = name;
        this.damage = damage;
        this.range = range;
        this.recoveryTime = recoveryTime;
        this.accuracy = accuracy;
        this.moveSpeed = moveSpeed;
    }
}
