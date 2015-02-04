using System;
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class LoadPowerGadget : MonoBehaviour 
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FreeLibrary(IntPtr hModule);
    [DllImport("EnergyLib32")]
    public static extern bool IntelEnergyLibInitialize();
    [DllImport("EnergyLib32")]
    public static extern bool GetNumMsrs(out int nMsr);

    /*Variables*/
    static IntPtr module;
    public static int pMSRCount = 0;

    [MenuItem("Power Gadget/Initialize")]
    static void InitilializeIntelPowerGadget()
    {
        ///Load the Power Gadget library
        LoadNativeDll("C:\\Program Files\\Intel\\Power Gadget 3.0\\EnergyLib32.dll");
    }

    [MenuItem("Power Gadget/Get Counters")]
    static void QueryPlatformCounters()
    {
        //Initialize and connect to the driver
        if (IntelEnergyLibInitialize() != true)
        {
            Debug.Log("Failed to initialized!");
        }
        else
        {
            Debug.Log("Initialized!");
        }

        if (pMSRCount == 0)
        {
            //Get the number of supported MSRs for bulk reading and logging
            if (GetNumMsrs(out pMSRCount) == true)
            {
                Debug.Log("Total supported MSRs: " + pMSRCount);
            }
        }
        else
        {
            Debug.Log("MSRs already queried: " + pMSRCount);
        }
    }

    /// <summary>
    /// Load a native library
    /// </summary>
    /// <param name="FileName"></param>
    static bool LoadNativeDll(string FileName)
    {
        //Make sure that the module isn't already loaded
        if (module != IntPtr.Zero)
        {
            Debug.Log("Total supported MSRs: " + pMSRCount);
            Debug.Log("Library has alreay been loaded.");
            return false;
        }

        //Load the module
        module = LoadLibrary(FileName);
        //sDebug.Log("last error = " + Marshal.GetLastWin32Error());

        //Make sure the module has loaded sucessfully
        if (module == IntPtr.Zero)
        {
            throw new Win32Exception();
        }
        else
        {
            Debug.Log("Library loaded.");
            return true;
        }
    }

    void OnDisable()
    {
        //Make sure there is something to release
        if (module != IntPtr.Zero)
        {
            //release the module
            if (FreeLibrary(module))
            {
                Debug.Log("Library released");
            }
        }
    }
}

