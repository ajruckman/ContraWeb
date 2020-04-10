using System.Collections.Generic;
using Superset.Web.Resources;

namespace Web
{
    public class Configuration
    {
        public readonly List<ResourceManifest> ResourceManifests = new List<ResourceManifest>
        {
            Superset.Web.ResourceManifests.LocalCSS,
            Superset.Web.ResourceManifests.Listeners,
            Superset.Web.ResourceManifests.FocusElement,

            Subsegment.Resources.ResourceManifest,

            ColorSet.ResourceManifests.Globals,

            FontSet.ResourceManifests.Inter,
            FontSet.ResourceManifests.JetBrainsMono,

            // FlareSelect.Resources.FocusScript,
            FS3.ResourceManifests.FlareSelect,
            FT3.ResourceManifests.FlareTables,

            // Fundament.Resources.ResourceManifest,
            // Rudiment.Resources.ResourceManifest,

            ShapeSet.ResourceManifests.ShapeSet,
            ShapeSet.ResourceManifests.BlazorErrorUIStyle,

            // new ResourceManifest(nameof(Web), scriptsExternal: new[]
            // {
                // "https://momentjs.com/downloads/moment.min.js",
                // "https://cdn.jsdelivr.net/npm/chart.js@2.9.3/dist/Chart.min.js",

                // "https://www.amcharts.com/lib/4/core.js",
                // "https://www.amcharts.com/lib/4/charts.js",
                // "https://www.amcharts.com/lib/4/themes/dark.js",
                // "js/Stats.js"
            // })
        };
    }
}