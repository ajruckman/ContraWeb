using System.Collections.Generic;
using Superset.Web.Resources;

namespace Web
{
    public class Configuration
    {
        public readonly List<ResourceManifest> ResourceManifests = new List<ResourceManifest>
        {
            Superset.Web.ResourceManifests.LocalCSS,
            
            Subsegment.Resources.ResourceManifest,

            ColorSet.ResourceManifests.Globals,

            FontSet.ResourceManifests.Inter,
            FontSet.ResourceManifests.JetBrainsMono,

            FlareSelect.Resources.FocusScript,
            FT3.ResourceManifests.FlareTables,
            
            Fundament.Resources.ResourceManifest,
            Rudiment.Resources.ResourceManifest,
            
            ShapeSet.ResourceManifests.Composite,
        };
    }
}