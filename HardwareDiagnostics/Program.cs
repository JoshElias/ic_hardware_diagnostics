using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace HardwareDiagnostics
{
    class Program
    {
        static void Main(string[] args)
        {
            string ERR = null;
            string directory = @"C:/Users/elia0028/Desktop/";
            string fullPath = String.Format(@"{0}{1}{2:dd-MM-yyyy}{3}", directory, "HS_", Time.Instance.UniversalTime.Date, ".xml");

            XDocument hardwareDiagnostics;
            if (File.Exists(fullPath))
                hardwareDiagnostics = XDocument.Load(fullPath);
            else
            {
                hardwareDiagnostics = new XDocument();
                XElement root = new XElement("HardwareDiagnostics");
                XAttribute date = new XAttribute("Date", String.Format("{0:dd-MM-yyyy}", DateTime.Now));
                root.Add(date);
                hardwareDiagnostics.Add(root);
            }

            // Generate our Master XML Diagnostics File
            XDocument generalDiagnostics = OHWManager.Instance.GetHardwareDiagnosticsXML(ref ERR);
            if (ERR != null) Console.WriteLine(ERR);
            XDocument smartDiagnostics = SMARTManager.Instance.CreateSMARTXML(ref ERR);
            if (ERR != null)    Console.WriteLine(ERR);
            
            //Combine and remove duplicates
            generalDiagnostics.Root.Add(smartDiagnostics.Root.Elements());
            XElement hardwareDiagnostic = new XElement("HardwareDiagnostic");

            XElement time = new XElement("Time", String.Format("{0:hh-mm-ss-tt}", Time.Instance.UniversalTime));
            
            hardwareDiagnostic.Add(time);
            hardwareDiagnostic.Add(generalDiagnostics.Root.Elements());
            hardwareDiagnostics.Root.Add(hardwareDiagnostic);
            hardwareDiagnostics.Save(fullPath);
        }
    }
}