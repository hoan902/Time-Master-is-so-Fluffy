using System;
using UnityEngine;

public static class STGameController
{
    public static Action<int, int> updatePlayerHpEvent;//<last, current>
    public static Action<DamageDealerInfo> hitPlayerEvent;//other object hit player
    public static Action<bool, bool> updatePlayerImmunityEvent; // <active, fromhit>

    public static void Init()
    {
        STGameModel.Init();
    }

    public static void UpdatePlayerHp(int value)
    {
        if(STGameModel.hp <= 0 && value <= 0)
            return;
        int last = STGameModel.hp;
        STGameModel.UpdateHp(value);
        updatePlayerHpEvent?.Invoke(last, STGameModel.hp);
    }

    public static void UpdatePlayerImmunity(bool active, bool fromHit)
    {
        STGameModel.immunity = active;
        updatePlayerImmunityEvent?.Invoke(active, fromHit);
    }

    public static void HitPlayer(DamageDealerInfo damage)
    {
        if(STGameModel.hp <= 0 || STGameModel.immunity)
            return;
        UpdatePlayerImmunity(true, true);
        UpdatePlayerHp(-damage.damage);
        hitPlayerEvent?.Invoke(damage);
    }
    // public static void TrySkin(SkinInfor newSkin)
    // {
    //     STGameModel.currentSkin = newSkin;
    //     trySkinEvent?.Invoke(newSkin);
    // }
    // public static void TryWeapon(WeaponName weaponName)
    // {
    //     GameController.EquipWeapon(weaponName);
    // }
}
