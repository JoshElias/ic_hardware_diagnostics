using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HardwareDiagnostics
{
    public class Time 
    {
        // The Singleton
        private static Time m_Instance;
        public static Time Instance
        {
            get {
             if (m_Instance == null) 
                 m_Instance = new Time();
               
                return m_Instance;
            }
        }

        // Constructor
        private Time()
        {

            m_TimeZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            m_LocalTime = DateTime.Now;
            m_UTCDateTime = m_LocalTime.ToUniversalTime();

            m_Instance = this;
        }

        // Members and Properties
        private readonly DateTime   m_UTCDateTime;
        public DateTime UniversalTime { get { return m_UTCDateTime; } }

        private readonly TimeZoneInfo m_TimeZone;
        public TimeZoneInfo TimeZone { get { return m_TimeZone; } }

        private readonly DateTime m_LocalTime;
        public DateTime LocalTime { get { return m_LocalTime; } }
    }
}
