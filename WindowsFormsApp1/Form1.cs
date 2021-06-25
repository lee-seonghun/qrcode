using OpenCvSharp;
using OpenCvSharp.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            for(int i=0; i< Enum.GetNames(typeof(QRCode파라메터.에러모드종류)).Length;
                i++)
            {
                comboBoxEorrorMode.Items.Add(QRCode파라메터.에러모드문자열(i));
            }

            comboBoxEorrorMode.SelectedIndex = 0;
        }


        QRCode qr = null;
        Mat qrMat;
        QRCode파라메터.에러모드종류 에러모드종류 = QRCode파라메터.에러모드종류.H;

        private void button1_Click(object sender, EventArgs e)
        {
            // 메시지 분석 - 인코딩 테스트
            // 1. 모두 숫자?
            // 2. 숫자 + 알파
            // else
            // 
            QRCode파라메터 파라메터 = new QRCode파라메터();

#if false
            파라메터.초기화(QRCode파라메터.버전종류.Ver1, 
                QRCode파라메터.에러모드종류.H, 
                QRCode파라메터.인코딩종류.알파벳A);
            qr = new QRCode(파라메터);
            qr.데이터생성("ABCDE123");
#else
            
            파라메터.초기화(QRCode파라메터.버전종류.Ver1,
                에러모드종류,
                QRCode파라메터.인코딩종류.알파벳A);
            qr = new QRCode(파라메터);
            qr.데이터생성(textBoxData.Text);

            //qr.데이터생성("HELLO WORLD");
#endif
            //qr.데이터생성("A23ABCDE");
            //qr.데이터생성("VER1");

            qr.셀생성();
            qr.파인터생성();
            qr.타이밍패턴생성();
            qr.예약영역생성();
           qr.print();
            qr.데이터패턴생성();

           qr.print();
            qr.마스크패턴();
//             qr.포멧비트생성();
            qr.print();
            //qr.cellvalue();

            //pictureBox1.Invalidate();
            qrMat = QRDataDraw(pictureBox1.Size, qr.파라메터.셀크기, false);

            pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(qrMat);
        }

        private Mat QRDataDraw(System.Drawing.Size s, int 셀크기, bool 에러비트함)
        {
            Mat ret;

            if (에러비트함)
            {
                ret = new Mat(s.Height, s.Width, MatType.CV_8SC3);
                ret.SetTo(new Scalar(255, 2555, 2555));
            }
            else
            {
                ret = new Mat(s.Height, s.Width, MatType.CV_8UC1);
                ret.SetTo(new Scalar(255));
            }
            

            int ox = s.Width / 2;
            int oy = s.Height / 2;
            //int d = 10;

            px = ox - d * 셀크기 / 2;
            py = oy - d * 셀크기 / 2;

            int tpy = py;
            for (int y = 0; y < 셀크기; y++, tpy += d)
            {
                int tpx = px;
                for (int x = 0; x < 셀크기; x++, tpx += d)
                {
                    if (에러비트함 == false)
                    {
                        if (qr.셀[y][x].value == 1)
                        {
                            ret.Rectangle(new OpenCvSharp.Rect(tpx, tpy, d, d), new Scalar(0), -1);
                        }
                    }
                    else
                    {

                        if (qr.셀[y][x].value == 1)
                        {
                            ret.Rectangle(new OpenCvSharp.Rect(tpx, tpy, d, d), new Scalar(0, 0, 0), -1);
                        }
                        else if (qr.셀[y][x].value == 0)
                        {
                            //ret.Rectangle(new OpenCvSharp.Rect(tpx, tpy, d, d), new Scalar(0, 0, 0), -1);
                        }
                        else if (qr.셀[y][x].value == -1)
                        {
                            ret.Rectangle(new OpenCvSharp.Rect(tpx, tpy, d, d), new Scalar(0, 255, 0), -1);
                        }
                        else if (qr.셀[y][x].value == -2)
                        {
                            ret.Rectangle(new OpenCvSharp.Rect(tpx, tpy, d, d), new Scalar(0, 0, 255), -1);
                        }

                    }
                }
            }
            return ret;
        }

        int px = 30;
        int py = 30;
        int d = 10;

        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            System.Drawing.Point coordinates = me.Location;

            int xidx = (coordinates.X - px) / d;
            int yidx = (coordinates.Y - py) / d;

            if (xidx >=0 && qr.셀.Length > xidx
                && yidx >=0 && qr.셀.Length > yidx)
            {
                qr.셀[yidx][xidx].반전시킴();
            }
            pictureBox1.Refresh();

        }
        Mat blur;

        private void button2_Click(object sender, EventArgs e)
        {
            if (qrMat != null)
            {
                Mat mat = QRDataDraw(pictureBox2.Size, qr.파라메터.셀크기, false);

                int ox = mat.Cols / 2;
                int oy = mat.Rows / 2;

                Random ra = new Random();
                int angle = ra.Next(-30, 30);
                double sc = ra.Next(500, 1200) / 1000.0;

                Mat r = Cv2.GetRotationMatrix2D(new OpenCvSharp.Point(oy, ox), angle, sc);
                
                blur = mat.GaussianBlur(new OpenCvSharp.Size(5, 5), 0);
                Cv2.WarpAffine(blur, blur, r, blur.Size(), InterpolationFlags.Cubic, BorderTypes.Constant, 255);

                //blur.Randn(new Scalar(0.0), new Scalar(30.0));
                Mat noise = new Mat(blur.Size(), MatType.CV_64F);
                
                Mat blur64 = blur.Normalize(0.0, 1.0, NormTypes.MinMax, MatType.CV_64F);
                blur64 = blur64.GaussianBlur(new OpenCvSharp.Size(3, 3), 0);
                noise.Randn(0.0, 0.05);
                blur64 = blur64 + noise;
                blur64 = blur64.Normalize(0.0, 255.0, NormTypes.MinMax, MatType.CV_64F);
                blur64.ConvertTo(blur, blur.Type());

                blur = blur.GaussianBlur(new OpenCvSharp.Size(3, 3),0);

                //blur = blur.GaussianBlur(new OpenCvSharp.Size(11, 11), 0);

                pictureBox2.Image = BitmapConverter.ToBitmap(blur);
            }

            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if(blur != null)
            {

                Mat binary = blur.Threshold(0, 255, ThresholdTypes.Otsu);
                //pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(binary);
                QRReader reader = new QRReader();
                if(reader.find(binary) == true)
                {
                    Mat output;
                    if(reader.findAlignmentMarker(binary,out output))
                    {
                        for(int y=0; y < qr.셀.Length;y ++)
                        {
                            for(int x=0; x<qr.셀.Length; x++)
                            {
                                byte pixel = output.Get<byte>(y, x);

                                Console.WriteLine("[({0} {1}) {2}, {3}", x, y, qr.셀[y][x].value, pixel < 128 ? 1 : 0);
                            }
                        }
                        pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(output);
                    }
                }
            }
        }
        int _에러비트생성수;

        public event EventHandler 에러비트생성수변경됨; // or via the "Events" list
        public int 에러비트생성수
        {
            get { return _에러비트생성수; }
            set
            {
                if (value != _에러비트생성수)
                {
                    _에러비트생성수 = value;
                    에러비트생성수변경됨?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox에러비트수.DataBindings.Add("Text", this, "에러비트생성수");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            int 남은에러비트생성개수 = 에러비트생성수;
            Random r = new Random();
            while(남은에러비트생성개수 > 0)
            {
                int x = r.Next(qr.파라메터.셀크기);
                int y = r.Next(qr.파라메터.셀크기);

                if(qr.셀[y][x].유형 == QRCodeCell.셀유형.데이터)
                {
                    if(qr.셀[y][x].value >=0)
                    {
                        qr.셀[y][x].반전시킴();
                        남은에러비트생성개수--;
                    }
                }
            }
            qrMat = QRDataDraw(pictureBox1.Size, qr.파라메터.셀크기, false);
            pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(qrMat);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            for(int y=0; y< qr.파라메터.셀크기; y++)
            {
                for(int x=0;x <qr.파라메터.셀크기; x++)
                {
                    qr.셀[y][x].값되돌리기();
                }
            }
            qrMat = QRDataDraw(pictureBox1.Size, qr.파라메터.셀크기, false);
            pictureBox1.Image = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(qrMat);
        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            System.Drawing.Point coordinates = me.Location;

            if(blur == null)
            {
                return;
            }

            IntPtr linePtr = blur.Ptr(coordinates.Y);
            byte[] linepixel = new byte[blur.Cols];

            Marshal.Copy(linePtr, linepixel, 0, linepixel.Length);

            int 최대높이 = pictureBox3.Height;
            double 비율 = 최대높이 / 255.0;

            var g = pictureBox3.CreateGraphics();
            g.Clear(Color.White);
            /*
            List<System.Drawing.Point> 점 = new List<System.Drawing.Point>();
            for(int x=0;x<linepixel.Length; x++)
            {
                System.Drawing.Point t = new System.Drawing.Point();
                t.X = x;
                t.Y = (int)((double)최대높이 - (double)linepixel[x] * 비율);
                점.Add(t);
            }
            g.DrawLines(myPen, 점.ToArray());

            */

            var edges = 영상처리.줄단위edge검출(linepixel);

            Pen myPen = new Pen(Color.Black, 1.0F);
            myPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

            Pen edgePen = new Pen(Color.Red, 1.0F);
            edgePen.DashStyle = System.Drawing.Drawing2D.DashStyle.Solid;

            System.Drawing.Point p1 = new System.Drawing.Point();
            p1.X = 0;
            p1.Y = (int)((double)최대높이 - (double)linepixel[0] * 비율);
            
            for(int x=1; x<linepixel.Length; x++)
            {
                System.Drawing.Point p2 = new System.Drawing.Point();
                p2.X = x;
                p2.Y = (int)((double)최대높이 - (double)linepixel[x] * 비율);

                if(edges[x] != 0)
                {
                    g.DrawLine(edgePen, p1, p2);
                }
                else
                {
                    g.DrawLine(myPen, p1, p2);
                }
                p1 = p2; 
            }
            g.Dispose();
            myPen.Dispose();
            edgePen.Dispose();

            Graphics g2 = pictureBox2.CreateGraphics();
            g2.DrawLine(new Pen(Color.Red, 1.0f),
                new System.Drawing.Point(0, coordinates.Y),
                new System.Drawing.Point(pictureBox2.Width, coordinates.Y));
            g2.Dispose();


        }

        private void comboBoxEorrorMode_SelectedIndexChanged(object sender, EventArgs e)
        {

            if(comboBoxEorrorMode.SelectedIndex < 0)
            {
                return;
            }
            에러모드종류 = QRCode파라메터.에러모드종류변환(comboBoxEorrorMode.SelectedIndex);
        }
    }
}