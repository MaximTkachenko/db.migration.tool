using System;

namespace DbMigrationTool
{
    public class DbMigration
    {
        public string Name { get; set; }
        public string Hash { get; set; }
        public DateTime AppliedDateTime { get; set; }
        public int DurationInSeconds { get; set; }
        public bool IsOk { get; set; }
        public int Mode { get; set; }
        public string Error { get; set; }
        public bool IsNew { get; private set; }

        public void SetResult(int duractioninSeconds, Exception ex = null)
        {
            IsNew = AppliedDateTime == DateTime.MinValue;
            DurationInSeconds = duractioninSeconds;
            AppliedDateTime = DateTime.Now;
            IsOk = ex == null;
            Error = ex == null ? null :  $"{ex.Message}\r\n{ex.StackTrace}";
        }
    }
}
