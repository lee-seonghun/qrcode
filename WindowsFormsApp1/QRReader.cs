using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class QRReader
    {
        // https://www.thonky.com/qr-code-tutorial/introduction
        // http://www.aishack.in/tutorials/scanning-qr-codes-verify-finder/
        public int skiprows = 5;

        public List<Point2f> possibleCenters = new List<Point2f>();
        public List<float> estimatedModuleSize = new List<float>();

        public bool find(Mat binary)
        {
            int[] stateCount = new int[5];
            int currentState = 0;

            for (int y = 0; y < binary.Rows; y += skiprows)
            {
                stateCount[0] = 0;
                stateCount[1] = 0;
                stateCount[2] = 0;
                stateCount[3] = 0;
                stateCount[4] = 0;
                currentState = 0;

                for (int x = 0; x < binary.Cols; x++)
                {
                    var pixel = binary.Get<byte>(y, x);
                    if (pixel == 0)
                    {
                        if ((currentState & 1) == 1)
                        {
                            currentState++;
                        }
                        stateCount[currentState]++;
                    }
                    else
                    {
                        // We got to a white pixel...
                        if ((currentState & 1) == 1)
                        {
                            stateCount[currentState]++;
                        }
                        else
                        {
                            // ...but, we were counting black pixels
                            if (currentState == 4)
                            {
                                if (checkRatio(stateCount))
                                {
                                    // This is where we do some more checks
                                    bool confirmed = handlePossibleCenter(binary, stateCount, y, x);
                                    if (possibleCenters.Count >= 3)
                                    {
                                        return true;
                                        //findAlignmentMarker(binary, )
                                    }
                                }
                                else
                                {
                                    currentState = 3;
                                    stateCount[0] = stateCount[2];
                                    stateCount[1] = stateCount[3];
                                    stateCount[2] = stateCount[4];
                                    stateCount[3] = 1;
                                    stateCount[4] = 0;
                                    continue;
                                }
                                currentState = 0;
                                stateCount[0] = 0;
                                stateCount[1] = 0;
                                stateCount[2] = 0;
                                stateCount[3] = 0;
                                stateCount[4] = 0;
                            }
                            else
                            {
                                // We still haven't go 'out' of the finder pattern yet
                                // So increment the state
                                // B->W transition
                                currentState++;
                                stateCount[currentState]++;
                            }
                        }
                    }
                }
            }
            //return possibleCenters.Count > 0;
            return false;
        }

        int computeDimension(Point2f tl, Point2f tr, Point2f bl, float moduleSize)
        {
            // The dimension is always a square of dimension 21*21, 25*25, 29*29, etc

            Point2f diff_top = tl - tr;
            Point2f diff_left = tl - bl;

            // Calculate the distance between the top-* and *-left points
            float dist_top = (float)Math.Sqrt(diff_top.DotProduct(diff_top));
            float dist_left = (float)Math.Sqrt(diff_left.DotProduct(diff_left));
            int width = (int)Math.Round(dist_top / moduleSize);
            int height = (int)Math.Round(dist_left / moduleSize);
            int dimension = ((width + height) / 2) + 7;
            switch (dimension % 4)
            {
                case 0:
                    dimension += 1;
                    break;

                case 1:
                    break;

                case 2:
                    dimension -= 1;
                    break;

                case 3:
                    dimension -= 2;
                    break;
            }
            return dimension;
        }
        public bool findAlignmentMarker(Mat img, out Mat output)
        {
            output = null;
            // Make sure we already found the finder patterns first
            if (possibleCenters.Count != 3)
            {
                return false;
            }
            Point2f ptTopLeft = possibleCenters[0];
            Point2f ptTopRight = possibleCenters[1];
            Point2f ptBottomLeft = possibleCenters[2];
            float moduleSize = (estimatedModuleSize[0] + estimatedModuleSize[1] + estimatedModuleSize[2]) / 3.0f;

            int dimension = computeDimension(ptTopLeft, ptTopRight, ptBottomLeft, moduleSize);

            if (dimension == 21)
            {
                // This is the smallest QR code and does not have have an alignment marker
                Point2f ptBottomRight = ptTopRight - ptTopLeft + ptBottomLeft;
                possibleCenters.Add(ptBottomRight);

                //Mat output;
                getTransformedMarker(dimension, img, out output);

                return true;
            }

            // TODO: Detect the alignment marker using a technique similar to previous posts
            return false;
        }
        private void getTransformedMarker(int dimension, 
            Mat binary, out Mat output)
        {
            List<Point2f> src = new List<Point2f>();

            src.Add(new Point2f(3.5f, 3.5f));
            src.Add(new Point2f(dimension - 3.5f, 3.5f));
            src.Add(new Point2f(3.5f, dimension - 3.5f));
            src.Add(new Point2f(dimension - 3.5f, dimension - 3.5f));

            Mat transform = Cv2.GetPerspectiveTransform(possibleCenters, src);
            output = new Mat();
            Cv2.WarpPerspective(binary, output, transform, new Size(dimension, dimension), InterpolationFlags.Nearest);
        }

        private bool handlePossibleCenter(Mat binary, int[] stateCount, int row, int col)
        {
            int stateCountTotal = 0;
            for (int i = 0; i < 5; i++)
            {
                stateCountTotal += stateCount[i];
            }

            float centerCol = centerFromEnd(stateCount, col);
            float centerRow = crossCheckVertical(binary, row, (int)centerCol, (int)stateCount[2], stateCountTotal);
            if(float.IsNaN(centerRow))
            {
                return false;
            }
            centerCol = crossCheckHorizontal(binary, (int)centerRow, (int)centerCol, stateCount[2], stateCountTotal);
            if(float.IsNaN(centerCol))
            {
                return false;
            }
            bool validPattern = crossCheckDiagonal(binary, (int)centerRow, (int)centerCol, stateCount[2], stateCountTotal);
            if (!validPattern)
            {
                return false;
            }
            Point2f ptNew = new Point2f() { X = centerCol, Y = centerRow };
            float newEstimatedModuleSize = stateCountTotal / 7.0f;
            bool found = false;
            int idx = 0;
            for (; idx < possibleCenters.Count; idx++)
            {
                Point2f pt = possibleCenters[idx];

                Point2f diff = pt - ptNew;
                float dist = (float)Math.Sqrt(diff.DotProduct(diff));

                // If the distance between two centers is less than 10px, they're the same.
                if (dist < newEstimatedModuleSize * 2)
                {
                    pt = pt + ptNew;
                    pt.X /= 2.0f; pt.Y /= 2.0f;
                    estimatedModuleSize[idx] = (estimatedModuleSize[idx] + newEstimatedModuleSize) / 2.0f;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                possibleCenters.Add(ptNew);
                estimatedModuleSize.Add(newEstimatedModuleSize);
            }
            return found;
        }

        private float centerFromEnd(int[] stateCount, int end)
        {
            return (float)(end - stateCount[4] - stateCount[3]) - (float)stateCount[2] / 2.0f;
        }

        private bool crossCheckDiagonal(Mat binary, int centerRow, int centerCol, int maxCount, int stateCountTotal)
        {
            int[] stateCount = new int[5];
            int i = 0;
            while (centerRow >= i && centerCol >= i && binary.Get<byte>(centerRow - i, centerCol - i) == 0)
            {
                stateCount[2]++;
                i++;
            }
            if (centerRow < i || centerCol < i)
            {
                return false;
            }
            while (centerRow >= i && centerCol >= i &&
                binary.Get<byte>(centerRow - i, centerCol - i) != 0 &&
                stateCount[1] <= maxCount)
            {
                stateCount[1]++;
                i++;
            }

            if (centerRow < i || centerCol < i || stateCount[1] > maxCount)
            {
                return false;
            }

            while (centerRow >= i && centerCol >= i &&
                binary.Get<byte>(centerRow - i, centerCol - i) == 0 &&
                stateCount[0] <= maxCount)
            {
                stateCount[0]++;
                i++;
            }
            if (stateCount[0] > maxCount)
            {
                return false;
            }

            int maxCol = binary.Cols;
            int maxRow = binary.Rows;
            i = 1;
            while ((centerRow + i) < maxRow && (centerCol + i) < maxCol &&
                binary.Get<byte>(centerRow + i, centerCol + i) == 0)
            {
                stateCount[2]++;
                i++;
            }
            if ((centerRow + i) == maxRow || (centerCol + i) == maxCol)
            {
                return false;
            }
            while ((centerRow + i) < maxRow && (centerCol + i) < maxCol &&
                binary.Get<byte>(centerRow + i, centerCol + i) != 0 &&
                stateCount[3] < maxCount)
            {
                stateCount[3]++;
                i++;
            }
            if ((centerRow + i) == maxRow || (centerCol + i) == maxCol || stateCount[3] > maxCount)
            {
                return false;
            }
            while ((centerRow + i) < maxRow && (centerCol + i) < maxCol &&
    binary.Get<byte>(centerRow + i, centerCol + i) == 0 &&
    stateCount[4] < maxCount)
            {
                stateCount[4]++;
                i++;
            }
            if ((centerRow + i) == maxRow || (centerCol + i) == maxCol || stateCount[4] > maxCount)
            {
                return false;
            }
            int newStateCountTotal = 0;
            for (int j = 0; j < 5; j++)
            {
                newStateCountTotal += stateCount[j];
            }

            return (Math.Abs(stateCountTotal - newStateCountTotal) < 2 * stateCountTotal) && checkRatio(stateCount);
        }

        private float crossCheckHorizontal(Mat binary, int centerRow, int startCol, int centerCount, int stateCountTotal)
        {
            int maxCols = binary.Cols;
            int[] stateCount = new int[5];

            int col = startCol;
            while (col >= 0 && binary.Get<byte>(centerRow, col) == 0)
            {
                stateCount[2]++;
                col--;
            }

            if (col < 0)
            {
                return float.NaN;
            }
            while (col >= 0 && binary.Get<byte>(centerRow, col) != 0 && stateCount[1] < centerCount)
            {
                stateCount[1]++;
                col--;
            }
            if (col < 0 || stateCount[1] == centerCount)
            {
                return float.NaN;
            }
            while (col >= 0 && binary.Get<byte>(centerRow, col) == 0 && stateCount[0] < centerCount)
            {
                stateCount[0]++;
                col--;
            }
            if (col < 0 || stateCount[0] == centerCount)
            {
                return float.NaN;
            }
            col = startCol + 1;
            while (col < maxCols && binary.Get<byte>(centerRow, col) == 0)
            {
                stateCount[2]++;
                col++;
            }
            if (col == maxCols)
            {
                return float.NaN;
            }
            while (col < maxCols && binary.Get<byte>(centerRow, col) != 0 && stateCount[3] < centerCount)
            {
                stateCount[3]++;
                col++;
            }
            if (col == maxCols || stateCount[3] == centerCount)
            {
                return float.NaN;
            }
            while (col < maxCols && binary.Get<byte>(centerRow, col) == 0 && stateCount[4] < centerCount)
            {
                stateCount[4]++;
                col++;
            }
            if (col == maxCols || stateCount[4] == centerCount)
            {
                return float.NaN;
            }
            int newStateCountTotal = 0;
            for (int i = 0; i < 5; i++)
            {
                newStateCountTotal += stateCount[i];
            }

            if (5 * Math.Abs(stateCountTotal - newStateCountTotal) >= stateCountTotal)
            {
                return float.NaN;
            }
            return checkRatio(stateCount) ? centerFromEnd(stateCount, col) : float.NaN;
        }

        private float crossCheckVertical(Mat binary, int startRow, int centerCol, int centralCount, int stateCountTotal)
        {
            int maxRows = binary.Rows;
            int[] crossCheckStateCount = new int[5];
            int row = startRow;
            byte pixel = binary.Get<byte>(row, centerCol);
            while (row >= 0 && pixel == 0)
            {
                crossCheckStateCount[2]++;
                row--;
                pixel = binary.Get<byte>(row, centerCol);
            }
            if (row < 0)
            {
                return float.NaN;
            }
            pixel = binary.Get<byte>(row, centerCol);
            while (row >= 0 && pixel != 0 &&
                crossCheckStateCount[1] < centralCount)
            {
                crossCheckStateCount[1]++;
                row--;
                pixel = binary.Get<byte>(row, centerCol);
            }

            if (row < 0 || crossCheckStateCount[1] >= centralCount)
            {
                return float.NaN;
            }

            while (row >= 0 && binary.Get<byte>(row, centerCol) == 0 &&
                crossCheckStateCount[0] < centralCount)
            {
                crossCheckStateCount[0]++;
                row--;
            }
            if (row < 0 || crossCheckStateCount[0] >= centralCount)
            {
                return float.NaN;
            }

            row = startRow + 1;
            while (row < maxRows && binary.Get<byte>(row, centerCol) == 0)
            {
                crossCheckStateCount[2]++;
                row++;
            }

            if (row == maxRows)
            {
                return float.NaN;
            }

            while (row < maxRows && binary.Get<byte>(row, centerCol) != 0 && crossCheckStateCount[3] < centralCount)
            {
                crossCheckStateCount[3]++;
                row++;
            }

            if (row == maxRows || crossCheckStateCount[3] >= stateCountTotal)
            {
                return float.NaN;
            }

            while (row < maxRows && binary.Get<byte>(row, centerCol) == 0
                && crossCheckStateCount[4] < centralCount)
            {
                crossCheckStateCount[4]++;
                row++;
            }
            if (row == maxRows || crossCheckStateCount[4] >= centralCount)
            {
                return float.NaN;
            }

            int crossCheckStateCountTotal = 0;
            for (int i = 0; i < 5; i++)
            {
                crossCheckStateCountTotal += crossCheckStateCount[i];
            }

            if (5 * Math.Abs(crossCheckStateCountTotal - stateCountTotal) >= 2 * stateCountTotal)
            {
                return float.NaN;
            }

            float center = centerFromEnd(crossCheckStateCount, row);
            return checkRatio(crossCheckStateCount) ? center : float.NaN;
        }

        private bool checkRatio(int[] stateCount)
        {
            int totalFinderSize = 0;
            for (int i = 0; i < 5; i++)
            {
                int count = stateCount[i];
                totalFinderSize += count;
                if (count == 0)
                {
                    return false;
                }
            }

            if (totalFinderSize < 7)
            {
                return false;
            }

            int moduleSize = (int)Math.Ceiling(totalFinderSize / 7.0);
            int maxVariance = moduleSize / 2;

            bool retVal =
                ((Math.Abs(moduleSize - stateCount[0])) < maxVariance) &&
                ((Math.Abs(moduleSize - stateCount[1])) < maxVariance) &&
                ((Math.Abs(3 * moduleSize - stateCount[2])) < maxVariance) &&
                ((Math.Abs(moduleSize - stateCount[3])) < maxVariance) &&
                ((Math.Abs(moduleSize - stateCount[4])) < maxVariance);
            return retVal;
        }
    }
}
