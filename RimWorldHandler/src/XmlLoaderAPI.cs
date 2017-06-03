using Verse;

namespace RimWorldHandler {

    /// <summary>
    /// API Handler for Verse.XmlLoader
    /// </summary>
    public static class XmlLoaderAPI {

        /// <summary>
        /// Gets an object from an xml file
        /// </summary>
        /// <typeparam name="T">The object type</typeparam>
        /// <param name="filepath">The xml file path</param>
        /// <param name="resolveCrossRefs">Set true to resolve cross refs[Optional](Default:false)</param>
        /// <returns></returns>
        public static T ItemFromXmlFile<T>(string filepath, bool resolveCrossRefs = false) where T : new() {
            return XmlLoader.ItemFromXmlFile<T>(filepath, resolveCrossRefs);
        }
    }
}