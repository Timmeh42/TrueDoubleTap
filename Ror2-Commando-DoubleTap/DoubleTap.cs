using System;
using System.Reflection;
using BepInEx;
using AetherLib;

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
                Type firePistol = assembly.GetClass("EntityStates.Commando.CommandoWeapon", "FirePistol2");

                float curdur = (float) firePistol.GetPublicStaticFieldInfo("baseDuration").GetValue(null);

                FieldInfo remainingShots = firePistol.GetField("remainingShots", BindingFlags.Public | BindingFlags.Instance);

                firePistol.SetPublicStaticField("baseDuration", 0.5f * curdur);
                if ((int) remainingShots.GetValue(self) % 2 != 0)
                {
                    firePistol.SetPublicStaticField("baseDuration", 1.5f * curdur);
                }
                orig(self);
                firePistol.SetPublicStaticField("baseDuration", curdur);
            };
        }
    }
}
