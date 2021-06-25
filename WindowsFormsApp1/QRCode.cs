using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// https://kennysoft.kr/qr_ko/qr1_kr.htm
/// </summary>

namespace WindowsFormsApp1
{
    public class QRCode파라메터
    {
        public enum 버전종류
        {
            Ver1,
        };

        public int 셀크기
        {
            get
            {
                if (버전 == 버전종류.Ver1)
                {
                    return 21;
                }
                return 0;
            }
        }

        public enum 인코딩종류 : int
        {
            /// <summary>
            /// 숫자 : 0001
            /// </summary>
            숫자 = 1,
            /// <summary>
            /// 알파벳 : 0010
            /// </summary>
            알파벳A = 2,
            /// <summary>
            /// 바이트 : 0100
            /// </summary>
            비트 = 4,
        };

        public enum 에러모드종류 : int
        {
            /// <summary>
            /// 7% 오류 복구
            /// </summary>
            L = 1,
            /// <summary>
            /// 15% 오류 복구
            /// </summary>
            M = 0,
            /// <summary>
            /// 25% 오류 복구
            /// </summary>
            Q = 3,
            /// <summary>
            /// 30% 오류 복구
            /// </summary>
            H = 2
        }
        // 버전 1에서 에러코드, 인코딩 모드별 데이터 수
        // 버전 1 : 
        //      숫자    알파벳  바이트
        // L :   41      25     17
        // M :   35      20     14
        // Q :   27      16     11
        // H :   17      10     7
        
            // EULJI-0000

        // 버전 1 데이터 길이 비트
        // 숫자 : 10비트
        // 알파벳 : 9비트
        // 바이트 : 8비트

        public 버전종류 버전 { get; private set; }
        public 인코딩종류 인코딩 { get; private set; }
        public 에러모드종류 에러모드 { get; private set; }

        public int 길이비트수()
        {
            if(인코딩 == 인코딩종류.숫자)
            {
                return 11;
            }
            else if(인코딩 == 인코딩종류.알파벳A)
            {
                return 9;
            }
            return 8;
        }
        static public 에러모드종류 에러모드종류변환(string 이름)
        {
            return (에러모드종류)Enum.Parse(typeof(에러모드종류), 이름);
        }

        static public string 에러모드문자열(int 인덱스)
        {
            if (인덱스 == 1)
            {
                return "M ~15%";
            }
            else if (인덱스 == 2)
            {
                return "Q ~25%";
            }
            else if (인덱스 == 3)
            {
                return "H ~30%";
            }
            return "L ~7%";
        }
        static public 에러모드종류 에러모드종류변환(int 인덱스)
        {
            if (인덱스 == 1)
            {
                return 에러모드종류.M;
            }
            else if (인덱스 == 2)
            {
                return 에러모드종류.Q;
            }
            else if (인덱스 == 3)
            {
                return 에러모드종류.H;
            }
            return 에러모드종류.L;
        }

        public void 초기화(버전종류 _버전, 에러모드종류 _에러모드, 인코딩종류 _인코딩)
        {
            버전 = _버전;
            에러모드 = _에러모드;
            인코딩 = _인코딩;

            if (인코딩 == 인코딩종류.알파벳A)
            {
                인코딩비트수 = 11;
            }

            if (버전 == 버전종류.Ver1)
            {
                if(에러모드 == 에러모드종류.H)
                {
                    오류정정코드갯수 = 17;
                    데이터코드갯수 = 9;
                }
                else if(에러모드 == 에러모드종류.M)
                {
                    오류정정코드갯수 = 10;
                    데이터코드갯수 = 16;
                }
                else if(에러모드 == 에러모드종류.Q)
                {
                    오류정정코드갯수 = 13;
                    데이터코드갯수 = 13;
                }
                else if (에러모드 == 에러모드종류.L)
                {
                    오류정정코드갯수 = 7;
                    데이터코드갯수 = 19;
                    
                }
            }
        }

        public int 전체데이터코드개수 { get { return 오류정정코드갯수 + 데이터코드갯수; } }

        public int 데이터코드갯수 { get; private set; }

        public int 오류정정코드갯수 { get; private set; }

        public int 인코딩비트수 { get; private set; }
        public int 인코딩비트
        {
            get
            {
                return (int)인코딩;
            }
        }
    }

    public class QRData
    {
        public QRCode파라메터 파라메터;

        public QRData(QRCode파라메터 _파라메터)
        {
            파라메터 = _파라메터;
        }

        public int[] 데이터코드배열;
        public int 데이터갯수;
        public int[] 인코딩배열;

        byte[] 바이트채움 = new byte[2] { 0xec, 0x11 };

        public void 데이터코드로인코딩()
        {
            if (파라메터.버전 == QRCode파라메터.버전종류.Ver1)
            {
                데이터코드배열 = new int[파라메터.데이터코드갯수];

                // [0] 
                데이터코드배열[0] =(byte)( 파라메터.인코딩비트 << 4);
                데이터코드배열[0] |= (byte)((데이터갯수 &0x1e0)>> 5);
                데이터코드배열[1] = (byte)((데이터갯수& 0x1f) << 3);

                int dstbitvalue = 1 << 2;
                int dstindex = 1;
                int dstvalue = 데이터코드배열[1];

                int srcbitoffset = (파라메터.인코딩비트수);
                int srcindex = 0;
                int srcvalue = 인코딩배열[0];

                int msbmask = (1 << (파라메터.인코딩비트수 - 1));

                while(true)
                {
                    if ((srcvalue & msbmask) == msbmask)
                    {
                         dstvalue |= (byte)dstbitvalue;
                    }

                    dstbitvalue = dstbitvalue >> 1;
                    if (dstbitvalue == 0)
                    {
                        데이터코드배열[dstindex] = dstvalue;
                        dstindex++;
                        dstvalue = 0;
                        dstbitvalue = (1 << 7);
                    }

                    srcvalue = srcvalue << 1;
                    srcbitoffset--;
                    if (srcbitoffset == 0)
                    {
                        srcindex++;
                        if (srcindex == 인코딩배열.Length)
                        {
                            break;
                        }
                        srcbitoffset = 파라메터.인코딩비트수;
                        srcvalue = 인코딩배열[srcindex];
                    }
                }

                if(dstbitvalue != (1<< 7))
                {
                    데이터코드배열[dstindex] = dstvalue;
                    dstindex++;
                }

                int padoffset = 0;
                for(int i= dstindex; i< 파라메터.데이터코드갯수; i++)
                {
                    데이터코드배열[i] = 바이트채움[padoffset];
                    if(padoffset == 0)
                    {
                        padoffset = 1;
                    }
                    else
                    {
                        padoffset = 0;
                    }
                }

            }
        }
    }

    public class QRCodeCell
    {
        public enum 셀유형
        {
            미할당,
            파인터,
            분리자,
            수평타이밍패턴,
            수직타이밍패턴,
            검정셀,
            포맷정보1,
            포맷정보2,
            데이터,
        }

        public 셀유형 유형 = 셀유형.미할당;

        public int value = 0;

        public QRCodeCell(int value)
        {
            유형 = 셀유형.데이터;
            this.value = value;
        }

        public QRCodeCell(셀유형 유형, int value)
        {
            this.유형 = 유형;
            this.value = value; 
        }
        public int 값되돌리기()
        {
            if (value == -1)
            {
                value = 1;
            }
            else if (value == -2)
            {
                value = 0;
            }
            
            return value;
        }
        public int 반전시킴()
        {
            if(value == 1)
            {
                value = -1;
            }
            else if(value == -1)
            {
                value = 1;
            }
            else if(value == -2)
            {
                value = 0;
            }
            else if(value == 0)
            {
                value = -2;
            }
            return value;
        }
    }

    public class QRCode마스크패턴
    {

    }
    public class QRCode
    {

        public List<QRCodeCell>[] 포멧비트셀 = new List<QRCodeCell>[2];

        public QRCodeCell[][] 셀;
        public string 메시지;
        public byte[] 데이터배열;
        public QRCode파라메터 파라메터;

        public QRCode(QRCode파라메터 파라메터)
        {
            this.파라메터 = 파라메터;
        }
        
        public List<QRCodeCell[][]> 마스크 = new List<QRCodeCell[][]>();

        public void 마스크패턴()
        {
            List<int> weights = new List<int>();

            for (int type = 0; type < 8; type++)
            {
                QRCodeCell[][] 마스크결과 = 마스크패턴(type, 셀);
                //
                마스크.Add(마스크결과);

                List<int> 포멧 = new List<int>();
                int error = (int)파라메터.에러모드;

                포멧.Add((error & 2) >> 1);
                포멧.Add((error & 1));
                for (int i = 2; i >= 0; i--)
                {
                    포멧.Add((type >> i) & 1);
                }

                if (type == 0 && error == 0)
                {
                    int[] f = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
                    포멧.AddRange(f);
                }
                else
                {
                    List<int> 다항식 = new List<int>(포멧);
                    //포멧 = new List<int>() { 0, 1, 1, 0, 0 };

                    int[] g = new int[] { 1, 0, 1, 0, 0, 1, 1, 0, 1, 1, 1 };

                    for (int i = 0; i < (15 - 포멧.Count); i++)
                    {
                        다항식.Add(0);
                    }

                    while (다항식.First() == 0)
                    {
                        다항식.Remove(0);
                    }

                    do
                    {
                        List<int> 생성다항식 = new List<int>();
                        생성다항식.AddRange(g);

                        // 생성다항식을 다항식과 같은 차수로 맞춤
                        int 자릿수 = 다항식.Count - g.Length;
                        for (int i = 0; i < 자릿수; i++)
                        {
                            생성다항식.Add(0);
                        }

                        for (int i = 0; i < 다항식.Count; i++)
                        {
                            다항식[i] = 다항식[i] ^ 생성다항식[i];
                        }
                        while (다항식.First() == 0)
                        {
                            다항식.Remove(0);
                        }
                    } while (다항식.Count >= g.Length);

                    // 왼쪽에 0채움
                    while (다항식.Count < 10)
                    {
                        다항식.Insert(0, 0);
                    }
                    포멧.AddRange(다항식);
                }


                int[] xor = new int[] { 1, 0, 1, 0, 1, 0, 0, 0, 0, 0, 1, 0, 0, 1, 0 };
                for (int i = 0; i < 포멧.Count; i++)
                {
                    포멧[i] = 포멧[i] ^ xor[i];
                }
                // 포멧.Reverse();

                int 포멧비트위치 = 0;

                for (int i = 0; i < 7; i++)
                {
                    // QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보1, 포멧[포멧비트위치]);
                    if (마스크결과[8][i].유형 == QRCodeCell.셀유형.미할당)
                    {
                        마스크결과[8][i].유형 = QRCodeCell.셀유형.포맷정보1;
                        마스크결과[8][i].value = 포멧[포멧비트위치];
                        포멧비트위치++;
                    }
                }

                for (int i = 0; i  < 8; i ++)
                {
                    int xo = 파라메터.셀크기 - 8;
                    if (마스크결과[8][xo + i].유형 == QRCodeCell.셀유형.미할당)
                    {
                        //                        QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보1, 포멧[포멧비트위치]);
                        마스크결과[8][xo + i].유형 = QRCodeCell.셀유형.포맷정보1;
                        마스크결과[8][xo + i].value = 포멧[포멧비트위치];
                        포멧비트위치++;
                    }
                }

                포멧비트위치 = 14;
                for (int i = 0; i < 9; i++)
                {
                    if (마스크결과[i][8].유형 == QRCodeCell.셀유형.미할당)
                    {
                        // QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보2, 포멧[포멧비트위치]);
                        마스크결과[i][8].value = 포멧[포멧비트위치];
                        마스크결과[i][8].유형 = QRCodeCell.셀유형.포맷정보2;
                        포멧비트위치--;
                    }
                }

                for (int i = 0; i < 7; i++)
                {
                    int yo = 파라메터.셀크기 - 7;
                    if (마스크결과[yo + i][8].유형 == QRCodeCell.셀유형.미할당)
                    {
                        //QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보2, 포멧[포멧비트위치]);
                        마스크결과[yo + i][8].유형 = QRCodeCell.셀유형.포맷정보2;
                        마스크결과[yo + i][8].value = 포멧[포멧비트위치];
                        포멧비트위치--;
                    }
                }
                Console.WriteLine($"마스크 : {type}");
                QRCode.print(마스크결과, 파라메터, true);
                weights.Add(가중치계산(마스크결과));
            }

            int mi = 0;
            int m = weights[0];

            for (int i = 1; i < weights.Count; i++)
            {
                if (m > weights[i])
                {
                    m = weights[i];
                    mi = i;
                }
            }

            for(int y = 0; y< 파라메터.셀크기; y++)
            {
                for(int x= 0; x<파라메터.셀크기; x++)
                {
                    셀[y][x] = 마스크[mi][y][x];
                }
            }
        }

        public int 가중치계산(QRCodeCell[][] t)
        {
            // 가중치 계산
            // 1 - 같은 색이 5 + i번 반복된다.
            // 2 - 

            int weight = 0;
            #region 같은 색 가중치
            for (int y = 0; y < t.Length; y++)
            {
                int 반복수 = 1;
                int 반복중인비트 = t[y][0].value;

                for (int x = 1; x < t.Length; x++)
                {
                    if (반복중인비트 == t[y][x].value)
                    {
                        반복수++;
                        if (반복수 == 5)
                        {
                            weight += 3;
                        }
                        else if(반복수 > 5)
                        {
                            weight++;
                        }
                    }
                    else
                    {
                        반복수 = 1;
                        반복중인비트 = t[y][x].value;
                    }
                }
            }

            for (int x = 0; x < t.Length; x++)
            {
                int 반복수 = 1;
                int 반복중인비트 = t[0][x].value;
                for (int y = 1; y < t.Length; y++)
                {
                    if (반복중인비트 == t[y][x].value)
                    {
                        반복수++;
                        if (반복수 == 5)
                        {
                            weight += 3;
                        }
                        else if (반복수 > 5)
                        {
                            weight++;
                        }
                    }
                    else
                    {
                        반복수 = 1;
                        반복중인비트 = t[y][x].value;
                    }
                }
            }
            #endregion

            #region 2x2 

            for (int y = 0; y < t.Length - 1; y++)
            {
                for (int x = 0; x < t.Length - 1; x++)
                {
                    int v = t[y][x].value;
                    if (v == t[y][x + 1].value
                        && v == t[y + 1][x].value
                        && v == t[y + 1][x + 1].value)
                    {
                        weight += 3;
                    }
                }
            }
            #endregion

            #region b1:w1:b3:w1:b1 - 수평
            int[,] patten_n3 = new[,] {
                { 1, 0, 1, 1, 1, 0, 1, 0, 0, 0, 0 },
                { 0, 0, 0, 0, 1, 0, 1, 1, 1, 0, 1}
            };

            for (int y = 0; y < t.Length; y++)
            {
                for (int x = 0; x < t.Length - patten_n3.GetLength(1); x++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        bool find = true;
                        for (int j = 0; j < patten_n3.GetLength(1); j++)
                        {
                            if (t[y][x + j].value != patten_n3[i, j])
                            {
                                find = false;
                                break;
                            }
                        }
                        if(find)
                        {
                            weight += 40;
                        }
                    }
                }
            }
            #endregion

            #region b1:w1:b3:w1:b1 - 수직
            for (int x = 0; x < t.Length; x++)
            {
                for (int y = 0; y < t.Length - patten_n3.GetLength(1); y++)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        bool find = true;
                        for (int j = 0; j < patten_n3.GetLength(1); j++)
                        {
                            if (t[y + j][x].value != patten_n3[i, j])
                            {
                                find = false;
                                break;
                            }
                        }
                        if (find)
                        {
                            weight += 40;
                        }
                    }
                }
            }
            #endregion

            #region 검정색 비율
            int total = 0;
            int black = 0;

            for (int y = 0; y < t.Length; y++)
            {
                for (int x = 0; x < t.Length; x++)
                {
                    total++;
                    if (t[y][x].value == 1)
                    {
                        black++;
                    }
                }
            }

            double p = (black / total) * 100;
            double p1 = Math.Floor((p + 5) / 5);
            double p2 = Math.Floor((p - 5) / 5);
            int max = (int)Math.Ceiling(p1 * 5);
            int min = (int)Math.Ceiling(p2 * 5);
            max = Math.Abs(max - 50);
            min = Math.Abs(min - 50);

            max = max / 5;
            min = max / 5;

            weight += Math.Min(max, min) * 10;
            #endregion

            return weight;
        }

        public QRCodeCell 계산(int type, int x, int y, int value)
        {
            int mask = -1;
            switch (type)
            {
                case 0:
                    // (row + column) mod 2 == 0
                    mask = (y + x) % 2;
                    break;
                case 1:
                    // (row) mod 2 == 0
                    mask = y % 2;
                    break;
                case 2:
                    // (column) mod 3 == 0
                    mask = x % 3;
                    break;
                case 3:
                    // (row + column) mod 3 == 0
                    mask = (y + x) % 3;
                    break;
                case 4:
                    // ( floor(row / 2) + floor(column / 3) ) mod 2 == 0
                    mask = ((int)Math.Floor(y / 2.0) + (int)Math.Floor(x / 3.0)) % 2;
                    break;
                case 5:
                    // ((row * column) mod 2) + ((row * column) mod 3) == 0
                    mask = ((y * x) % 2) + ((y * x) % 3);
                    break;
                case 6:
                    // ( ((row * column) mod 2) + ((row * column) mod 3) ) mod 2 == 0
                    mask = (((y * x) % 2) + ((y * x) % 3) % 2);
                    break;
                case 7:
                    // ( ((row + column) mod 2) + ((row * column) mod 3) ) mod 2 == 0
                    mask = (((y + x) % 2) + ((y * x) % 3)) % 2;
                    break;
            }

            if (mask == 0)
            {
                value = value == 0 ? 1 : 0;
            }
            return new QRCodeCell(value);
        }

        public QRCodeCell[][] 마스크패턴(int type, QRCodeCell[][] 원본)
        {
            QRCodeCell[][] 마스크 = new QRCodeCell[원본.Length][];

            for (int y = 0; y < 마스크.Length; y++)
            {
                마스크[y] = new QRCodeCell[원본.Length];
            }

            for (int y = 0; y < 원본.Length; y++)
            {
                for (int x = 0; x < 원본[y].Length; x++)
                {
                    QRCodeCell t = 원본[y][x];
                    if (t.유형 != QRCodeCell.셀유형.데이터)
                    {
                        if(원본[y][x] == null)
                        {
                            마스크[y][x] = null;
                        }
                        else
                        {
                            마스크[y][x] = new QRCodeCell(t.유형, t.value);
                        }
                    }
                    else
                    {
                        마스크[y][x] = 계산(type, x, y, t.value);
                    }
                }
            }
            return 마스크;
        }

        public void 셀생성()
        {
            if (파라메터.버전 == QRCode파라메터.버전종류.Ver1)
            {
                셀 = new QRCodeCell[파라메터.셀크기][];
                for (int i = 0; i < 파라메터.셀크기; i++)
                {
                    셀[i] = new QRCodeCell[파라메터.셀크기];
                }
            }
        }

        private void 사각형(int x0, int y0)
        {
            // 위줄
            for(int i=0;i<7;i++)
            {
                for(int j=0; j<7;j++)
                {
                    셀[y0 + i ][x0 + j] = new QRCodeCell(QRCodeCell.셀유형.파인터, 0);
                }
            }

            for (int i = 0; i < 7; i++)
            {
                셀[y0][x0 + i].value = 1;
            }
            // 왼쪽 세로 줄
            for (int i = 1; i < 6; i++)
            {
                셀[y0 + i][x0].value = 1;
            }
            // 오른쪽 세로 줄
            for (int i = 1; i < 6; i++)
            {
                셀[y0 + i][x0 + 6].value = 1;
            }
            // 아래줄
            for (int i = 0; i < 7; i++)
            {
                셀[y0 + 6][x0 + i].value = 1;
            }
            // 가운데 상자
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    셀[y0 + 2 + i][x0 + 2 + j].value = 1;
                }
            }
        }

        public void 파인터생성()
        {
            // 왼쪽 위(0,0)
            사각형(0, 0);
            // 분리자
            for (int i = 0; i < 7; i++)
            {
                셀[i][7] = new QRCodeCell(QRCodeCell.셀유형.분리자, 0);
            }
            for (int i = 0; i < 8; i++)
            {
                셀[7][i] = new QRCodeCell(QRCodeCell.셀유형.분리자, 0);
            }
            사각형(파라메터.셀크기 - 7, 0);
            for (int i = 0; i < 7; i++)
            {
                셀[i][파라메터.셀크기 - 8] = new QRCodeCell(QRCodeCell.셀유형.분리자, 0);
            }
            for (int i = 0; i < 8; i++)
            {
                셀[7][파라메터.셀크기 - 8+i] = new QRCodeCell(QRCodeCell.셀유형.분리자, 0);
            }

            사각형(0, 파라메터.셀크기 - 7);
            for (int i = 0; i < 8; i++)
            {
                셀[파라메터.셀크기 - 8][i] = new QRCodeCell(QRCodeCell.셀유형.분리자, 0);
            }

            for (int i = 0; i < 8; i++)
            {
                셀[파라메터.셀크기 - 8 + i][7] = new QRCodeCell(QRCodeCell.셀유형.분리자, 0);
            }

        }
/*
        public void 포멧비트생성()
        {
            포멧비트[0] = new List<QRCodeCell>();
            for (int i =0; i<8; i++)
            {
                QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보1, 0);
                셀[8][파라메터.셀크기 - 1 - i] = a;
                포멧비트[0].Add(a);
            }
            for(int i= 7; i >= 0; i--)
            {
                if (셀[8][i] == null || 셀[8][i].유형 == QRCodeCell.셀유형.미할당)
                {
                    QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보1, 0);
                    셀[8][i] = a;
                    포멧비트[0].Add(a);
                }
            }

            포멧비트[1] = new List<QRCodeCell>();
            for(int i=0;i<9;i++)
            {
                if (셀[i][8] == null || 셀[i][8].유형 == QRCodeCell.셀유형.미할당)
                {
                    QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보2, 0);
                    셀[i][8] = a;
                    포멧비트[1].Add(a);
                }
            }
            for(int i=파라메터.셀크기 - 7; i< 파라메터.셀크기; i++)
            {
                if (셀[i][8] == null || 셀[i][8].유형 == QRCodeCell.셀유형.미할당)
                {
                    QRCodeCell a = new QRCodeCell(QRCodeCell.셀유형.포맷정보2, 0);
                    셀[i][8] = a;
                    포멧비트[1].Add(a);
                }
            }
        }
*/

        public void cellvalue()
        {
            for (int i = 0; i < 파라메터.셀크기; i++)
            {
                for (int j = 0; j < 파라메터.셀크기; j++)
                {
                    Console.Write(셀[i][j].value == 1 ? '*' : ' ');
                }
                Console.Write("\n");
            }
        }
        public void print()
        {
            QRCode.print(셀, 파라메터, false);
        }

        static public void print(QRCodeCell [][]셀, QRCode파라메터 파라메터, bool 값인쇄)
        {
            for(int i=0;i< 파라메터.셀크기; i++)
            {
                for(int j=0; j< 파라메터.셀크기; j++)
                {
                    if (셀[i][j] != null)
                    {
                        if (값인쇄)
                        {
                            if (셀[i][j].value == 0)
                            {
                                Console.Write(".");
                            }
                            else
                            {
                                Console.Write("0");
                            }
                        }
                        else
                        {
                            if (셀[i][j].유형 == QRCodeCell.셀유형.분리자)
                            {
                                Console.Write('*');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.포맷정보1)
                            {
                                Console.Write('A');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.포맷정보2)
                            {
                                Console.Write('B');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.파인터)
                            {
                                Console.Write('P');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.수직타이밍패턴)
                            {
                                Console.Write('|');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.수평타이밍패턴)
                            {
                                Console.Write('-');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.검정셀)
                            {
                                Console.Write('^');
                            }
                            else if (셀[i][j].유형 == QRCodeCell.셀유형.미할당)
                            {
                                Console.Write("R");
                            }
                            else
                            {
                                Console.Write(셀[i][j].value);
                            }
                        }
                    }
                    else
                    {
                        Console.Write('.');
                    }
                }
                Console.Write("\n");
            }
            Console.Write("\n");
        }

        public void 예약영역생성()
        {

            셀[파라메터.셀크기 - 8][8] = new QRCodeCell(QRCodeCell.셀유형.검정셀, 1);
            // 왼쪽 위 파인더 왼쪽 예약 영역
            for (int i=0; i < 9; i++)
            {
                if (셀[i][8] == null)
                {
                    셀[i][8] = new QRCodeCell(QRCodeCell.셀유형.미할당, 0);
                }
            }
            // 왼쪽 위 파인더 아래 예약 영역
            for(int i=0; i<8; i++)
            {
                if (셀[8][i] == null)
                {
                    셀[8][i] = new QRCodeCell(QRCodeCell.셀유형.미할당, 0);
                }
            }

            // 오른쪽 위 파인더 아래 예약 영역
            int 시작 = 파라메터.셀크기 - 8;
            for(int i=0; i<8;i++)
            {
                if (셀[8][시작 + i] == null)
                {
                    셀[8][시작 + i] = new QRCodeCell(QRCodeCell.셀유형.미할당, 0);
                }
            }

            // 아래 파인더 왼쪽 예약 영역
            시작 = 파라메터.셀크기 - 8;
            for(int  i = 0; i<8;i++)
            {
                if (셀[시작 + i][8] == null)
                {
                    셀[시작 + i][8] = new QRCodeCell(QRCodeCell.셀유형.미할당, 0);
                }
            }
        }
        public void 타이밍패턴생성()
        {
            // 위 - 왼쪽-오른쪽
            int p = 0;
            for (int i = 7 + 1; i < 파라메터.셀크기 - 8; i ++, p++)
            {
                셀[6][i] = new QRCodeCell(QRCodeCell.셀유형.수평타이밍패턴, 0);
                if (p % 2 == 0)
                {
                    셀[6][i].value = 1;
                }
            }
            p = 0;
            // 위 - 아래
            for (int i = 7 + 1; i < 파라메터.셀크기 - 8; i ++, p++)
            {
                셀[i][6] = new QRCodeCell(QRCodeCell.셀유형.수직타이밍패턴, 0);
                if (p % 2 == 0)
                {
                    셀[i][6].value = 1;
                }
            }

        }

        public void 데이터패턴생성()
        {
            int 방향 = 0;// 0 : up, 1 : down
            int x0 = 파라메터.셀크기 - 1;
            int y0 = 파라메터.셀크기 - 1;
            int dataidx = 데이터배열.Length - 1;
            int bitidx = 7;
            byte data = 데이터배열[dataidx];
            int x = x0;
            int y = y0;
            int offset = 0;
            //Console.WriteLine(data.ToString() + "\n");
            while (dataidx >= 0 && x >= 0)
            {
                if (셀[y][x - offset] != null)
                {
                    if (셀[y][x - offset].유형 == QRCodeCell.셀유형.수직타이밍패턴)
                    {
                        // 다음줄로 이동
                        x --;
                    }
                }

                if (셀[y][x - offset] == null)
                {
                    int v = (data >> bitidx) & 1;
                    셀[y][x - offset] = new QRCodeCell(v);

                    if (bitidx == 0)
                    {
                        bitidx = 7;
                        if (dataidx == 0)
                        {
                            break;
                        }
                        else
                        {
                            dataidx--;
                        }
                        data = 데이터배열[dataidx];
                        //Console.WriteLine(data.ToString() + "\n");
                    }
                    else
                    {
                        bitidx--;
                    }

                    // print();
                }
                
                offset++;
                if(offset == 2)
                {
                    offset = 0;
                    if (방향 == 0)
                    {
                        if (y == 0)
                        {
                            방향 = 1;
                            x -= 2;
                        }
                        else
                        {
                            y--;
                        }
                    }
                    else
                    {
                        if(y == y0)
                        {
                            방향 = 0;
                            x -= 2;
                        }
                        else
                        {
                            y++;
                        }
                    }
                }
            }    
            // 
        }

        /// <summary>
        /// 문자열을 비트 배열로 변환한다.
        /// </summary>
        /// <param name="파라메터"></param>
        /// <param name="데이터"></param>
        /// <returns></returns>
        public void 데이터생성(string 데이터)
        {
            데이터 = 데이터.ToUpper();
            //QRData qrData = new QRData(파라메터);

            비트스트림 s = new 비트스트림();
            s.추가(4, (int)파라메터.인코딩);
            s.추가(파라메터.길이비트수(), 데이터.Length);

            if (파라메터.인코딩 == QRCode파라메터.인코딩종류.알파벳A)
            {
                #region 영숫자 아스키코드를 숫자값으로 변환 
                int 짝수 = 데이터.Length / 2;

                for (int i = 0; i < 짝수 * 2; i += 2)
                {
                    char c = 데이터[i];
                    int d1 = ascii2bin(데이터[i]);
                    int d2 = ascii2bin(데이터[i + 1]);

                    d1 = d1 * 45 + d2;
                    s.추가(파라메터.인코딩비트수, d1);
                }

                if (데이터.Length % 2 == 1)
                {
                    char c = 데이터.Last();
                    int b = ascii2bin(c);
                    s.추가(6, b);
                }
                #endregion
            }
            else if(파라메터.인코딩 == QRCode파라메터.인코딩종류.비트)
            {
                byte[] bits = Encoding.Default.GetBytes(데이터);
                for(int i=0;i<bits.Length; i++)
                {
                    s.추가(8, bits[i]);
                }
            }
            else if(파라메터.인코딩 == QRCode파라메터.인코딩종류.숫자)
            {
                int 몫 = 데이터.Length / 3;
                int pos = 0;
                for (; pos < 몫 * 3; pos += 3)
                {
                    string o = 데이터.Substring(pos, 3);
                    int v = Convert.ToInt32(o);
                    s.추가(10, v);
                }
                int m = 데이터.Length % 3;
                if(m == 1)
                {
                    string o = 데이터.Substring(pos, 1);
                    int v = Convert.ToInt32(o);
                    s.추가(4, v);
                }
                else if(m==2)
                {
                    string o = 데이터.Substring(pos, 2);
                    int v = Convert.ToInt32(o);
                    s.추가(7, v);
                }
            }
            else
            {
                return;
            }

            int 필요한비트수 = 파라메터.데이터코드갯수 * 8;

            if (필요한비트수 > s.총비트수)
            {
                int 종료비트수 = Math.Min(4, 필요한비트수 - s.총비트수);
                s.추가(종료비트수, 0);
            }
            List<int> 인코딩배열;
            int 나머지 = s.변환하기(8, out 인코딩배열);
            if(인코딩배열.Count < 파라메터.데이터코드갯수)
            {
                /*
                if(나머지 < 4)
                {
                    인코딩배열.Add(0);
                }
                */
                byte[] padding = new byte[] { 236, 17 };
                int 모자란수 = 파라메터.데이터코드갯수 - 인코딩배열.Count;
                for (int i=0;  i<모자란수; i++)
                {
                    인코딩배열.Add(padding[ i % 2]);
                }
            }

            인코딩배열.Reverse();
            #region RS 부호화
            // 데이터 코드로 인코딩
            RS파라메터 RS파라메터 = new RS파라메터(8, 파라메터.전체데이터코드개수,
                파라메터.오류정정코드갯수, 0);
            int[] 원시다항식 = new int[] { 1, 0, 1, 1, 1, 0, 0, 0, 1 };
            RS부호기 rs부호기 = new RS부호기(RS파라메터, 원시다항식);
            rs부호기.encode_rs(인코딩배열.ToArray());
            #endregion

            데이터배열 = new byte[rs부호기.encoded.Length];
            for (int i = 0; i < rs부호기.encoded.Length; i++)
            {
                데이터배열[i] = (byte)rs부호기.encoded[i];
            }
            //return qrData;

            List<int> 인코딩결과배열;
            s.변환하기(1, out 인코딩결과배열);
            foreach (var a in 인코딩결과배열)
            {
                Console.Write(a);
            }
            Console.Write("\n");

        }

        private static int ascii2bin(char c)
        {
            int v = -1;
            if (c >= '0' && c <= '9')
            {
                v = (byte)((int)c - (int)'0');
            }
            else if (c >= 'A' && c <= 'Z')
            {
                v = (byte)((int)c - (int)'A' + 10);
            }
            else
            {
                switch (c)
                {
                    case ' ':
                        v = 36;
                        break;
                    case '$':
                        v = 37;
                        break;
                    case '%':
                        v = 38;
                        break;
                    case '*':
                        v = 39;
                        break;
                    case '+':
                        v = 40;
                        break;
                    case '-':
                        v = 41;
                        break;
                    case '.':
                        v = 42;
                        break;
                    case '/':
                        v = 43;
                        break;
                    case ':':
                        v = 44;
                        break;
                }
            }

            return v;
        }

        byte[] 바이트채움 = new byte[2] { 0xec, 0x11 };

        public int[] 데이터코드로인코딩(int[] 인코딩배열, int 데이터갯수)
        {
            if (파라메터.버전 == QRCode파라메터.버전종류.Ver1)
            {
                //int[] 데이터코드배열 = new int[파라메터.데이터코드갯수];
                List<int> 데이터코드배열 = new List<int>();
                int 필요한비트수 = 파라메터.데이터코드갯수 * 8;

                // [0] 
                int dstvalue = (byte)(파라메터.인코딩비트 << 4);
                dstvalue |= (byte)((데이터갯수 & 0x1e0) >> 5);
                데이터코드배열.Add(dstvalue);
                
                //int [0] = (byte)(파라메터.인코딩비트 << 4);
                //데이터코드배열[0] |= (byte)((데이터갯수 & 0x1e0) >> 5);
                //데이터코드배열[1] = (byte)((데이터갯수 & 0x1f) << 3);
                필요한비트수 -= 13;
                //필요한비트수 -= 5;

                int dstbitvalue = 1 << 2;
                int dstindex = 1;
                //int dstvalue = 데이터코드배열[1];
                dstvalue = (byte)((데이터갯수 & 0x1f) << 3);
                int srcbitoffset = (파라메터.인코딩비트수);
                int srcindex = 0;
                int srcvalue = 인코딩배열[0];

                int 나머지 = 데이터갯수 % 2;
                int msbmask = (1 << (파라메터.인코딩비트수 - 1));

                while (true)
                {
                    if ((srcvalue & msbmask) == msbmask)
                    {
                        dstvalue |= (byte)dstbitvalue;
                    }
                    
                    dstbitvalue = dstbitvalue >> 1;
                    if (dstbitvalue == 0)
                    {
                        // 필요한비트수 -= 8;
                        //데이터코드배열[dstindex] = dstvalue;
                        데이터코드배열.Add(dstvalue);
                        dstindex++;
                        dstvalue = 0;
                        dstbitvalue = (1 << 7);
                    }

                    필요한비트수--;
                    srcvalue = srcvalue << 1;
                    srcbitoffset--;
                    if (srcbitoffset == 0)
                    {
                        srcindex++;
                        if (srcindex == 인코딩배열.Length)
                        {
                            break;
                        }
                        if (srcindex == 인코딩배열.Length - 1)
                        {
                            if (나머지 == 0)
                            {
                                srcbitoffset = 파라메터.인코딩비트수;
                            }
                            else
                            {
                                srcbitoffset = 6;
                            }
                        }
                        else
                        {
                            srcbitoffset = 파라메터.인코딩비트수;
                        }
                        srcvalue = 인코딩배열[srcindex];
                    }
                }

                int dst남은비트수 = 0;
                필요한비트수 = Math.Min(필요한비트수, 4);
                for (int i= dstbitvalue; i!=0; i= i>>1)
                {
                    dst남은비트수++;
                    필요한비트수--;
                    if (필요한비트수 > 0)
                    {
                        dstvalue <<= 1;
                    }
                    else
                    {
                        break;
                    }
                }
                
                if (dstbitvalue != (1 << 7))
                {
                    //데이터코드배열[dstindex] = dstvalue;
                    데이터코드배열.Add(dstvalue);
                    dstindex++;
                    dstvalue = 0;
                }
                
                int padoffset = 0;
                for (int i = dstindex; i < 파라메터.데이터코드갯수; i++)
                {
                    데이터코드배열.Add(바이트채움[padoffset]);
                    if (padoffset == 0)
                    {
                        padoffset = 1;
                    }
                    else
                    {
                        padoffset = 0;
                    }
                }
                return 데이터코드배열.ToArray();
            }
            return null;
        }

    }
}
