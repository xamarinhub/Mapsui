﻿using Mapsui.Layers;
using Mapsui.Providers.Wms;
using Mapsui.UI;

namespace Mapsui.Samples.Common.Maps.Data
{
    public class WmsSample : ISample
    {
        public string Name => "6. WMS";
        public string Category => "Data";

        public void Setup(IMapControl mapControl)
        {
            mapControl.Map = CreateMap();
        }

        public static Map CreateMap()
        {
            var map = new Map {CRS = "EPSG:28992"};
            // The WMS request needs a CRS
            map.Layers.Add(CreateLayer());
            return map;
        }

        public static ILayer CreateLayer()
        {
            return new ImageLayer("Windsnelheden (PDOK)") {DataSource = CreateWmsProvider()};
        }

        private static WmsProvider CreateWmsProvider()
        {
            const string wmsUrl = "https://geodata.nationaalgeoregister.nl/windkaart/wms?request=GetCapabilities";

            var provider = new WmsProvider(wmsUrl)
            {
                ContinueOnError = true,
                TimeOut = 20000,
                CRS = "EPSG:28992"
            };

            provider.AddLayer("windsnelheden100m");
            provider.SetImageFormat(provider.OutputFormats[0]);
            return provider;
        }
    }
}