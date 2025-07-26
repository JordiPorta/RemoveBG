using System;
using System.Drawing;
using System.Collections.Generic;
using System.IO;

class Program
{
    static void Main()
    {
        string inputPath = Path.Combine(AppContext.BaseDirectory, "../../../Profile.jpg");
        string outputPath = Path.Combine(AppContext.BaseDirectory, "../../../output.png");

        if (!File.Exists(inputPath))
        {
            Console.WriteLine("❌ Image not found: " + inputPath);
            return;
        }

        Bitmap bmp = new Bitmap(inputPath);
        Bitmap output = new Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

        // Copy original pixels
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                output.SetPixel(x, y, bmp.GetPixel(x, y));
            }
        }

        int tolerance = 20;
        int maxInternalRegionSize = 300; // You can adjust this

        // Check if a color is white within the tolerance
        bool IsWhite(Color c) =>
            c.R > 255 - tolerance &&
            c.G > 255 - tolerance &&
            c.B > 255 - tolerance;

        bool[,] visited = new bool[bmp.Width, bmp.Height];

        // Flood fill to detect connected white regions
        void FloodFill(Point start, List<Point> region, ref bool touchesEdge)
        {
            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(start);
            visited[start.X, start.Y] = true;

            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                region.Add(p);

                if (p.X == 0 || p.Y == 0 || p.X == bmp.Width - 1 || p.Y == bmp.Height - 1)
                    touchesEdge = true;

                foreach (var (dx, dy) in new[] { (-1, 0), (1, 0), (0, -1), (0, 1) })
                {
                    int nx = p.X + dx;
                    int ny = p.Y + dy;
                    if (nx >= 0 && ny >= 0 && nx < bmp.Width && ny < bmp.Height && !visited[nx, ny])
                    {
                        Color c = bmp.GetPixel(nx, ny);
                        if (IsWhite(c))
                        {
                            visited[nx, ny] = true;
                            queue.Enqueue(new Point(nx, ny));
                        }
                    }
                }
            }
        }

        // Process all white regions
        for (int y = 0; y < bmp.Height; y++)
        {
            for (int x = 0; x < bmp.Width; x++)
            {
                if (!visited[x, y] && IsWhite(bmp.GetPixel(x, y)))
                {
                    List<Point> region = new List<Point>();
                    bool touchesEdge = false;
                    FloodFill(new Point(x, y), region, ref touchesEdge);

                    // Remove external white regions and small internal ones
                    if (touchesEdge || region.Count <= maxInternalRegionSize)
                    {
                        foreach (var p in region)
                        {
                            output.SetPixel(p.X, p.Y, Color.FromArgb(0, 255, 255, 255)); // transparent
                        }
                    }
                }
            }
        }

        output.Save(outputPath, System.Drawing.Imaging.ImageFormat.Png);
        Console.WriteLine("✔ White background (external and internal) removed successfully.");
    }
}
