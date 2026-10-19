using System;
using System.Threading.Tasks;
using Mono.Addins;
using Pinta.Core;
using Cairo;

[assembly: Addin("BetterOverlayPlugin", "1.0")]
[assembly: AddinDependency("Pinta", "3.1.2")]

namespace CustomOverlayPlugin
{
    [Extension]
    public sealed class BetterOverlayExtension : IExtension
    {
        public void Initialize()
        {
            PintaCore.Effects.RegisterEffect<CustomOverlayEffect>(new CustomOverlayEffect());
        }

        public void Uninitialize()
        {
            PintaCore.Effects.UnregisterInstanceOfEffect<CustomOverlayEffect>();
        }
    }

    public class CustomOverlayEffect : BaseEffect
    {
        public override bool IsTileable => true;
        public override string Icon => "Menu.Effects.Photo.Glow.png";
        public override string Name => "Better Overlay";
        public override string EffectMenuCategory => "Photo";
        public override bool IsConfigurable => true;

        public new CustomOverlayData EffectData => (CustomOverlayData)base.EffectData!;

        private ColorBgra[]? backgroundLayerCache;
        private int cacheWidth;
        private int cacheHeight;

        public CustomOverlayEffect()
        {
            base.EffectData = new CustomOverlayData();
        }

        //ALL THIS IS BROKEN AT THIS POINT, NOTHING IS WORKING - DEBUGGING CONTINUES 
        /*public override Task<bool> LaunchConfiguration()
        {
            var doc = PintaCore.Workspace.ActiveDocument;
            int currentLayerIndex = doc.Layers.CurrentUserLayerIndex;

            if (currentLayerIndex > 0)
            {
                var bottomLayer = doc.Layers[currentLayerIndex - 1];
                ImageSurface bgSurface = bottomLayer.Surface;
                
                cacheWidth = bgSurface.Width;
                cacheHeight = bgSurface.Height;
                
                ReadOnlySpan<ColorBgra> bgPixels = bgSurface.GetReadOnlyPixelData();
                backgroundLayerCache = bgPixels.ToArray(); 
            }
            else
            {
                backgroundLayerCache = null;
            }

            return AddinUtilities.LaunchSimpleEffectDialog(
                PintaCore.Chrome, 
                PintaCore.Workspace, 
                this, 
                AddinManager.CurrentLocalizer
            );
        }

        public override void Render(ImageSurface src, ImageSurface dest, ReadOnlySpan<RectangleI> rois)
        {
            ReadOnlySpan<ColorBgra> top_layer_data = src.GetReadOnlyPixelData();
            Span<ColorBgra> dest_data = dest.GetPixelData();
            int src_width = src.Width;

            double midpoint = EffectData.BasePoint / 255.0;
            bool hasBackground = backgroundLayerCache != null && src.Width == cacheWidth && src.Height == cacheHeight;

            foreach (ref readonly var rect in rois)
            {
                for (int y = rect.Y; y < rect.Bottom; ++y)
                {
                    int row_offset = y * src_width;
                    for (int x = rect.X; x < rect.Right; ++x)
                    {
                        int pixel_index = row_offset + x;
                       /* ColorBgra topPixel = top_layer_data[pixel_index];

                        if (topPixel.A == 0)
                        {
                            dest_data[pixel_index] = ColorBgra.FromBgra(0, 0, 0, 0);
                            continue;
                        }

                        ColorBgra basePixel = hasBackground ? backgroundLayerCache![pixel_index] : ColorBgra.FromBgra(128, 128, 128, 255);

                        double alphaFactor = topPixel.A / 255.0;
                        byte trueR = (byte)Math.Clamp(topPixel.R / alphaFactor, 0, 255);
                        byte trueG = (byte)Math.Clamp(topPixel.G / alphaFactor, 0, 255);
                        byte trueB = (byte)Math.Clamp(topPixel.B / alphaFactor, 0, 255);

                        double baseAlphaFactor = basePixel.A / 255.0;
                        byte baseR = baseAlphaFactor > 0 ? (byte)Math.Clamp(basePixel.R / baseAlphaFactor, 0, 255) : (byte)0;
                        byte baseG = baseAlphaFactor > 0 ? (byte)Math.Clamp(basePixel.G / baseAlphaFactor, 0, 255) : (byte)0;
                        byte baseB = baseAlphaFactor > 0 ? (byte)Math.Clamp(basePixel.B / baseAlphaFactor, 0, 255) : (byte)0;

                        byte rBlend = CalculateCustomOverlay(trueR, baseR, midpoint);
                        byte gBlend = CalculateCustomOverlay(trueG, baseG, midpoint);
                        byte bBlend = CalculateCustomOverlay(trueB, baseB, midpoint);

                        byte premultR = (byte)((rBlend * topPixel.A + 127) / 255);
                        byte premultG = (byte)((gBlend * topPixel.A + 127) / 255);
                        byte premultB = (byte)((bBlend * topPixel.A + 127) / 255); 

                        dest_data[pixel_index] = ColorBgra.FromBgra(0, 0, 0, 255);
                    }
                }
            }
        } */ 

        private byte CalculateCustomOverlay(byte overlayChannel, byte baseChannel, double midpoint)
        {
            double a = overlayChannel / 255.0; 
            double b = baseChannel / 255.0;   
            double result;

            if (b < midpoint)
            {
                result = (a * b) / midpoint;
            }
            else
            {
                result = 1.0 - ((1.0 - a) * (1.0 - b)) / (1.0 - midpoint);
            }

            return (byte)Math.Clamp(result * 255.0, 0, 255);
        }
    }

    public class CustomOverlayData : EffectData
    {
        private int basePoint = 128;

        [Caption("Overlay Midpoint")]
        [MinimumValue(1), MaximumValue(254)]
        public int BasePoint
        {
            get => basePoint;
            set
            {
                if (basePoint != value)
                {
                    basePoint = value;
                    FirePropertyChanged(nameof(BasePoint));
                }
            }
        }
    }
}
