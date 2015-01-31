using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class PowerGadgetUsage : MonoBehaviour 
{
    public GameObject[] DataPoints;

    /* TODO:
    Implement the following API calls:
     
        bool GetPowerData(int iNode, int iMSR, double *pResult, int *nResult); - Returns the data collected by the most recent call to ReadSample(). The returned data is for the data on the package specified by iNode, from the MSR specified by iMSR. The data is returned in pResult, and the number of double results returned in pResult is returned in nResult. Refer Table 1: MSR Functions.
 
        bool GetSysTime(void *pSysTime); - Returns the system time as of the last call to ReadSample(). The data returned in pSysTime is structured as follows: pSysTime[63:32] = time in seconds ; pSysTime[31:0] = time in nanoseconds
  
        bool GetTimeInterval(double *pOffset); - Returns in pOffset the time (in seconds) that has elapsed between the two most recent calls to ReadSample().
 
        bool GetTDP(int iNode, double *TDP); - Reads the package power info MSR on the package specified by iNode, and returns the TDP (in Watts) in TDP. It is recommended that Package Power Limit is used instead of TDP whenever possible, as it is a more accurate upper bound to the package power than TDP.
 
        bool GetMaxTemperature(int iNode, int *degreeC); - Reads the temperature target MSR on the package specified by iNode, and returns the maximum temperature (in degrees Celsius) in degreeC.
 
        bool GetTemperature(int iNode, int *degreeC); - Reads the temperature MSR on the package specified by iNode, and returns the current temperature (in degrees) Celsius in degreeC.
 
        bool GetBaseFrequency(int iNode, double *pBaseFrequency); - Returns in pBaseFrequency the advertised processor frequency for the package specified by iNode.
     */

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

    [DllImport("EnergyLib32", CharSet = CharSet.Unicode)]
    public static extern bool GetMsrName(int iMsr, StringBuilder szName);
    [DllImport("EnergyLib32")]
    public extern static bool GetMsrFunc(int iMsr, out int pFuncID);
    [DllImport("EnergyLib32")]
    public extern static bool GetPowerData(int iNode, int iMSR, out double pResult, out int nResult);
    
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
    int pMsrpFuncID = 0;

    /// <summary>
    /// Called once 
    /// </summary>
    void Awake()
    {
        ///Load the Power Gadget library
        LoadNativeDll("C:\\Program Files\\Intel\\Power Gadget 3.0\\EnergyLib32.dll");
    }

    /// <summary>
    /// Called once
    /// </summary>
	void Start () 
    {
        //Initialize and connect to the driver
        if (IntelEnergyLibInitialize() != true)
        {
            Debug.Log("Failed to initialized!");
        }

        //Check if Intel Graphics is available on this platform, print GT frequency
        if (IsGTAvailable() && GetGTFrequency(out pGTFreq) == true) 
        { 
            Debug.Log("GPU frequency: " + pGTFreq + "MHz"); 
        } 
 
        //Get and print CPU frequency
        if (GetIAFrequency(1, out pIAFreq) == true) 
        { 
            Debug.Log("CPU Frequency: " + pIAFreq + "MHz"); 
        }
    
        //Chek the number of CPU packages on the system
        if (GetNumNodes(out pNodeCount) == true)
        {
            Debug.Log("CPUs: " + pNodeCount);
        }

        //Get the number of supported MSRs for bulk reading and logging
        if (GetNumMsrs(out pMSRCount) == true)
        {
            Debug.Log("Total supported MSRs: " + pMSRCount);
            DataPoints[0].GetComponent<Text>().text = pMSRCount.ToString();
        }

        // Not sure what the purpose of this function is 
        if (GetMsrFunc(1, out pMsrpFuncID))
        {
            Debug.Log("MsrFunc: " + pMsrpFuncID);
        }

        double _double = 0.0;
        int _int = 0;
        if (GetPowerData(1, 1, out _double, out _int))
        {
            Debug.Log("Power Data: " + _double + " + " + _int);
        }

        for (int i = 0; i < pMSRCount; i++)
        {
            StringBuilder b = new StringBuilder();
            if (GetMsrName(i, b))
            {
                Debug.Log("MSR name: " + b.ToString());
            }
        }

        //Invoke("StartLog", 0);
        //Invoke("StopLogging", 5);

        //if (ReadSample())
        //{
        //    Debug.Log("sample read");
        //    Invoke("StartLog", 0);
        //    Invoke("StopLogging", 5);
        //}
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
        }
        else 
        {
            return true;
        }
    }
}
