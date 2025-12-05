using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public enum WeaponType { WaterGun, Harpoon, Torpedo }

    public static WeaponType equippedWeapon = WeaponType.WaterGun;
}
