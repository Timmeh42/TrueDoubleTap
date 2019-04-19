using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using RoR2;

namespace TrueDoubleTap
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Timmeh42.TrueDoubleTap", "True DoubleTap", "1.2.2")]
    public class TrueDoubleTap : BaseUnityPlugin
    {
        public float ParseFloat(string strInput, float defaultVal = 1.0f, float min = float.MinValue, float max = float.MaxValue)
        {
            if (float.TryParse(strInput, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                if (parsedFloat <= min) parsedFloat = min;
                if (parsedFloat >= max) parsedFloat = max;

                return parsedFloat;
            }
            return defaultVal;
        }

        public bool ParseBool(string strInput, bool defaultVal = true)
        {
            return bool.TryParse(strInput, out bool parsedBool) ? parsedBool : defaultVal;
        }


        float ratio => ParseFloat(Config.Wrap("DoubleTap", "Ratio", "Ratio of time between shots to time between bursts (decimal number between 0.01 and 1)", "0.3").Value, 1f, 0.01f, 1f);
        bool chatOutput => ParseBool(Config.Wrap("DoubleTap", "ChatOutput", "Whether to write mod output to chat (boolean 'true' or 'false')", "true").Value);

        public void Awake()
        {
            if (chatOutput) Chat.AddMessage(String.Format("Commando DoubleTap ratio set to {0}", ratio));

            On.EntityStates.Commando.CommandoWeapon.FirePistol2.OnEnter += (orig, self) =>
            {
                Assembly assembly = self.GetType().Assembly;
                Type firePistol = assembly.GetTypes().First(t => t.IsClass && t.Namespace == "EntityStates.Commando.CommandoWeapon" && t.Name == "FirePistol2");
                float current_duration = (float) firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).GetValue(null);
                firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, 2f * ratio / (1f + ratio) * current_duration);
                FieldInfo remainingShots = firePistol.GetField("remainingShots", BindingFlags.Public | BindingFlags.Instance);
                if ((int) remainingShots.GetValue(self) % 2 != 0)
                {
                    firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, 2f * 1f / (1f + ratio) * current_duration);
                }
                orig(self);
                firePistol.GetField("baseDuration", BindingFlags.Static | BindingFlags.Public).SetValue(null, current_duration);
            };
        }
    }
}
