using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using OpenHardwareMonitor.Hardware;
using System.Xml;
using System.IO;
using HelperLibrary;

namespace OHMWrapper
{
    public class OHW
    {
        private static OHW m_Instance;
        public static OHW Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new OHW();

                return m_Instance;
            }
        }
        private OHW()
        {
            m_Instance = this;
        }

        public string GetHardwareDiagnostics(ref string ERR)
        {
            MySettings settings = new MySettings(new Dictionary<string, string>
            {
                { "/intelcpu/0/temperature/0/values", "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Iu6//MH37x79i9/+NX6N3/TJm9/5f/01fw1+fosnv+A/+OlfS37/jZ/s/Lpv9fff6Ml/NTef/yZPnozc5679b+i193//TQZ+/w2Dd+P9/sZeX/67v/GTf/b3iP3u4/ObBL//73+i+f039+D8Zk/+xz/e/P6beu2TQZju8yH8f6OgzcvPv/U3/Rb8+z/0f/9b/+yfaOn8079X6fr6Cws7ln/iHzNwflPv99/wyS/+xY4+v/evcJ+733+jJ5//Cw7/4ndy9Im3+U2e/Fbnrk31C93vrt/fyPvdb+N//hsF7/4/AQAA//9NLZZ8WAIAAA==" },
                { "/intelcpu/0/load/0/values", "H4sIAAAAAAAEAOy9B2AcSZYlJi9tynt/SvVK1+B0oQiAYBMk2JBAEOzBiM3mkuwdaUcjKasqgcplVmVdZhZAzO2dvPfee++999577733ujudTif33/8/XGZkAWz2zkrayZ4hgKrIHz9+fB8/Iu6//MH37x79i9++mpwcv/md/9df89egZ/xX/ym/5y/4D37618Lv7ya//u+58+u+5d9/z7/5t/w9/6u5fP5bH/6av+eTkXyefXxp26ONaf/v/dG/sf39D/rvnv4e5vc/0IP56/waK/vuHzf5I38P8/tv+mv8Rbb9f0pwTF9/zr/1X9vP/8I//+/6Pf7Z30N+/zdf/HX29zd/859q4aCNP5b//U+U3/+7f+zXOjZwfqvDX/V7/o9/vPz+a1G/pv0f+fGlhfk7eZ//N3/0v28//5X0u/n8Cxq7+f1X/tHft20A5x8a/W5/02+BP36Nf+j/nv8XfzrT+c2//Ob4p3+vktvUhNs/+xcWikP6e/4T/5jS5M8/sL8vP/5ff49f/Ivl9//sHzv6PX/vXyG//9R/94/9HuZ34P/5vyC//3W/5e/1exa/k+Bw4bUBnU2bP4Xg/1bn0uafeTH6PatfKL//N3/0t2y/gG9+/8+IzqYNxmU+/+jwX7afY67/nwAAAP//GYSA31gCAAA=" },
            });

            Computer myComputer = new Computer(settings)
            {
                MainboardEnabled = true,
                CPUEnabled = true,
                RAMEnabled = true,
                GPUEnabled = true,
                FanControllerEnabled = true,
                HDDEnabled = true
            };

            // Open get all the hardware diagnostics and store it in OHM DB
            myComputer.Open();

            // Cycle through all available hardware diagnostics and populate an XML
            try
            {
                XmlWriterSettings wSettings = new XmlWriterSettings();
                wSettings.Indent = true;
                MemoryStream ms = new MemoryStream();
                XmlWriter xw = XmlWriter.Create(ms, wSettings);
                xw.WriteStartDocument();

                // Write root
                xw.WriteStartElement("HardwareDiagnostics");

                foreach (var hardwareItem in myComputer.Hardware)
                {
                    hardwareItem.Update();

                    if (hardwareItem.Name.Contains("processor") || hardwareItem.Name.Contains("Processor"))
                        xw.WriteStartElement("Processor");
                    else
                        xw.WriteStartElement(HelperFunctions.ExceptChars(hardwareItem.Name, new[] { ' ', '/', '2' }));

                    xw.WriteStartAttribute("Name");
                    xw.WriteString(HelperFunctions.ExceptChars(hardwareItem.Name, new[] { ' ', '/', '2' }));
                    xw.WriteEndAttribute();

                    if (hardwareItem.SubHardware.Length <= 0)
                    {
                        foreach (var sensor in hardwareItem.Sensors)
                        {
                            //xw.WriteStartElement(HelperFunctions.ExceptChars(sensor.Identifier.ToString(), new[] {' ', '#', '/'}));
                            //xw.WriteEndElement();
                            xw.WriteStartElement(HelperFunctions.CleanHardwareIdentifiers(sensor.Identifier.ToString()));
                            xw.WriteString(sensor.Value.HasValue ? sensor.Value.Value.ToString() : "no value");
                            xw.WriteEndElement();
                        }

                    }
                    else
                    {
                        foreach (IHardware subHardware in hardwareItem.SubHardware)
                        {
                            subHardware.Update();


                            xw.WriteStartElement(HelperFunctions.ExceptChars(subHardware.Name, new[] { ' ' }));

                            foreach (var sensor in subHardware.Sensors)
                            {
                                xw.WriteStartElement(HelperFunctions.CleanHardwareIdentifiers(sensor.Identifier.ToString()));
                                xw.WriteString(sensor.Value.HasValue ? sensor.Value.Value.ToString() : "no value");
                                xw.WriteEndElement();
                            }

                            xw.WriteEndElement();
                        }
                    }

                    //End the Hardware Element
                    xw.WriteEndElement();
                }

                //End the root Element
                xw.WriteEndElement();

                //Close the document
                xw.WriteEndDocument();
                xw.Flush();

                Byte[] buffer = new Byte[ms.Length];
                buffer = ms.ToArray();
                return System.Text.Encoding.UTF8.GetString(buffer);
            }
            catch (XmlException e)
            {
                ERR = "There was an error generating the XML for OHM's Disgnostics: " + e.ToString();
            }

            return null;
        }
    }
}
/*

                    //Create SMART Data XML
                    XmlWriterSettings wSettings = new XmlWriterSettings();
                    wSettings.Indent = true;
                    MemoryStream ms = new MemoryStream();
                    XmlWriter xw = XmlWriter.Create(ms, wSettings);
                    xw.WriteStartDocument();

                    // Write root
                    xw.WriteStartElement("S.M.A.R.T");

                    //Get the dictonary of SMART attributes
                    var dict = new SmartData(arrVendorSpecific);
                    foreach (var attribute in dict.Attributes)
                    {
                        //Start element
                        xw.WriteStartElement(attribute.AttributeType.ToString());

                        //Write Attribute
                        xw.WriteStartAttribute("FailureImminent");
                        xw.WriteString(attribute.FailureImminent.ToString());
                        xw.WriteEndAttribute();

                        //Write Value of Element
                        xw.WriteString(attribute.Value.ToString());

                        xw.WriteEndElement();
                    }

                    xw.WriteEndElement();

                    //Close the document
                    xw.WriteEndDocument();
                    xw.Flush();

                    Byte[] buffer = new Byte[ms.Length];
                    buffer = ms.ToArray();
                    return System.Text.Encoding.UTF8.GetString(buffer);

*/