namespace KWS
{
    public abstract class WaterPassCore
    {
        public string PassName;
        internal WaterSystem WaterInstance;
        protected bool IsInitialized;

        public abstract void Release();
    }
}