using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpButton : MonoBehaviour
{
    public TMP_Text weaponName;
    public TMP_Text weaponDesciption;
    public Image weaponIcon;

    private Weapon assignedWeapon;
    public void ActivateButton(Weapon weapon)
    {
        weaponName.text = weapon.name;
        weaponDesciption.text = weapon.stats[weapon.weaponLevel].description;
        weaponIcon.sprite = weapon.weaponImage;

        assignedWeapon = weapon;
    }

    public void SelectUpgrade()
    {
        assignedWeapon.levelUP();
        UIController.Instance.LevelUpPanelClose();
        AudioController.Instance.PlaySound(AudioController.Instance.selectUpgrade);
    }
}
