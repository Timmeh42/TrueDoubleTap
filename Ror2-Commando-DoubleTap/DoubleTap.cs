using System;
using System.Globalization;
using System.Linq;
using System.Reflection;
using BepInEx;
using RoR2;

namespace TrueDoubleTap
{
    [BepInDependency("com.bepis.r2api")]
    [BepInPlugin("com.Timmeh42.TrueDoubleTap", "True DoubleTap", "1.3.0")]
    public class TrueDoubleTap : BaseUnityPlugin
    {
        public float ParseFloat(string strInput, float defaultVal = 1.0f, float min = float.MinValue, float max = float.MaxValue)
        {
            if (float.TryParse(strInput, NumberStyles.Any, CultureInfo.InvariantCulture, out float parsedFloat))
            {
                return parsedFloat <= min ? min : parsedFloat >= max ? max : parsedFloat;
            }
            return defaultVal;
        }

        public bool ParseBool(string strInput, bool defaultVal = true)
        {
            return bool.TryParse(strInput, out bool parsedBool) ? parsedBool : defaultVal;
        }

        float ratio => ParseFloat(Config.Wrap("DoubleTap", "Ratio", "Ratio of time between shots to time between bursts (decimal number between 0.01 and 1 - default 0.3)", "0.3").Value, 1f, 0.01f, 1f);
        float spread => ParseFloat(Config.Wrap("DoubleTap", "SpreadScale", "Factor to scale the default spread by (decimal number 0 or higher - default 1 (same as unmodded))", "1.0").Value, 1f, 0f);
        bool chatOutput => ParseBool(Config.Wrap("DoubleTap", "ChatOutput", "Whether to write mod output to chat (boolean 'true' or 'false' - default 'true')", "true").Value);

        public void Awake()
        {
            if (chatOutput)
            {
                Chat.AddMessage(String.Format("TrueDoubleTap: burst set to {0}", ratio));
                switch (spread)
                {
                    case 1f:
                        Chat.AddMessage(String.Format("TrueDoubleTap: spread set to {0}x (default)", spread));
                        break;
                    case 0f:
                        Chat.AddMessage("I guess they never miss, huh?");
                        break;
                    default:
                        Chat.AddMessage(String.Format("TrueDoubleTap: spread set to {0}x", spread));
                        break;
                }
            }

            On.RoR2.Run.Awake += (orig, self) =>
            {
                orig(self);
                if (spread != 1.0f)
                {
                    Assembly assembly = self.GetType().Assembly;
                    Type firePistol = assembly.GetTypes().First(t => t.IsClass && t.Namespace == "EntityStates.Commando.CommandoWeapon" && t.Name == "FirePistol2");

                    var recoilAmplitudeOld = (float) firePistol.GetField("recoilAmplitude", BindingFlags.Static | BindingFlags.Public).GetValue(self);
                    var spreadBloomValueOld = (float)firePistol.GetField("spreadBloomValue", BindingFlags.Static | BindingFlags.Public).GetValue(self);

                    firePistol.GetField("recoilAmplitude", BindingFlags.Static | BindingFlags.Public).SetValue(null, spread * recoilAmplitudeOld);
                    firePistol.GetField("spreadBloomValue", BindingFlags.Static | BindingFlags.Public).SetValue(null, spread * spreadBloomValueOld);
                }
            };

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
