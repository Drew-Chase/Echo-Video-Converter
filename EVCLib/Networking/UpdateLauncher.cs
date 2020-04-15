using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;

namespace ChaseLabs.Echo.Video_Converter.Networking
{
    /// <summary>
    /// <para>
    /// Author: Drew Chase
    /// </para>
    /// <para>
    /// Company: Chase Labs
    /// </para>
    /// </summary>
    public class UpdateLauncher
    {
        public static bool CheckForUpdate()
        {
            return UpdateManager.Singleton.CheckForUpdate(Values.Singleton.LauncherVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL);
        }
    }
}