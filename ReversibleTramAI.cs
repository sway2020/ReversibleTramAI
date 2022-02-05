using ICities;
using CitiesHarmony.API;

namespace ReversibleTramAI
{
    public class Mod : IUserMod
    {
        public const string version = "v0.1.1";
        public string Name => "Reversible Tram AI " + version;
        public string Description
        {
            get { return "Provides a modified reversible Tram AI"; }
        }

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled)
            {
                Patcher.UnpatchAll();
            }
        }

    }
}
