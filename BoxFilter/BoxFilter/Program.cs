using System;
using System.Collections.Generic;

public static class Solution
{
    // Box Blur - each pixel x,y in the source image should be the average
    // of that pixel and the pixels around it.
    //
    // Pixels are greyscale, where 0.0f is black and 1.0f is white
    //
    // Example: 
    //                              Blur value for pixel at        
    //                              index 6 is the average     Result at index 6
    //    Original image       =>   of surrounding pixels =>      = (6/9) = 0.66...
    //                                   _____________              _____________
    //    0.0  1.0  1.0  1.0        0.0 |1.0  1.0  1.0|        0.0 |1.0  1.0  1.0|
    //    0.0  0.0  1.0  1.0        0.0 |0.0 (1.0) 1.0|        0.0 |0.0 (.67) 1.0| 
    //    0.0  0.0  0.0  1.0        0.0 |0.0__0.0__1.0|        0.0 |0.0__0.0__1.0|
    //    0.0  0.0  0.0  0.0        0.0  0.0  0.0  0.0         0.0  0.0  0.0  0.0
    //
    public static float[] BoxBlurImageMessy(float[] pixels, int width, int height, int radius)
    {
        float[] blurred = new float[pixels.Length];

        bool right = true;
        float sum = pixels[0];

        int count = 0;

        for (int x = 0; x <= radius; ++x)
        {
            // Don't include bottom row to not double count when entering outer loop below.
            for (int y = 0; y < radius; ++y)
            {
                int index = y * height + x;
                sum += pixels[index];
                count++;
            }
        }

        for (int y = 0; y < height; ++y)
        {
            {
                int y_remove = y - radius;
                if (y_remove >= 0)
                {
                    for (int x_remove = right ? 0 : width - radius - 1, increment = right ? 1 : -1; right ? x_remove <= radius : x_remove >= 0; x_remove += increment)
                    {
                        if (x_remove >= 0 && x_remove < width)
                        {
                            int index_remove = y_remove * height + x_remove;
                            sum -= pixels[index_remove];
                            count--;
                        }
                    }
                }
            }

            {
                int y_add = y + radius;
                if (y_add < height)
                {
                    for (int x_add = right ? 0 : width - radius - 1, increment = right ? 1 : -1; right ? x_add <= radius : x_add >= 0; x_add += increment)
                    {
                        if (x_add >= 0 && x_add < width)
                        {
                            int index_add = y_add * height + x_add;
                            sum += pixels[index_add];
                            count++;
                        }
                    }
                }
            }

            int x_first = (right ? 0 : width - 1);
            int index = y * height + x_first;
            blurred[index] = sum / (float)count;

            for (int x = right ? 1 : width - 2, increment = right ? 1 : -1; right ? (x < width) : (x >= 0); x += increment)
            {
                index = y * height + x;

                int x_remove = x - (right ? radius + 1 : -radius - 1);
                if (x_remove >= 0 && x_remove < width)
                {
                    for (int y_remove = y - radius - 1; y_remove <= y + radius; ++y_remove)
                    {
                        if (y_remove >= 0 && y_remove < height)
                        {
                            int index_remove = y_remove * height + x_remove;
                            sum -= pixels[index_remove];
                            count--;
                        }
                    }
                }

                int x_add = x + (right ? radius + 1 : -radius - 1);
                if (x_add >= 0 && x_add < width)
                {
                    for (int y_add = y - radius - 1; y_add <= y + radius; ++y_add)
                    {
                        if (y_add >= 0 && y_add < height)
                        {
                            int index_add = y_add * height + x_add;
                            sum += pixels[index_add];
                            count++;
                        }
                    }
                }

                blurred[index] = sum / (float)count;
            }

            right = !right;
        }

        return blurred;
    }

    public static void DisplayResult(float[] pixels, int width, int height)
    {
        // Simple display. Feel free to customize.
        for (int i = 0; i < pixels.Length; ++i)
        {
            if (i % width == 0)
            {
                Console.WriteLine();
            }
            Console.Write($"{pixels[i],5:F2} ");
        }
        Console.WriteLine();
    }

    public static float[] BoxBlurImageClean(float[] pixels, int width, int height, int radius, bool alternate = true)
    {
        var blurred = new float[pixels.Length];

        bool movingRight = true;

        float sum = 0;

        var previousBox = new HashSet<(int x, int y)>();
        var newBox = new HashSet<(int x, int y)>();

        // Iterate over y first for better locality.
        for (int y = 0; y < height; ++y)
        {
            // Iterate over x according to direction.
            for (int x = movingRight ? 0 : width - 1, increment = movingRight ? 1 : -1;
                movingRight ? (x < width) : (x >= 0);
                x += increment)
            {
                // Calculate positions in box at new position.
                newBox.Clear();
                for (int deltaY = -radius; deltaY <= radius; ++deltaY)
                {
                    for (int deltaX = -radius; deltaX <= radius; ++deltaX)
                    {
                        int boxX = x + deltaX;
                        int boxY = y + deltaY;
                        if (boxX >= 0 && boxX < width && boxY >= 0 && boxY < height)
                        {
                            newBox.Add((boxX, boxY));
                        }
                    }
                }

                // Remove position values no longer present in new box.
                foreach (var previousPosition in previousBox)
                {
                    if (!newBox.Contains(previousPosition))
                    {
                        sum -= pixels[previousPosition.y * height + previousPosition.x];
                    }
                }

                // Add position values newly present in new box.
                foreach (var newPosition in newBox)
                {
                    if (!previousBox.Contains(newPosition))
                    {
                        sum += pixels[newPosition.y * height + newPosition.x];
                    }
                }

                // Calculate new pixel value.
                blurred[y * height + x] = sum / (float)newBox.Count;

                // Swap the new and old boxes.
                var temp = previousBox;
                previousBox = newBox;
                newBox = temp;
            }

            // Switch direction if appropriate.
            if (alternate)
            {
                movingRight = !movingRight;
            }
        }

        return blurred;
    }

    private static void Main()
    {
        float[] testPixels = new float[] {
            0, 1, 1, 1,
            0, 0, 1, 1,
            0, 0, 0, 1,
            0, 0, 0, 0,
        };
        int width = 4;
        int height = 4;

        //var resultMessy = BoxBlurImageMessy(testPixels, width: width, height: height, radius: 1);
        var resultClean = BoxBlurImageClean(testPixels, width: width, height: height, radius: 1, alternate: false);
        var resultCleanAlternating = BoxBlurImageClean(testPixels, width: width, height: height, radius: 1, alternate: true);

        //DisplayResult(resultMessy, width: width, height: height);
        DisplayResult(resultClean, width: width, height: height);
        DisplayResult(resultCleanAlternating, width: width, height: height);
    }
}