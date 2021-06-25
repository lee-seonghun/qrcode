using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class RS파라메터
    {
        /// <summary>
        /// m : 심볼 비트 개수, 유한체 크기, GF(2^m)
        /// </summary>
        public int m;
        /// <summary>
        /// code word : 전체 심볼 개수
        /// </summary>
        public int length;

        /// <summary>
        /// parity : 패리티 심볼 개수, n - k = 2t
        /// </summary>
        public int red;

        /// <summary>
        /// k : 부호화할 데이터 심볼 개수
        /// </summary>
        public int k { get; private set; }
        public int t { get; private set; }
        public int t2 { get; private set; }
        public int t21 { get; private set; }
        public int init_zero { get; private set; }
        public RS파라메터(int m, int length, int red, int init_zero)
        {
            this.m = m;
            this.length = length;
            this.red = red;
            this.init_zero = init_zero;
            k = this.length - this.red;
            t = this.red / 2;
            t2 = 2 * t;
            t21 = t2 + 1;
        }
    }

    public class RS부호기
    {
        public RS파라메터 파라메터;
        /// <summary>
        /// 원시다항식배열, 0번째가 상수항이고 계수가 {0,1}인 m차 다항식
        /// </summary>
        public int[] 원시다항식;
        /// <summary>
        /// n = 2^m - 1, GF(2^m)의 원소 개수, 나머지 연산에 사용된다.
        /// </summary>
        public int n { get; private set; }
        public int length { get { return 파라메터.length; } }
        public int k { get { return 파라메터.k; } }
        public int[] alpha_to;
        public int[] index_of;
        public int[] p
        {
            get
            {
                return 원시다항식;
            }
        }

        public int m
        {
            get { return 파라메터.m; }
        }

        public int[] g;


        public int[] encoded;

        public int init_zero
        {
            get
            {
                return 파라메터.init_zero;
            }
        }

        public RS부호기(RS파라메터 _파라메터, int[] 원시다항식)
        {
            this.파라메터 = _파라메터;
            if (원시다항식.Length == 파라메터.m + 1)
            {
                this.원시다항식 = new int[원시다항식.Length];
                Array.Copy(원시다항식, this.원시다항식, 원시다항식.Length);

                n = 1;
                for (int i = 0; i <= 파라메터.m; i++)
                {
                    n *= 2;
                }
                n = n / 2 - 1;

                g = new int[length - k + 1];

                alpha_to = new int[n + 1];
                index_of = new int[n + 1];

                encoded = new int[length];
            }
            else
            {
                //throw
            }

            generate_gf();
            gen_poly();
        }

        // generate GF(2^m) from the irreducible polynomial p(X) in p[0]..p[m]  
        //  
        // lookup tables:  log->vector form           alpha_to[] contains j=alpha**i;  
        //                 vector form -> log form    index_of[j=alpha**i] = i  
        // alpha=2 is the primitive element of GF(2^m)  
        public void generate_gf()
        {
            int mask = 1;
            alpha_to[m] = 0;

            //
            for (int i = 0; i < m; i++)
            {
                alpha_to[i] = mask;
                index_of[alpha_to[i]] = i;
                if (p[i] != 0)
                {
                    alpha_to[m] ^= mask;
                }
                mask <<= 1;
            }
            index_of[alpha_to[m]] = m;
            mask >>= 1;
            for (int i = m + 1; i < n; i++)
            {
                if (alpha_to[i - 1] >= mask)
                {
                    alpha_to[i] = alpha_to[m] ^ ((alpha_to[i - 1] ^ mask) << 1);
                }
                else
                {
                    alpha_to[i] = alpha_to[i - 1] << 1;
                }
                index_of[alpha_to[i]] = i;
            }
            index_of[0] = -1;
        }

        // Compute the generator polynomial of the t-error correcting, length  
        // n=(2^m -1) Reed-Solomon code from the product of (X+alpha^i), for  
        // i = init_zero, init_zero + 1, ..., init_zero+length-k-1  
        public void gen_poly()
        {
            // g(x) = ( X + alpha^init_zero)
            g[0] = alpha_to[init_zero];
            g[1] = 1;

            for (int i = 2; i <= length - k; i++)
            {
                g[i] = 1;
                for (int j = i - 1; j > 0; j--)
                {
                    if (g[j] != 0)
                    {
                        g[j] = g[j - 1] ^ alpha_to[(index_of[g[j]] + i + init_zero - 1) % n];
                    }
                    else
                    {
                        g[j] = g[j - 1];
                    }
                }
                g[0] = alpha_to[(index_of[g[0]] + i + init_zero - 1) % n];
            }

            for (int i = 0; i <= length - k; i++)
            {
                g[i] = index_of[g[i]];
            }
        }
        /// <summary>
        /// 인덱스 0이 상수항이고 최고차항이 인덱스N-1번째에 있다.
        /// </summary>
        /// <param name="msg"></param>
        public void encode_rs(int[] msg)
        {
            int[] b = new int[length - k];

            for (int i = k - 1; i >= 0; i--)
            {
                int feedback = index_of[msg[i] ^ b[length - k - 1]];
                if (feedback != -1)
                {
                    for (int j = length - k - 1; j > 0; j--)
                    {
                        if (g[j] != -1)
                        {
                            b[j] = b[j - 1] ^ alpha_to[(g[j] + feedback) % n];
                        }
                        else
                        {
                            b[j] = b[j - 1];
                        }
                    }
                    b[0] = alpha_to[(g[0] + feedback) % n];
                }
                else
                {
                    for (int j = length - k - 1; j > 0; j--)
                    {
                        b[j] = b[j - 1];
                    }
                    b[0] = 0;
                }
            }
            for (int i = 0; i < length - k; i++)
            {
                encoded[i] = b[i];
            }
            for (int i = 0; i < k; i++)
            {
                encoded[i + length - k] = msg[i];
            }
        }

        public int[] 나누기(QRData data, int n)
        {
            int[] fx = new int[26];
            for (int i = 0; i < data.파라메터.데이터코드갯수; i++)
            {
                fx[i] = data.데이터코드배열[i];
            }

            return fx;

        }
    }

}
