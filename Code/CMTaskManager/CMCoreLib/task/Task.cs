using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Threading;

namespace CMCore.task
{

    public class ScheduledTask
    {

        #region Public Properties
        public enum TCurrentState
        {
            EReady,
            EWorking,
            EFinish
        }

        
        public string TaskName { get; set; }

        public int Id { get; set; }


        public string MinPage { get; set; }

        public string ETaskSource { get; set; }

        public TCurrentState CurrentState { get; set; }

        public DateTime TaskDate { get; set; }

        public DateTime LastInUse { get; set; }

        public string CityName { get; set; }

        #endregion

        #region Public Functions

        public ScheduledTask(int Id, string ETaskSource, string TaskName, DateTime TaskDate, string CityName, string MinPage)
        {
            this.Id = Id;
            this.ETaskSource = ETaskSource;
            this.CurrentState = TCurrentState.EReady;
            this.TaskName = TaskName;
            this.TaskDate = TaskDate;
            this.CityName = CityName;
            this.MinPage = MinPage;

        }


        #endregion
    }
}
