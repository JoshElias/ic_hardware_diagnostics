using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Management.Instrumentation;

namespace HardwareDiagnostics
{  
    public class SMARTManager
    {
        #region Fields

        private static SMARTManager m_Instance;
        public static SMARTManager Instance
        {
            get {
                if (m_Instance == null)
                    m_Instance = new SMARTManager();

                return m_Instance;
            }
        }

        public readonly string SMARTXML;



        #endregion

        #region Constructor

        private SMARTManager()
        {
            m_Instance = this;
        }


        #endregion

        #region Methods

        public XDocument CreateSMARTXML(ref string ERR)
        {
            try {
                // WQL for WMI's info on the storage driver
                var searcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MSStorageDriver_ATAPISmartData");

                foreach (ManagementObject queryObj in searcher.Get()) {
                    // get the VendorSpecific byte[] from storage driver
                    var arrVendorSpecific = (byte[])queryObj.GetPropertyValue("VendorSpecific");

                    //Create SMART Data XML
                    XDocument smartXML = new XDocument();
                    XElement root = new XElement("root");
                    XElement smart = new XElement("SMART");

                    //Get the dictonary of SMART attributes
                    var dict = new SmartData(arrVendorSpecific);
                    string elementName;
                    XElement element;
                    foreach (var attribute in dict.Attributes) {
                        //Start element
                        elementName = attribute.AttributeType.ToString();
                        if (!String.IsNullOrEmpty(elementName) && Char.IsLetter(elementName[0]))
                        {
                            element = new XElement(elementName);

                            //Write Attribute
                            XElement hddAttribute = new XElement("FailureImminent", attribute.FailureImminent.ToString());

                            element.Add(hddAttribute);
                            smart.Add(element);
                        }
                    }

                    root.Add(smart);
                    smartXML.Add(root);

                    return smartXML;
                }
            } catch (ManagementException e) {
                ERR = "An error occurred while querying for HDD WMI data: " + e.Message; 
            }

            return null;
        }

        public string RunChkdsk(string a_DriveName, ref string ERR)
        {
            try
            {
                ManagementObject classInstance =
                                    new ManagementObject("root\\CIMV2",
                                    String.Format("Win32_LogicalDisk.DeviceID='{0}'", a_DriveName),
                                    null);

                // Obtain in-parameters for the method
                ManagementBaseObject inParams =
                    classInstance.GetMethodParameters("Chkdsk");

                // Add the input parameters.
                inParams["FixErrors"] = true;
                inParams["OkToRunAtBootUp"] = false;
                inParams["RecoverBadSectors"] = true;
                inParams["VigorousIndexCheck"] = true;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("Chkdsk", inParams, null);

                // List outParams
                string callback = "Out parameters:\n" + "ReturnValue: " + outParams["ReturnValue"];
                return callback;
            }
            catch (ManagementException err)
            {
                ERR = "An error occurred while trying to execute the WMI method: " + err.Message;
            }

            return null;
        }
        public string ScheduleAutoChk(string[] a_DriveNames, ref string ERR)
        {
            try {
                ManagementClass classInstance =
                    new ManagementClass("root\\CIMV2",
                    "Win32_Volume", null);

                // Obtain in-parameters for the method
                ManagementBaseObject inParams =
                    classInstance.GetMethodParameters("ScheduleAutoChk");

                // Add the input parameters.
                inParams["Volume"] = a_DriveNames;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("ScheduleAutoChk", inParams, null);

                // List outParams
                string callback = "Out parameters:\n" + "ReturnValue: " + outParams["ReturnValue"];
                return callback;
            } catch (ManagementException err) {
                ERR = "An error occurred while trying to execute the WMI method: " + err.Message;
            }

            return null;
        }
        public string ExcludeFromAutoChk(string[] a_DriveNames, ref string ERR)
        {
            try {
                ManagementClass classInstance =
                    new ManagementClass("root\\CIMV2",
                    "Win32_LogicalDisk", null);

                // Obtain in-parameters for the method
                ManagementBaseObject inParams =
                    classInstance.GetMethodParameters("ExcludeFromAutochk");

                // Add the input parameters.
                inParams["LogicalDisk"] = a_DriveNames;

                // Execute the method and obtain the return values.
                ManagementBaseObject outParams =
                    classInstance.InvokeMethod("ExcludeFromAutochk", inParams, null);

                // List outParams
                string callback = "Out parameters:\n" + "ReturnValue: " + outParams["ReturnValue"];
                return callback;
            } catch (ManagementException err) {
                ERR = "An error occurred while trying to execute the WMI method: " + err.Message;
            }

            return null;
        }

        public void Defrag(string a_ComputerName, string a_DriveName, ref string ERR)
        {
            try
            {

                ManagementScope scope;

                // If the computer is not local
                if (!a_ComputerName.Equals("localhost", StringComparison.OrdinalIgnoreCase) && !a_ComputerName.Equals("127.0.0.1")) {
                    ConnectionOptions options = new ConnectionOptions();
                    options.Username = "";
                    options.Password = "";
                    options.Authority = "ntlmdomain:DOMAIN";
                    scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", a_ComputerName), options);
                } else
                    scope = new ManagementScope(String.Format("\\\\{0}\\root\\CIMV2", a_ComputerName), null);

                scope.Connect();

                // Get the Win32_Volume objects with our specified name (C:\\\\)
                string WQL = String.Format("SELECT * FROM Win32_Volume Where Name='{0}'", a_DriveName);
                ObjectQuery query = new ObjectQuery(WQL);
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(scope, query);

                //Cycle through and invoke the defrag method
                foreach (ManagementObject ClassInstance in Searcher.Get()) {
                    ManagementBaseObject inParams = ClassInstance.GetMethodParameters("Defrag");
                    inParams["Force"] = true;
                    ManagementBaseObject outParams = ClassInstance.InvokeMethod("Defrag", inParams, null);

                    if(!outParams["ReturnValue"].ToString().Equals("0"))
                        ERR = String.Format("{0}: {1}\n{2}: {3}", "DefragAnalysis", outParams["DefragAnalysis"], "ReturnValue", outParams["ReturnValue"]);

                }


            } catch (ManagementException err) {
                ERR = "An error occurred while trying to execute the WMI method: " + err.Message + "\n" + err.StackTrace;
            }
        }

        public string DefragAnalysis(string a_DriveName, ref string ERR)
        {
            try {
                ManagementObject classInstance = 
                    new ManagementObject("root\\CIMV2",
                    String.Format("Win32_Volume.DeviceID='{0}'", a_DriveName),
                    null);


                // Execute the method and obtain the return values.
                ManagementBaseObject outParams = 
                    classInstance.InvokeMethod("DefragAnalysis", null, null);

                // List outParams
                string callback = "Out parameters:\n" + "ReturnValue: " + outParams["ReturnValue"];
                return callback;
            } catch (ManagementException err) {
                ERR = "An error occurred while trying to execute the WMI method: " + err.Message;
            }

            return null;
        }

        #endregion
    }
}
