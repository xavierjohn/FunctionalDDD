using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace IconGenerator;

/// <summary>
/// Generates a railway-oriented programming icon for the FunctionalDDD NuGet packages.
/// Creates a 128x128 PNG with railway tracks representing the dual-track nature of ROP.
/// </summary>
class Program
{
    static void Main(string[] args)
    {
        const int size = 128;
        const string outputPath = "../../icon.png";

        using var bitmap = new Bitmap(size, size);
        using var graphics = Graphics.FromImage(bitmap);

        // Enable high-quality rendering
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

        // Clear background (transparent)
        graphics.Clear(Color.Transparent);

        // Create gradient colors (Purple to Blue - representing DDD and .NET)
        using var gradientBrush = new LinearGradientBrush(
            new Rectangle(0, 0, size, size),
            Color.FromArgb(91, 44, 111),    // #5B2C6F - Purple
            Color.FromArgb(0, 120, 212),    // #0078D4 - Blue
            LinearGradientMode.Horizontal);

        // Draw railway tracks
        DrawRailwayTracks(graphics, gradientBrush, size);

        // Draw a subtle diverging point (representing success/error paths)
        DrawDivergingPoint(graphics, gradientBrush, size);

        // Save as PNG with transparency
        var fullPath = Path.GetFullPath(outputPath);
        bitmap.Save(fullPath, ImageFormat.Png);

        Console.WriteLine($"? Icon generated successfully: {fullPath}");
        Console.WriteLine($"?? Size: 128x128 pixels");
        Console.WriteLine($"?? Format: PNG with transparency");
        Console.WriteLine($"?? Colors: Purple (#5B2C6F) to Blue (#0078D4) gradient");
    }

    static void DrawRailwayTracks(Graphics g, Brush brush, int size)
    {
        const float trackWidth = 8f;
        const float spacing = 24f;
        
        // Calculate positions for dual tracks (success and error paths)
        float centerY = size / 2f;
        float topTrackY = centerY - spacing;
        float bottomTrackY = centerY + spacing;

        // Draw top track (success path)
        DrawTrack(g, brush, size, topTrackY, trackWidth);

        // Draw bottom track (error path)
        DrawTrack(g, brush, size, bottomTrackY, trackWidth);

        // Draw railway sleepers (ties) to connect the tracks
        DrawSleepers(g, brush, size, topTrackY, bottomTrackY, trackWidth);
    }

    static void DrawTrack(Graphics g, Brush brush, int size, float y, float width)
    {
        const float margin = 10f;
        var pen = new Pen(brush, width)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        // Main rail line
        g.DrawLine(pen, margin, y, size - margin, y);
    }

    static void DrawSleepers(Graphics g, Brush brush, int size, float topY, float bottomY, float width)
    {
        const float margin = 15f;
        const float sleeperSpacing = 18f;
        const float sleeperWidth = 3f;

        var pen = new Pen(brush, sleeperWidth)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        // Draw vertical sleepers connecting the tracks
        for (float x = margin + sleeperSpacing; x < size - margin; x += sleeperSpacing)
        {
            g.DrawLine(pen, x, topY, x, bottomY);
        }
    }

    static void DrawDivergingPoint(Graphics g, Brush brush, int size, float trackWidth = 6f)
    {
        // Draw a subtle arrow or diverging point at the left side
        // This represents the branching nature of ROP (success/error paths)
        const float margin = 15f;
        const float centerY = 64f;
        
        var pen = new Pen(brush, trackWidth)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        // Draw a subtle branching indicator
        PointF[] arrow = 
        [
            new PointF(margin, centerY),
            new PointF(margin + 20, centerY - 24),
            new PointF(margin, centerY),
            new PointF(margin + 20, centerY + 24)
        ];

        g.DrawLines(pen, arrow);
    }
}
