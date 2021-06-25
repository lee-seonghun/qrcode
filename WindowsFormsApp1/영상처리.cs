using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class 영상처리
    {
        static public int [] 줄단위edge검출(byte[] line)
        {
            double[] 블러결과 = new double[line.Length];

            블러실행(line, 블러결과);

            double[] 미분값 = new double[블러결과.Length];
            int[] 각도 = new int[블러결과.Length];
            미분하기(블러결과, 미분값, 각도);

            double[] 극대값위치 = new double[미분값.Length];
            #region 지역최대값 선택
            for (int x = 1; x < 미분값.Length - 1; x++)
            {
                // 극대값
                if(미분값[x-1] < 미분값[x]
                    && 미분값[x+1] < 미분값[x])
                {
                    극대값위치[x] = 미분값[x];
                }
                else
                {
                    //미분값[x] = 0;
                }
            }
            #endregion
            Array.Copy(극대값위치, 미분값, 미분값.Length);
            int[] 결과 = new int[line.Length];
            const int 임계 = 80;
            const int 임계2 = 20;

            for (int x = 0; x < 극대값위치.Length; x++)
            {
                if (임계 < 미분값[x])
                {
                    결과[x] = 1;
                
                }
                else if (임계2 < 미분값[x])
                {
                    if (각도[x] > 0)
                    {
                        Stack<int> 방문 = new Stack<int>();
                        for (int i = x + 1; i < 극대값위치.Length; i++)
                        {
                            if (미분값[i] > 임계)
                            {
                                while(방문.Count > 0)
                                {
                                    int x0 = 방문.Pop();
                                    결과[x0] = 1;
                                }
                            }
                            else if (미분값[i] > 임계2)
                            {
                                방문.Push(i);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    else if (각도[x] < 0)
                    {
                        Stack<int> 방문 = new Stack<int>();
                        for (int i = x - 1; i >= 0; i--)
                        {
                            if (미분값[i] > 임계)
                            {
                                while (방문.Count > 0)
                                {
                                    int x0 = 방문.Pop();
                                    결과[x0] = 1;
                
                                }
                            }
                            else if (미분값[i] > 임계2)
                            {
                                방문.Push(i);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }
            
            return 결과;
        }

        /// <summary>
        /// https://github.com/dlbeer/quirc/blob/master/lib/identify.c
        /// </summary>
        /// <param name="lines"></param>
        private void 이진화(byte[] lines)
        {

        }
        private static void 미분하기(double[] 블러결과, double[] 미분값, int [] 각도)
        {
            #region 미분
            double[] 미분계수 = new double[] { -2, 0, 2 };
            for (int x = 1; x < 블러결과.Length - 1; x++)
            {
                double 미분 = 0;
                for (int i = 0; i < 미분계수.Length; i++)
                {
                    미분 += 블러결과[x - 1 + i] * 미분계수[i];
                }
                if(미분 > 0)
                {
                    각도[x] = 1;
                }
                else if(미분 < 0)
                {
                    각도[x] = -1;
                }

                미분값[x] = Math.Abs(미분);
            }
            #endregion
        }

        private static void 블러실행(byte[] line, double[] 블러결과)
        {
            #region 블러
            // http://dev.theomader.com/gaussian-kernel-calculator/ 에서 계산
            double[] 커널 = new double[] { 0.06136, 0.24477, 0.38774, 0.24477, 0.06136 };

            for (int x = 0; x < 3; x++)
            {
                블러결과[x] = line[x];
            }

            for (int x = line.Length - 3; x < line.Length; x++)
            {
                블러결과[x] = line[x];
            }

            for (int x = 3; x < line.Length - 3; x++)
            {
                // 1차원 blur
                double 블러값 = 0;
                for (int i = 0; i < 커널.Length; i++)
                {
                    블러값 += line[x - 2 + i] * 커널[i];
                }

                블러결과[x] = 블러값;
            }
            #endregion
        }
    }
}
