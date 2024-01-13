namespace net.rs64.TexTransTool
{
    [System.Serializable]
    public class TTTException : System.Exception
    {
        public object[] AdditionalMessage;
        public TTTException(string message, params object[] additionalMessage) : base(message)
        {
            AdditionalMessage = additionalMessage;
        }
        public TTTException(string message, System.Exception inner) : base(message, inner) { }
    }
}