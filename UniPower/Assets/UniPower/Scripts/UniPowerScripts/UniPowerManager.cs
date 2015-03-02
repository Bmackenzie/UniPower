using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.ComponentModel;
using System.Runtime.InteropServices;

public class UniPowerManager : MonoBehaviour
{
    #region Library Import
    [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
    static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)]string lpFileName);
    [DllImport("kernel32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool FreeLibrary(IntPtr hModule);
    [DllImport("EnergyLib32")]
    public static extern bool IntelEnergyLibInitialize();
    [DllImport("EnergyLib32")]
    public static extern bool GetNumMsrs(out int nMsr);
    #endregion

    #region Variables
	char[] delimiters = {'/'};
    static bool isInitialized = false;
    static IntPtr module;
    public static int pMSRCount = 0;

    [SerializeField]
    private Button Btn_1 = null; // assign in the editor

	[SerializeField]
    private static UniPowerManager menuManager;
    #endregion
	
    #region Initialization
    void Start()
    {
        Btn_1.onClick.AddListener(() => { Btn_1_OnClick(); });
    }
    #endregion
    
    #region Callbacks

    private void Btn_1_OnClick()
    {
        if (!isInitialized)
        {

			//Uncomment below if you want to place the dll into the project rather than loading at runtime. 
			/*  
			string path = "";
			string[] splitPath = Application.dataPath.Split(delimiters);
			foreach(string word in splitPath)
			{
				path += word + "\\";
			}
			Debug.Log(path + "Plugins\\EnergyLib32.dll");
            ///Load the Power Gadget library
			LoadNativeDll(path + "Plugins\\EnergyLib32.dll");
			*/
			LoadNativeDll("C:\\Program Files\\Intel\\Power Gadget 3.0\\EnergyLib32.dll");
            isInitialized = true;
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
	#endregion 
}


