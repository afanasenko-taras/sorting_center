using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

public struct Box
{
    public double Width;
    public double Height;
    public double Depth;

    public Box(double w, double h, double d)
    {
        Width = w;
        Height = h;
        Depth = d;
    }

    public double Volume => Width * Height * Depth;
}

public struct Placement
{
    public Box Box;
    public double X;
    public double Y;
    public double Z;
}

public class PackingResult
{
    public List<Placement> Placements = new List<Placement>();
    public double TotalHeight;
    public double Density;
    public bool Success;
    public string Error;
    public TimeSpan ElapsedTime;
}

public class DensityPacker3D
{
    private const int MaxOrientations = 6;
    private const double Epsilon = 0.001;
    private Stopwatch _timer;
    private Action<string> _progress;

    public PackingResult Pack(List<Box> boxes, double palletW, double palletD, Action<string> progress = null)
    {
        _timer = Stopwatch.StartNew();
        _progress = progress;

        var result = new PackingResult();
        var sortedBoxes = SortBoxes(boxes);
        var freeSpaces = new List<Space> { new Space(0, 0, 0, palletW, palletD, double.MaxValue) };

        Report("Starting 3D packing...");

        foreach (var box in sortedBoxes)
        {
            var bestPlace = FindBestPlacement(box, freeSpaces, palletW, palletD);

            if (!bestPlace.HasValue)
            {
                result.Error = $"Can't place box {box.Width}x{box.Height}x{box.Depth}";
                result.Success = false;
                return result;
            }

            var placement = bestPlace.Value;
            result.Placements.Add(placement);
            UpdateFreeSpaces(freeSpaces, placement, palletW, palletD);

            result.TotalHeight = Math.Max(
                result.TotalHeight,
                placement.Z + placement.Box.Height
            );
        }

        FinalizeResult(result, boxes, palletW, palletD);
        return result;
    }

    private List<Box> SortBoxes(List<Box> boxes)
    {
        return boxes
            .OrderByDescending(b => b.Volume)
            .ThenByDescending(b => Math.Max(Math.Max(b.Width, b.Height), b.Depth))
            .ToList();
    }

    private Placement? FindBestPlacement(Box originalBox, List<Space> freeSpaces,
        double maxW, double maxD)
    {
        foreach (var space in freeSpaces.OrderBy(s => s.Z))
        {
            foreach (var box in GenerateOrientations(originalBox))
            {
                if (ValidatePlacement(space, box, maxW, maxD))
                {
                    return new Placement
                    {
                        Box = box,
                        X = space.X,
                        Y = space.Y,
                        Z = space.Z
                    };
                }
            }
        }
        return null;
    }

    private IEnumerable<Box> GenerateOrientations(Box box)
    {
        yield return new Box(box.Width, box.Height, box.Depth);
        yield return new Box(box.Width, box.Depth, box.Height);
        yield return new Box(box.Height, box.Width, box.Depth);
        yield return new Box(box.Height, box.Depth, box.Width);
        yield return new Box(box.Depth, box.Width, box.Height);
        yield return new Box(box.Depth, box.Height, box.Width);
    }

    private bool ValidatePlacement(Space space, Box box, double maxW, double maxD)
    {
        return box.Width <= space.W + Epsilon &&
               box.Depth <= space.D + Epsilon &&
               space.X + box.Width <= maxW + Epsilon &&
               space.Y + box.Depth <= maxD + Epsilon &&
               space.Z + box.Height <= space.MaxHeight + Epsilon;
    }

    private void UpdateFreeSpaces(List<Space> spaces, Placement placed,
        double maxW, double maxD)
    {
        var newSpaces = new List<Space>();

        foreach (var space in spaces)
        {
            if (Intersects(space, placed))
            {
                SplitSpace(space, placed, newSpaces, maxW, maxD);
            }
            else
            {
                newSpaces.Add(space);
            }
        }

        spaces.Clear();
        spaces.AddRange(newSpaces
            .Where(s => s.W > Epsilon && s.D > Epsilon && s.H > Epsilon)
            .OrderBy(s => s.Z)
            .ThenBy(s => s.X + s.Y));
    }

    private bool Intersects(Space space, Placement placed)
    {
        return !(placed.X >= space.X + space.W ||
                 placed.X + placed.Box.Width <= space.X ||
                 placed.Y >= space.Y + space.D ||
                 placed.Y + placed.Box.Depth <= space.Y ||
                 placed.Z >= space.Z + space.H ||
                 placed.Z + placed.Box.Height <= space.Z);
    }

    private void SplitSpace(Space original, Placement placed, List<Space> spaces,
        double maxW, double maxD)
    {
        // Горизонтальное разбиение справа
        if (placed.X + placed.Box.Width < original.X + original.W - Epsilon)
        {
            spaces.Add(new Space(
                placed.X + placed.Box.Width,
                original.Y,
                original.Z,
                original.W - placed.Box.Width,
                original.D,
                original.H
            ));
        }

        // Вертикальное разбиение сверху
        if (placed.Y + placed.Box.Depth < original.Y + original.D - Epsilon)
        {
            spaces.Add(new Space(
                original.X,
                placed.Y + placed.Box.Depth,
                original.Z,
                placed.Box.Width,
                original.D - placed.Box.Depth,
                original.H
            ));
        }

        // Вертикальное разбиение по высоте
        if (placed.Z + placed.Box.Height < original.Z + original.H - Epsilon)
        {
            spaces.Add(new Space(
                original.X,
                original.Y,
                placed.Z + placed.Box.Height,
                original.W,
                original.D,
                original.H - placed.Box.Height
            ));
        }
    }

    private void FinalizeResult(PackingResult result, List<Box> boxes,
        double palletW, double palletD)
    {
        result.Success = true;
        result.ElapsedTime = _timer.Elapsed;

        double totalVolume = boxes.Sum(b => b.Volume);
        double containerVolume = palletW * palletD * result.TotalHeight;
        result.Density = totalVolume / containerVolume;

        Report($"Packing completed in {result.ElapsedTime.TotalSeconds:0.00}s");
        Report($"Total height: {result.TotalHeight:0.##}");
        Report($"Density: {result.Density:P2}");
    }

    private void Report(string message)
    {
        _progress?.Invoke($"[{_timer.Elapsed.TotalSeconds:0.00}s] {message}");
    }

    private struct Space
    {
        public double X;
        public double Y;
        public double Z;
        public double W;
        public double D;
        public double H;
        public double MaxHeight => Z + H;

        public Space(double x, double y, double z, double w, double d, double h)
        {
            X = x;
            Y = y;
            Z = z;
            W = w;
            D = d;
            H = h;
        }
    }
}


class Program()
{
    public static void  Main()
    {
        var boxes = new List<Box>
{
    new Box(2, 3, 4),
    new Box(1, 2, 5),
    new Box(3, 3, 3),
    new Box(2, 2, 2)
};

        var packer = new DensityPacker3D();
        var result = packer.Pack(boxes, 5, 4, msg => Console.WriteLine(msg));

        if (result.Success)
        {
            Console.WriteLine($"\nTotal height: {result.TotalHeight}");
            Console.WriteLine($"Density: {result.Density:P2}");
            Console.WriteLine($"Time: {result.ElapsedTime.TotalMilliseconds} ms");

            foreach (var p in result.Placements)
            {
                Console.WriteLine($"Box {p.Box.Width}x{p.Box.Height}x{p.Box.Depth} " +
                    $"at ({p.X}, {p.Y}, {p.Z})");
            }
        }
    }
}