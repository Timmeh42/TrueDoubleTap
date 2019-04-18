using System;
using System.Linq;
using System.Reflection;
using BepInEx;

namespace TrueDoubleTap
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Timmeh42.TrueDoubleTap", "True DoubleTap", "1.0.0")]
    public class TrueDoubleTap : BaseUnityPlugin
    {
        public void Awake()
        {
            On.EntityStates.Commando.CommandoWeapon.FirePistol2.OnEnter += (orig, self) =>
            {
                Assembly assembly = self.GetType().Assembly;
                Type firePistol = assembly.GetTypes().First(t => t.IsClass && t.Namespace == "EntityStates.Commando.CommandoWeapon" && t.Name == "FirePistol2");
                float current_duration = (float) firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, 0.5f * current_duration);
                FieldInfo remainingShots = firePistol.GetField("remainingShots", BindingFlags.Public | BindingFlags.Instance);
                if ((int) remainingShots.GetValue(self) % 2 != 0)
                {
                    firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, 1.5f * current_duration);
                }
                orig(self);
                firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, current_duration);
            };
        }
    }
}
