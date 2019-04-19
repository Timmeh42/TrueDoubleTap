using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using RoR2;

namespace TrueDoubleTap
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Timmeh42.TrueDoubleTap", "True DoubleTap", "1.2.0")]
    public class TrueDoubleTap : BaseUnityPlugin
    {
        public float RatioConfig(string configline)
        {
            if (float.TryParse(configline, NumberStyles.Any, CultureInfo.InvariantCulture, out float ratioTemp))
            {
                var ratioMin = 0.01f;
                var ratioMax = 1f;
                return (ratioTemp <= ratioMin) ? ratioMin : (ratioTemp >= ratioMax) ? ratioMax : ratioTemp;
            }
            return 1f;
        }
        float Ratio => RatioConfig(Config.Wrap("DoubleTap", "Ratio", "Ratio of time between shots to time between bursts", "0.3").Value);

        public bool ChatOutputConfig(string configline)
        {
            if (bool.TryParse(configline, out bool chatOutput))
            {
                return chatOutput;
            }
            return true;
        }
        bool chatOutput => ChatOutputConfig(Config.Wrap("DoubleTap", "ChatOutput", "Whether to write mod output to chat", "true").Value);

        public void Awake()
        {
            if (chatOutput) Chat.AddMessage(String.Format("Commando DoubleTap ratio set to {0}", Ratio));

            On.EntityStates.Commando.CommandoWeapon.FirePistol2.OnEnter += (orig, self) =>
            {
                Assembly assembly = self.GetType().Assembly;
                Type firePistol = assembly.GetTypes().First(t => t.IsClass && t.Namespace == "EntityStates.Commando.CommandoWeapon" && t.Name == "FirePistol2");
                float current_duration = (float) firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, 2f * Ratio / (1f + Ratio) * current_duration);
                FieldInfo remainingShots = firePistol.GetField("remainingShots", BindingFlags.Public | BindingFlags.Instance);
                if ((int) remainingShots.GetValue(self) % 2 != 0)
                {
                    firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, 2f * 1f / (1f + Ratio) * current_duration);
                }
                orig(self);
                firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, current_duration);
            };
        }
    }
}
