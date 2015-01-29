using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class PowerGadgetUsage : MonoBehaviour 
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    [DllImport("EnergyLib32")]
    public static extern bool IntelEnergyLibInitialize(); 
    [DllImport("EnergyLib32")]
    public static extern bool ReadSample();
    [DllImport("EnergyLib32")]
    public static extern bool StopLog();
    [DllImport("EnergyLib32")]
    public static extern bool IsGTAvailable(); 
    [DllImport("EnergyLib32")]
    public static extern bool GetNumNodes(out int nNodes);
    [DllImport("EnergyLib32")]
    public static extern bool GetNumMsrs(out int nMsr);
    [DllImport("EnergyLib32")]
    public static extern bool GetIAFrequency(int iNode, out int GTFreq);
    [DllImport("EnergyLib32")]
    public static extern bool GetGTFrequency(out int IAFreq);
    [DllImport("EnergyLib32")]
    public static extern bool StartLog(string buffer);

    /*Variables*/
    private static IntPtr module;

    //Status variable
    bool pInitialize;
    bool pIsGTAvailable;
    bool pGetNumNodes;
    bool pGetNumMsrs;
    bool pGetIAFrequency;
    bool pGetGTFrequency;

    //Tracking variables
    int pNodeCount = 0;
    int pMSRCount = 0;
    int pIAFreq = 0;
    int pGTFreq = 0;

	// Use this for initialization
	void Start () 
    {
        ///Check and Load the Power Gadget library
        CheckLoadPowerGadget();

        pInitialize = IntelEnergyLibInitialize();
        if (pInitialize != true)
        {
            Debug.Log("Failed to initialized!");
        }

        pIsGTAvailable = IsGTAvailable();
        if (pIsGTAvailable == true)
        {
            pGetGTFrequency = GetGTFrequency(out pGTFreq);
            if (pGetGTFrequency == true)
            {
                Debug.Log("GPU frequency: " + pGTFreq);
            }
        }

        pGetNumNodes = GetNumNodes(out pNodeCount);
        if (pGetNumNodes == true)
        {
            Debug.Log("CPUs: " + pNodeCount);
        }

        pGetNumMsrs = GetNumMsrs(out pMSRCount);
        if (pGetNumMsrs == true)
        {
            Debug.Log("Total supported MSRs: " + pMSRCount);
        }

        pGetIAFrequency = GetIAFrequency(1, out pIAFreq);
        if (pGetIAFrequency == true)
        {
            Debug.Log("CPU Frequency: " + pIAFreq);
        }

        if(ReadSample())
        {
            Debug.Log("sample read");
        }
        Invoke("StartLog", 5);
        Invoke("StopLogging", 15);
	}
    void StartLog()
    {
        if (StartLog("C:\\User\\Bryan\\Desktop\\data.csv"))
        {
            Debug.Log("log started");
        }
    }

    void StopLogging()
    {
        if (StopLog())
        {
            Debug.Log("log stopped");
        }
    }

    void Update()
    {
        ReadSample();
        pGetNumMsrs = GetNumMsrs(out pMSRCount);
    }

    /// <summary>
    /// Checks if Intel Power Gadget is installed on the system, then loads it
    /// </summary>
    /// <returns></returns>
    public bool CheckLoadPowerGadget()
    {
        string isInstalled = System.Environment.GetEnvironmentVariable("IPG_Dir");

        if (isInstalled != null)
        {
            LoadNativeDll(isInstalled + "\\EnergyLib32.dll");
            return true;
        }

        Debug.Log("Failed to locate/Load Power Gadget please check your install");
        return false;
    }

    /// <summary>
    /// Load a native library
    /// </summary>
    /// <param name="FileName"></param>
    public static void LoadNativeDll(string FileName)
    {
        if (module != IntPtr.Zero)
        {
            Debug.Log("Library has alreay been loaded.");
            return;
        }

        module = LoadLibrary(FileName);
        if (module == IntPtr.Zero)
        {
            throw new Win32Exception();
        }
    }

    
}
