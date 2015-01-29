using System;
using System.Text;
using UnityEngine;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class PowerGadgetUsage : MonoBehaviour 
{
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    [DllImport("kernel32.dll")]
    static extern uint GetLastError();
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FreeLibrary(IntPtr hModule);
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
    private IntPtr module;

    //Tracking variables
    int pNodeCount = 0;
    int pMSRCount = 0;
    int pIAFreq = 0;
    int pGTFreq = 0;

    void Awake()
    {
        ///Load the Power Gadget library
        LoadNativeDll("C:\\Program Files\\Intel\\Power Gadget 3.0\\EnergyLib32.dll");
    }
	// Use this for initialization
	void Start () 
    {
        //Initialize and connect to the driver
        if (IntelEnergyLibInitialize() != true){Debug.Log("Failed to initialized!");}

        //Check if Intel Graphics is available on this platform, print GT frequency
        if (IsGTAvailable() && GetGTFrequency(out pGTFreq) == true) { Debug.Log("GPU frequency: " + pGTFreq + "MHz"); } 
 
        //Get and print CPU frequency
        if (GetIAFrequency(1, out pIAFreq) == true) { Debug.Log("CPU Frequency: " + pIAFreq + "MHz"); }
    
        //Chek the number of CPU packages on the system
        if (GetNumNodes(out pNodeCount) == true){Debug.Log("CPUs: " + pNodeCount);}

        //Get the number of supported MSRs for bulk reading and logging
        if (GetNumMsrs(out pMSRCount) == true){Debug.Log("Total supported MSRs: " + pMSRCount);}

        if (ReadSample())
        {
            Debug.Log("sample read");
            Invoke("StartLog", 0);
            Invoke("StopLogging", 5);
        }
	}
    void StartLog()
    {
        if (StartLog(Application.dataPath))
        {
            Debug.Log("log started " + Application.dataPath);
        }
    }

    void StopLogging()
    {
        if (StopLog())
        {
            Debug.Log("log stopped");
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

    /// <summary>
    /// Load a native library
    /// </summary>
    /// <param name="FileName"></param>
    public bool LoadNativeDll(string FileName)
    {
        //Make sure that the module isn't already loaded
        if (module != IntPtr.Zero)
        {
            Debug.Log("Library has alreay been loaded.");
            return false;
        }

        //Load the module
        module = LoadLibrary(FileName);
        Debug.Log("last error = " + Marshal.GetLastWin32Error());
        //Make sure the module has loaded sucessfully
        if (module == IntPtr.Zero)
        {
            throw new Win32Exception();
            return false;
        }
        else 
        {
            return true;
        }
    }
}
