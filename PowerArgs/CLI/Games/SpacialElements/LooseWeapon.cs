﻿using PowerArgs.Cli;
using PowerArgs.Cli.Physics;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

namespace PowerArgs.Games
{
    public class LooseWeapon : LooseItem
    {
        public Weapon InnerWeapon { get; private set; }

        public LooseWeapon(Weapon weapon)
        {
            this.InnerWeapon = weapon;
        }

        public override bool CanIncorporate(Character target)
        {
            return target.Inventory != null;
        }

        public override void Incorporate(Character target)
        {
            foreach(var item in target.Inventory.Items)
            {
                if(item.GetType() == InnerWeapon.GetType())
                {
                    if ((item as Weapon).AmmoAmount != -1)
                    {
                        (item as Weapon).AmmoAmount += InnerWeapon.AmmoAmount;
                    }
                    return;
                }
            }

            target.Inventory.Items.Add(InnerWeapon);
        }
    }

    [SpacialElementBinding(typeof(LooseWeapon))]
    public class LooseWeaponRenderer : ThemeAwareSpacialElementRenderer
    {
        public LooseWeaponRenderer()
        {
            Background = ConsoleColor.White;
            Foreground = ConsoleColor.Black;
        }

        protected override void OnPaint(ConsoleBitmap context)
        {
            var indicator = (Element as LooseWeapon).InnerWeapon.GetType().Name[0];
            context.Pen = new PowerArgs.ConsoleCharacter(indicator, Foreground, Background);
            context.DrawPoint(0, 0);
        }
    }

    public class LooseWeaponReviver : ItemReviver
    {
        public bool TryRevive(LevelItem item, List<LevelItem> allItems, out ITimeFunction hydratedElement)
        {
            if (item.HasValueTag("ammo") == false || item.HasValueTag("amount") == false)
            {
                hydratedElement = null;
                return false;
            }

            var weaponTypeName = item.GetTagValue("ammo");
            var weaponType = Type.GetType(weaponTypeName, false, true);

            if (weaponType == null)
            {
                weaponType = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Weapon)) && t.Name == weaponTypeName).SingleOrDefault();
            }

            if (weaponType == null)
            {
                weaponType = Assembly.GetEntryAssembly().GetTypes().Where(t => t.IsSubclassOf(typeof(Weapon)) && t.Name == weaponTypeName).SingleOrDefault();
            }

            if (weaponType == null)
            {
                throw new ArgumentException("Could not resolve weapon type: " + weaponTypeName);
            }

            var amount = int.Parse(item.GetTagValue("amount"));

            var weapon = Activator.CreateInstance(weaponType) as Weapon;
            weapon.AmmoAmount = amount;

            hydratedElement = new LooseWeapon(weapon);
            return true;
        }
    }
}
