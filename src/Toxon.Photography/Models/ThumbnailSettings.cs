using System;

namespace Toxon.Photography.Models
{
    internal class ThumbnailSettings
    {
        public ThumbnailSettings(int? width, int? height, int quality)
        {
            if(!width.HasValue && !height.HasValue) throw new ArgumentException("One or both of Width and Height must be set");
            if(quality < 0 || quality > 100) throw new ArgumentOutOfRangeException(nameof(quality), "Quality must be between 0 and 100 (inclusive)");

            Width = width;
            Height = height;
            Quality = quality;
        }

        public int? Width { get; }
        public int? Height { get; }

        public int Quality { get; }

        public (int Width, int Height) CalculateDimensions(int fullWidth, int fullHeight)
        {
            if (Width.HasValue && Height.HasValue)
            {
                // TODO check ratio?
                return (Width.Value, Height.Value);
            }

            if (Width.HasValue)
            {
                var width = Width.Value;
                return (width, (int)decimal.Round(fullHeight * (width / (decimal)fullWidth)));
            }

            if (Height.HasValue)
            {
                var height = Height.Value;
                return ((int)decimal.Round(fullWidth * (height / (decimal)fullHeight)), height);
            }

            throw new InvalidOperationException("Width and/or Height must be set");
        }
    }
}