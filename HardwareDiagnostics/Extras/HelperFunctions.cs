using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HardwareDiagnostics
{
    public static class HelperFunctions
    {
        #region Properties

        #endregion

        #region Constructors

        #endregion

        #region Methods

        public static string ExceptChars(this string str, IEnumerable<char> toExclude)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < str.Length; i++)
            {
                char c = str[i];
                if (!toExclude.Contains(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static string CleanHardwareIdentifiers(string identifier)
        {
            string cleanIdentifier = "";

            if (identifier.IndexOf("load", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Load";
            else if (identifier.IndexOf("temperature", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Temperature";
            else if (identifier.IndexOf("clock", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Clock";
            else if (identifier.IndexOf("voltage", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Voltage";
            else if (identifier.IndexOf("control", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Control";
            else if (identifier.IndexOf("fan", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Fan";
            else if (identifier.IndexOf("data", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "Data";
            else if (identifier.IndexOf("gpu", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "GPU";
            else if (identifier.IndexOf("ram", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "RAM";
            else if (identifier.IndexOf("io", StringComparison.OrdinalIgnoreCase) >= 0)
                cleanIdentifier += "IO";

            else
                return identifier;
            /*
            if (identifier.EndsWith("0"))
                cleanIdentifier += "_0";
            else if ((identifier.EndsWith("1")))
                cleanIdentifier += "_1";
            else if ((identifier.EndsWith("2")))
                cleanIdentifier += "_2";
            else if ((identifier.EndsWith("3")))
                cleanIdentifier += "_3";
            else if ((identifier.EndsWith("4")))
                cleanIdentifier += "_4";
            else if ((identifier.EndsWith("5")))
                cleanIdentifier += "_5";
            else if ((identifier.EndsWith("6")))
                cleanIdentifier += "_6";
            else if ((identifier.EndsWith("7")))
                cleanIdentifier += "_7";
            else if ((identifier.EndsWith("8")))
                cleanIdentifier += "_8";
            */
            return cleanIdentifier;
        }

        #endregion


    }
}
