using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace S33M3DXEngine.Debug
{
    public class StopWatchWrapping
    {
        public class dataStruct
        {
            public double TimeSpend;
            public string Informations;

            public override string ToString()
            {
                return Informations + " : " + TimeSpend + "s";
            }
        }

        #region Private Variables
        private long _fromTime;
        private List<dataStruct> _bufferedTimes;
        private int _arrayIndex = 0;
        #endregion

        #region Public Properties
        public List<dataStruct> BufferedTimes
        {
            get { return _bufferedTimes; }
            set { _bufferedTimes = value; }
        }

        public int ArrayIndex
        {
            get { return _arrayIndex; }
            set { _arrayIndex = value; }
        }

        public bool isEnabled { get; set; }
        #endregion

        public StopWatchWrapping(int bufferSize)
        {
            _bufferedTimes = new List<dataStruct>(bufferSize);
            for(int i =0; i < bufferSize;i++) _bufferedTimes.Add(new dataStruct());
        }

        #region Public Methods


        public void Reset()
        {
            _bufferedTimes.Clear();
            _arrayIndex = 0;
        }

        public void StartMeasure(bool doCatpure, string Informations)
        {
            if (isEnabled == false || doCatpure) return;
            _bufferedTimes[_arrayIndex].Informations = Informations;
            _fromTime = Stopwatch.GetTimestamp();
        }

        public void StopMeasure()
        {
            if (isEnabled == false) return;
            long toTime = Stopwatch.GetTimestamp();
            _bufferedTimes[_arrayIndex].TimeSpend = (toTime - _fromTime) / (double)(Stopwatch.Frequency);
            _arrayIndex++;
            if (_arrayIndex >= _bufferedTimes.Count) _arrayIndex = 0;
        }
        #endregion

        #region Private Methods
        #endregion

    }
}
