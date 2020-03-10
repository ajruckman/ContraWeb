using System.Collections.Generic;
using Superset.Web.Resources;

namespace Web
{
    public class Configuration
    {
        public readonly List<ResourceManifest> ResourceManifests = new List<ResourceManifest>
        {
            Superset.Web.ResourceManifests.LocalCSS
        }; 
    }
}