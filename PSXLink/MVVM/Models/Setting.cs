namespace PSXLink.MVVM.Models
{
    public class Setting
    {
        private static Setting? _instance;
        private static readonly object _lock = new();

        public static Setting Instance()
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance = new();
                }
            }

            return _instance;
        }

        public bool CheckVersion { get; set; } = true;
        public bool CheckOnly { get; set; } = true;
    }
}
