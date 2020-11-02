namespace Neraloc.Opc.Kepserver
{
    using System;

    public class KepserverWriteException : Exception
    {
        public KepserverWriteException(string codes) : base($"Write exception - Codes:{Environment.NewLine}{codes}") { }
    }

}
