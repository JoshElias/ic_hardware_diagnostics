using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using SMART;

namespace HardwareDiagnostics
{
    public class HardwareInfo
    {
        #region Fields

        private static HardwareInfo m_Instance;
        public static HardwareInfo Instance
        {
            get
            {
                if (m_Instance == null)
                    m_Instance = new HardwareInfo();

                return m_Instance;
            }
        }

        public readonly SMART.SMARTManager SMARTInfo;
       // public readonly CPU.CPU CPUInfo;

        #endregion

        #region Constructor

        private HardwareInfo()
        {
            m_Instance = this;

            SMARTInfo = SMART.SMARTManager.Instance;
            //CPUInfo = CPU.CPU.Instance;
        }

        #endregion
    }
}
