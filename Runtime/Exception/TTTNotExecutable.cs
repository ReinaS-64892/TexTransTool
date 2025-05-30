using net.rs64.TexTransCore;

namespace net.rs64.TexTransTool
{
    [System.Serializable]
    public class TTTNotExecutable : TTException
    {
        public TTTNotExecutable(params object[] additionalMessage) : base("Not Executable", additionalMessage) { }
        public TTTNotExecutable(string message, params object[] additionalMessage) : base(message, additionalMessage) { }
        public TTTNotExecutable(string message, System.Exception inner) : base(message, inner) { }
    }
}
