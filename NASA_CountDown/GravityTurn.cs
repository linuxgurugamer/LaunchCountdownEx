
using KSP.Localization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using NASA_CountDown.Config;

// Thanks to nightingale for this file
// Original file: A https://github.com/jrossignol/ContractConfigurator/blob/master/source/ContractConfigurator/Util/Version.cs#L29
//
namespace NASA_CountDown
{
    /// <summary>
    /// Utility class with version checking functionality.
    /// </summary>
    public static class GravityTurnAPI
    {
        public static Assembly GravityTurnAssembly;

        public static bool GravityTurnActive = false;

        /// <summary>
        /// Verify the loaded assembly meets a minimum version number.
        /// </summary>
        /// <param name="name">Assembly name</param>
        /// <param name="version">Minium version</param>
        /// <param name="silent">Silent mode</param>
        /// <returns>The assembly if the version check was successful.  If not, logs and error and returns null.</returns>
        public static Assembly VerifyAssemblyVersion(string name, string version, bool silent = false)
        {
            Log.Info("Entering VerifyAssemblyVersion, name: " + name);
            // Logic courtesy of DMagic
            var assembly = AssemblyLoader.loadedAssemblies.SingleOrDefault(a => a.assembly.GetName().Name == name);
            Log.Info("VerifyAssemblyVersion 2");
            if (assembly != null)
            {
                if (version == null || version == "")
                    return assembly.assembly;

                string receivedStr;

                // First try the informational version
                var ainfoV = Attribute.GetCustomAttribute(assembly.assembly, typeof(AssemblyInformationalVersionAttribute)) as AssemblyInformationalVersionAttribute;
                if (ainfoV != null)
                {
                    receivedStr = ainfoV.InformationalVersion;
                }
                // If that fails, use the product version
                else
                {
                    receivedStr = FileVersionInfo.GetVersionInfo(assembly.assembly.Location).ProductVersion;
                }

                System.Version expected = ParseVersion(version);
                System.Version received = ParseVersion(receivedStr);

                if (received >= expected)
                {
                    Log.Info("Version check for '" + name + "' passed.  Minimum required is " + version + ", version found was " + receivedStr);
                    return assembly.assembly;
                }
                else
                {
                    Log.Error("Version check for '" + name + "' failed!  Minimum required is " + version + ", version found was " + receivedStr);
                    return null;
                }
            }
            else
            {
                Log.Error("Couldn't find assembly for '" + name + "'!");
                return null;
            }
        }

        private static System.Version ParseVersion(string version)
        {
            Match m = Regex.Match(version, @"^[vV]?(\d+)(.(\d+)(.(\d+)(.(\d+))?)?)?");
            int major = m.Groups[1].Value.Equals("") ? 0 : Convert.ToInt32(m.Groups[1].Value);
            int minor = m.Groups[3].Value.Equals("") ? 0 : Convert.ToInt32(m.Groups[3].Value);
            int build = m.Groups[5].Value.Equals("") ? 0 : Convert.ToInt32(m.Groups[5].Value);
            int revision = m.Groups[7].Value.Equals("") ? 0 : Convert.ToInt32(m.Groups[7].Value);

            return new System.Version(major, minor, build, revision);
        }

        static internal bool GTAvailable = false;

        /// <summary>
        /// Verifies that the Gravity Turn version the player has is compatible.
        /// </summary>
        /// <returns>Whether the check passed.</returns>
        public static bool VerifyGTVersion()
        {
            string minVersion = "";
            if (GravityTurnAssembly == null)
            {
                gtName = Localizer.Format("#LOC_NASA_7");
                GravityTurnAssembly = GravityTurnAPI.VerifyAssemblyVersion(gtName, minVersion);
                if (GravityTurnAssembly == null)
                {
                    gtName = Localizer.Format("#LOC_NASA_8");
                    GravityTurnAssembly = GravityTurnAPI.VerifyAssemblyVersion(gtName, minVersion);
                }
                GTAvailable = (GravityTurnAssembly != null);
            }
            Log.Info("VerifyGTVersion, GTAvailable: " + GTAvailable);
            return GravityTurnAssembly != null;
        }

        static string gtName = Localizer.Format("#LOC_NASA_7");
        public static bool Launch()
        {
            Log.Info("Launch");
            if (!GTAvailable || !ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].useGravityTurn)
            {
                return false;
            }
            if (!ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].LaunchSequenceControl)
                return false;
            Log.Info("GravityTurn detected");
            try
            {
                //Type calledType = Type.GetType("GravityTurn.GravityTurner,GravityTurn");
                #region NO_LOCALIZATION
                Type calledType = Type.GetType("GravityTurn.GravityTurner," + gtName);
                if (calledType != null)
                {
                    MonoBehaviour GTRef = (MonoBehaviour)UnityEngine.Object.FindObjectOfType(calledType); //assumes only one instance of class GravityTurn exists as this command returns first instance found, also must inherit MonoBehavior for this command to work. Getting a reference to your Historian object another way would work also.
                    if (GTRef != null)
                    {
                        MethodInfo myMethod = calledType.GetMethod("Launch", BindingFlags.Instance | BindingFlags.Public);
                        #endregion
                        if (myMethod != null)
                        {
                            Log.Info("Invoking Launch");
                            myMethod.Invoke(GTRef, null);
                            GravityTurnActive = true;
                            return true;
                        }
                        else
                        {
                            Log.Info("Launch not available in GravityTurn");
                            GTAvailable = false;
                            return false;
                        }
                    }
                    else
                    {
                        Log.Info("GTRef  failed");
                        GTAvailable = false;
                        return false;
                    }
                }
                Log.Info("calledtype failed");
                GTAvailable = false;
                return false;
            }
            catch (Exception e)
            {
                Log.Info("Error calling type: " + e);
                GTAvailable = false;
                return false;
            }
        }

        public static bool Kill()
        {
            Log.Info("Kill");
            if (!GTAvailable || !ConfigInfo.Instance.VesselOptions[ModuleNASACountdown.CraftName(FlightGlobals.ActiveVessel)].useGravityTurn)
            {
                return false;
            }
            Log.Info("GravityTurn detected");
            try
            {
                #region NO_LOCALIZATION
                //Type calledType = Type.GetType("GravityTurn.GravityTurner,GravityTurn");
                Type calledType = Type.GetType("GravityTurn.GravityTurner," + gtName);
                if (calledType != null)
                {
                    MonoBehaviour GTRef = (MonoBehaviour)UnityEngine.Object.FindObjectOfType(calledType); //assumes only one instance of class GravityTurn exists as this command returns first instance found, also must inherit MonoBehavior for this command to work. Getting a reference to your Historian object another way would work also.
                    if (GTRef != null)
                    {
                        MethodInfo myMethod = calledType.GetMethod("Kill", BindingFlags.Instance | BindingFlags.Public);
                        #endregion
                        if (myMethod != null)
                        {
                            Log.Info("Invoking Kill");
                            myMethod.Invoke(GTRef, null);
                            return true;
                        }
                        else
                        {
                            Log.Info("Kill not available in GravityTurn");
                            GTAvailable = false;
                            return false;
                        }
                    }
                    else
                    {
                        Log.Info("GTRef  failed");
                        GTAvailable = false;
                        return false;
                    }
                }
                Log.Info("calledtype failed");
                GTAvailable = false;
                return false;
            }
            catch (Exception e)
            {
                Log.Info("Error calling type: " + e);
                GTAvailable = false;
                return false;
            }
        }

    }

}
