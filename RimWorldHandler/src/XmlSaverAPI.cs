using Verse;

namespace RimWorldHandler
{
    /// <summary>
    /// API Handler for Verse.XmlSaver
    /// </summary>
    public static class XmlSaverAPI
    {
        /// <summary>
        /// Saves an object to an xml file
        /// </summary>
        /// <param name="obj">The object to save</param>
        /// <param name="filepath">The xml file path</param>
        public static void SaveDataObject(object obj, string filepath)
        {
            DirectXmlSaver.SaveDataObject(obj, filepath);
        }
    }
}
