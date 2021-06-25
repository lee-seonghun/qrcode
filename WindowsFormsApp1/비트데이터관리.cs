using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsFormsApp1
{
    public class 비트데이터
    {
        public int 값 { get; private set; }
        public int 비트길이 { get; private set; }

        public 비트데이터(int 비트길이, int value)
        {
            this.비트길이 = 비트길이;
            값 = value;
        }
    }
    public class 비트스트림
    {
        // 
        List<비트데이터> 데이터 = new List<비트데이터>();

        public int 총비트수
        {
            get
            {
                int bit수 = 0;
                foreach(var t in 데이터)
                {
                    bit수 += t.비트길이;
                }
                return bit수;
            }
        }
        public void 추가(int 비트길이, int 값)
        {
            데이터.Add(new 비트데이터(비트길이, 값));
        }

        public int 변환하기(int 변환비트길이, out List<int> 반환)
        {
            반환 = new List<int>();

            비트데이터 v = null;

            int srcdataidx = 0;
            int srcbitidx = 0;

            int dstbitidx = 변환비트길이 - 1;
            int dstvalue = 0;

            while(true)
            {
                if(srcbitidx == 0)
                {
                    if(srcdataidx == 데이터.Count)
                    {
                        break;
                    }
                    v = 데이터[srcdataidx];
                    srcbitidx = v.비트길이 - 1;
                    srcdataidx++;
                }
                else
                {
                    srcbitidx--;
                }

                int b = (v.값 >> srcbitidx) & 1;
                dstvalue |= (b << dstbitidx);
                if (dstbitidx == 0)
                {
                    반환.Add(dstvalue);
                    dstbitidx = 변환비트길이 - 1;
                    dstvalue = 0;
                }
                else
                {
                    dstbitidx--;
                }
            }

            if(dstbitidx != 변환비트길이 - 1)
            {
                반환.Add(dstvalue);
            }
            
            return (변환비트길이 - dstbitidx + 1);
        }


    }
}
