using System;
using System.Collections.Generic;
using System.Text;

namespace EasyCommunication.SharedTypes
{
    /// <summary>
    /// Example for a serializable type
    /// </summary>
    [Serializable]
    public class SerializableImportantData
    {
        public string ImportantMessage { get; set; }
    }
}
