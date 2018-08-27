using System;

namespace KeikoObfuscator
{
    public interface ILogOutput
    {
        void ReportError(Exception exception);

        void WriteMessage(string message);
    }
}