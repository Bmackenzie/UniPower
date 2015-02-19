using System;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class PowerGadgetUsage : MonoBehaviour 
{

    public Text[] dataPoints;
    public Text debug;
    bool isLogging = false;

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
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FreeLibrary(IntPtr hModule);
    [DllImport("EnergyLib32")]
    public static extern bool IntelEnergyLibInitialize();
    [DllImport("EnergyLib32")]
    public static extern bool GetNumMsrs(out int nMsr);
    [DllImport("kernel32.dll")]
    static extern uint GetLastError();
    [DllImport("EnergyLib32")]
    public static extern bool ReadSample();
    [DllImport("EnergyLib32")]
    public static extern bool StopLog();
    [DllImport("EnergyLib32")]
    public static extern bool IsGTAvailable(); 
    [DllImport("EnergyLib32")]
    public static extern bool GetNumNodes(out int nNodes);
    [DllImport("EnergyLib32", CharSet = CharSet.Unicode)]
    public static extern bool GetMsrName(int iMsr, StringBuilder szName);
    [DllImport("EnergyLib32")]
    public extern static bool GetMsrFunc(int iMsr, out int pFuncID);
    [DllImport("EnergyLib32")]
    public extern static bool GetPowerData(int iNode, int iMSR, out double pResult, out int nResult);
    [DllImport("EnergyLib32")]
    public extern static bool GetPowerData(int iNode, int iMSR, IntPtr pResult, out int nResult);
    [DllImport("EnergyLib32")]
    public static extern bool GetIAFrequency(int iNode, out int GTFreq);
    [DllImport("EnergyLib32")]
    public static extern bool GetGTFrequency(out int IAFreq);
    [DllImport("EnergyLib32")]
    public static extern bool StartLog(string buffer);

    /*Initialization variables*/
    IntPtr module;
    int pMSRCount = 0;
    //Tracking variables
    int pIAFreq = 0;
    int pGTFreq = 0;
    int pMsrpFuncID = 0;
    int pNodeCount = 0;

    //void OnEnable()
    //{
    //    InitilializeIntelPowerGadget();
    //}

    /// <summary>
    /// Called once
    /// </summary>
	void Start() 
    {
        QueryPlatformCounters();

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

        // Not sure what the purpose of this function is 
        if (GetMsrFunc(1, out pMsrpFuncID))
        {
            Debug.Log("MsrFunc: " + pMsrpFuncID);
        }

        if (StartLog(Application.dataPath))
        {
            Debug.Log("log started " + Application.dataPath);
            isLogging = true;
            InvokeRepeating("GetDataNew", 1, 0.2f);
        }
        Invoke("StopLogging", 60f);
	}

    /// <summary>
    /// Initialize the Intel Power Gadget library
    /// </summary>
    void InitilializeIntelPowerGadget()
    {
        ///Load the Power Gadget library
        LoadNativeDll("C:\\Program Files\\Intel\\Power Gadget 3.0\\EnergyLib32.dll");
    }

    /// <summary>
    /// Load a native library
    /// </summary>
    /// <param name="FileName"></param>
    bool LoadNativeDll(string FileName)
    {
        //Make sure that the module isn't already loaded
        if (module != IntPtr.Zero)
        {
            Debug.Log("Total supported MSRs: " + pMSRCount);
            Debug.Log(" library has alreay been loaded.");
            debug.text += " library has alreay been loaded.";
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
            debug.text  += " library loaded";
            return true;
        }
    }

    /// <summary>
    /// Connect to the driver and return the number of MSRs available on the platform
    /// </summary>
    void QueryPlatformCounters()
    {
        //pMSRCount = UniPowerManager.pMSRCount;

        //Connect to the driver
        if (IntelEnergyLibInitialize() != true)
        {
            Debug.Log("Failed to initialized!");
            debug.text += " failed to initialized!";

        }
        else
        {
            Debug.Log("Initialized!");
            debug.text += " and initialized!.";

        }

        if (pMSRCount == 0)
        {
            //Get the number of supported MSRs for bulk reading and logging
            if (GetNumMsrs(out pMSRCount) == true)
            {
                debug.text += pMSRCount;
                Debug.Log("Total supported MSRs: " + pMSRCount);
            }
        }
        else
        {
            Debug.Log("MSRs already queried: " + pMSRCount);
        }
    }

    IntPtr pResults;
    int nResults = 0;
    int funcID = 0;
    void GetDataNew()
    {
        if (isLogging)
        {
            if (ReadSample())
            {
                pResults = Marshal.AllocHGlobal(sizeof(Double) * 4);
                for (int i = 0; i < pMSRCount; i++)
                {
                    if (GetMsrFunc(i, out funcID))
                    {
                        if (GetPowerData(0, i, pResults, out nResults))
                        {
                            Double[] results = new Double[nResults];
                            Marshal.Copy(pResults, results, 0, nResults);
                            StringBuilder b = new StringBuilder();

                            if (GetMsrName(i, b))
                            {
                                switch (funcID)
                                {
                                        /*
                                    case 0:
                                        Debug.Log(b.ToString() + " Frequency: " + "<color=green>" + results[0].ToString() + "</color>");
                                        break;
                                    case 1:
                                        Debug.Log(b.ToString() + " power (Watts): " + "<color=green>" + results[0].ToString() + "</color>");
                                        Debug.Log(b.ToString() + " Cumulative Energy (Joules): " + "<color=green>" + results[1].ToString() + "</color>");
                                        Debug.Log(b.ToString() + " Cumulative Energy (mWh): " + "<color=green>" + results[2].ToString() + "</color>");
                                        break;
                                    case 2:
                                        Debug.Log(b.ToString() + " Temperature (C): " + "<color=green>" + results[0].ToString() + "</color>");
                                        Debug.Log(b.ToString() + " Hot: " + "<color=green>" + (results[1] == 1 ? "True" : "False") + "</color>");
                                        break;
                                    case 3:
                                        Debug.Log(b.ToString() + " Power limit (Watts): " + "<color=green>" + results[0].ToString() + "</color>");
                                        break;
                                    default:
                                        Debug.Log(funcID.ToString() + " Default case");
                                        break;
                                        */
                                    case 0:
                                        dataPoints[0].text = b.ToString() + " Frequency: " + "<color=green>" + results[0].ToString() + "</color>";
                                        break;
                                    case 1:
                                        if (b.ToString() == "Processor")
                                        {
                                            dataPoints[1].text = b.ToString() + " power (Watts): " + "<color=green>" + results[0].ToString() + "</color>";
                                            dataPoints[2].text = b.ToString() + " Cumulative Energy (Joules): " + "<color=green>" + results[1].ToString() + "</color>";
                                            dataPoints[3].text = b.ToString() + " Cumulative Energy (mWh): " + "<color=green>" + results[2].ToString() + "</color>";
                                        }
                                        if (b.ToString() == "GT")
                                        {
                                            dataPoints[4].text = b.ToString() + " power (Watts): " + "<color=green>" + results[0].ToString() + "</color>";
                                            dataPoints[5].text = b.ToString() + " Cumulative Energy (Joules): " + "<color=green>" + results[1].ToString() + "</color>";
                                            dataPoints[6].text = b.ToString() + " Cumulative Energy (mWh): " + "<color=green>" + results[2].ToString() + "</color>";
                                        }
                                        if (b.ToString() == "IA")
                                        {
                                            dataPoints[7].text = b.ToString() + " power (Watts): " + "<color=green>" + results[0].ToString() + "</color>";
                                            dataPoints[8].text = b.ToString() + " Cumulative Energy (Joules): " + "<color=green>" + results[1].ToString() + "</color>";
                                            dataPoints[9].text = b.ToString() + " Cumulative Energy (mWh): " + "<color=green>" + results[2].ToString() + "</color>";
                                        }
                                        break;
                                    case 2:
                                        dataPoints[10].text = b.ToString() + " Temperature (C): " + "<color=green>" + results[0].ToString() + "</color>";
                                        dataPoints[11].text = b.ToString() + " Hot: " + "<color=green>" + (results[1] == 1 ? "True" : "False") + "</color>";
                                        break;
                                    case 3:
                                        dataPoints[12].text = b.ToString() + " Power limit (Watts): " + "<color=green>" + results[0].ToString() + "</color>";
                                        break;
                                    default:
                                        Debug.Log(funcID.ToString() + " Default case");
                                        break;
                                }
                            }
                        }
                    }
                    /*
                    StringBuilder b = new StringBuilder();
                    if (GetPowerData(0, i, pResults, out nResults))
                    {

                        Double[] results = new Double[nResults];
                        Marshal.Copy(pResults, results, 0, nResults);

                        for (int j = 0; j < nResults; j++)
                        {
                            if (GetMsrName(i, b))
                            {
                                if (nResults > 1)
                                {
                                    if (j == 0)
                                        b.Append(" Power(Watt)");
                                    if (j == 1)
                                        b.Append(" Cumulative Enery(Joules)");
                                    if (j == 2)
                                        b.Append(" Cumulative Enery(mWh)");
                                }
                            }
                            if(j == 0)
                                dataPoints[i].text = b.ToString() + ": " + "<color=green>" + results[j].ToString() + "</color>";
                            
                            //Debug.Log(b.ToString() + ": " + "<color=green>" + results[j].ToString() + "</color>");
                        }
                    }
                    */
                }
                Marshal.FreeHGlobal(pResults);
            }
        }
    }

    void OnDisbale()
    {
        StopLogging();
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
        isLogging = false;
        if (StopLog())
        {
            Debug.Log("log stopped");
        }
    }
}
