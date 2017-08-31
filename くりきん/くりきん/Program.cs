using System;
using System.Timers;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

class Program
{
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.Run(new Form1());
    }
}
class kin
{
    public int large;
    public int colarr;
    public int colarb;
    public int colarg;//  緑HP　赤攻撃速度　青増殖
    public int kinx;
    public int kiny;
    public int speedx;
    public int speedy;
    public int attacktime = 0;
    public int bubbletime = 0;
}
class dead
{
    public Stack enemy = new Stack();
    public Stack ally = new Stack();
}
class field
{
    public int X = 0;
    public int Y = 0;
}

class Form1 : Form
{
    int levelx = 0;
    const int BLOCK_SIZE = 100;
    static int[,] BS = new int[1000, 500];
    field fi = new field();
    field fu = new field();
    kin[] mikata = new kin[102];
    kin[] teki = new kin[102];
    dead bubble = new dead();
    bool cptimeflag = false;
    int cptime = 0;
    int maintimer = 0;
    int allypoint = 5;
    int enemypoint = 5;
    int readytime = 200;
    int enemylevel = 0;
    bool pause = false;
    bool gameend = false;
    public Form1()
    {
        for (int i = 0; i < BLOCK_SIZE * 10; i++) for (int j = 0; j < BLOCK_SIZE * 5; j++) BS[i, j] = 0;
        setup();
        var timer = new System.Timers.Timer();
        timer.Elapsed += new ElapsedEventHandler(timer_Tick);
        timer.Interval = 1;
        timer.Start();

        this.KeyDown += new KeyEventHandler(Form1_KeyDown);
        this.MouseDown += new MouseEventHandler(Form1_MouseDown);
        this.MouseUp += new MouseEventHandler(Form1_MouseUp);

        this.DoubleBuffered = true;  // ダブルバッファリング

        this.ClientSize = new Size(BLOCK_SIZE * 10, BLOCK_SIZE * 5);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.TopMost = true;
        this.MaximumSize = this.Size;
        this.MinimumSize = this.Size;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
    }
    void timer_Tick(object sender, EventArgs e)
    {
        if (enemylevel == 0 || gameend || pause) { }
        else if (readytime > 0) readytime--;
        else
        {
            if (readytime == 0) setupdeffi(enemylevel);
            readytime--;
            if (allypoint == 0 || enemypoint == 0 || 90 + readytime * 0.0151 < 0) { gameend = true; }
            maintimer++;
            if (maintimer > 250)
            {
                enemyAI();
                maintimer = 0;
            }
            int damege = 0;
            for (int i = 1; i < 101; i++)
            {
                if (mikata[i].large == 0)//死んだら移動をやめる
                {
                    mikata[i].speedx = 0;
                    mikata[i].speedy = 0;
                    continue;
                }

                /*  if (BS[mikata[i].kinx, mikata[i].kiny] != i)//重なったらはじく
                  {
                      if (mikata[i].kinx < BLOCK_SIZE * 4)
                          mikata[i].speedx = mikata[i].large;
                      else
                          mikata[i].speedx = -mikata[i].large;
                      if (mikata[i].kiny < BLOCK_SIZE * 2)
                          mikata[i].speedy = mikata[i].large;
                      else
                          mikata[i].speedy = -mikata[i].large;
                  }*/
                if (Math.Abs(mikata[i].speedx) < 4 && Math.Abs(mikata[i].speedy) < 4)
                {
                    damege = 0;
                    if (mikata[i].bubbletime <= 2000) mikata[i].bubbletime += mikata[i].colarb / (mikata[i].large + 1);
                    else mikata[i].bubbletime = 0;//分裂待機状態を無くす

                    mikata[i].attacktime += mikata[i].colarr * mikata[i].large;
                    if (mikata[i].attacktime > 1000)//味方の攻撃
                    {
                        mikata[i].attacktime = 0;
                        damege = enemycheck(mikata[i].kinx, mikata[i].kiny, mikata[i].large);//攻撃判定
                        if (damege > 1000)
                        {
                            if (teki[damege - 1000].colarg > 10) teki[damege - 1000].colarg -= 3;
                            else if (teki[damege - 1000].large != 0)
                            {
                                for (int j = 0; j < teki[damege - 1000].large; j++)
                                    for (int o = 0; o < teki[damege - 1000].large; o++)
                                        BS[teki[damege - 1000].kinx + j, teki[damege - 1000].kiny + o] = 0;
                                teki[damege - 1000].speedx = 0;
                                teki[damege - 1000].speedy = 0;
                                teki[damege - 1000].large = 0;
                                bubble.enemy.Push(damege - 1000);
                            }

                        }
                    }
                    if (damege == 0)
                    {
                        if (mikata[i].bubbletime > 2000 && bubble.ally.Count > 0 && mikata[i].large > 0)//分裂判定
                        {
                            mikata[i].bubbletime = 0;
                            allyborn(mikata[i].kinx, mikata[i].kiny, mikata[i].large, mikata[i].colarb, mikata[i].colarr, mikata[i].colarg);
                        }
                        searchdestroy(i, 1);
                    }
                }
                else
                {
                    mikata[i].attacktime = 0;//移動時攻撃をしない
                    mikata[i].bubbletime = 0;//移動時分裂しない
                    move(i);//移動

                }
            }

            for (int i = 1; i < 101; i++)//死んだら移動を辞める
            {
                if (teki[i].large == 0)
                {
                    teki[i].speedx = 0;
                    teki[i].speedy = 0;
                    continue;
                }

                /* if (BS[teki[i].kinx, teki[i].kiny] != i + 1000)//重なったらはじく
                 {
                     if (teki[i].kinx < BLOCK_SIZE * 4)
                         teki[i].speedx = teki[i].large;
                     else
                         teki[i].speedx = -teki[i].large;
                     if (teki[i].kiny < BLOCK_SIZE * 2)
                         teki[i].speedy = teki[i].large;
                     else
                         teki[i].speedy = -teki[i].large;
                 }*/
                if (Math.Abs(teki[i].speedx) < 4 && Math.Abs(teki[i].speedy) < 4)
                {
                    damege = 0;
                    if (teki[i].bubbletime <= 2000) teki[i].bubbletime += teki[i].colarb / (teki[i].large + 1);
                    else teki[i].bubbletime = 0;//分裂待機状態を無くす

                    teki[i].attacktime += teki[i].colarr * teki[i].large;
                    if (teki[i].attacktime > 1000)//敵の攻撃
                    {
                        teki[i].attacktime = 0;
                        damege = allycheck(teki[i].kinx, teki[i].kiny, teki[i].large);//攻撃判定
                        if (damege < 1000 && damege > 0)
                        {
                            if (mikata[damege].colarg > 10) mikata[damege].colarg -= 3;
                            else if (mikata[damege].large != 0)
                            {
                                for (int j = 0; j < mikata[damege].large; j++)
                                    for (int o = 0; o < mikata[damege].large; o++)
                                        BS[mikata[damege].kinx + j, mikata[damege].kiny + o] = 0;
                                mikata[damege].speedx = 0;
                                mikata[damege].speedy = 0;
                                mikata[damege].large = 0;
                                bubble.ally.Push(damege);
                            }

                        }
                    }
                    if (damege == 0)
                    {
                        if (teki[i].bubbletime > 2000 && bubble.enemy.Count > 0 && teki[i].large > 0)//分裂判定
                        {
                            teki[i].bubbletime = 0;
                            enemyborn(teki[i].kinx, teki[i].kiny, teki[i].large, teki[i].colarb, teki[i].colarr, teki[i].colarg);
                        }
                        searchdestroy(i, 2);
                    }
                }
                else
                {
                    teki[i].attacktime = 0;//移動時攻撃禁止
                    teki[i].bubbletime = 0;//移動時分裂禁止
                    enemymove(i);//移動
                }
            }
            if (cptimeflag) cptime += 6;//円拡大
            else cptime = 0;

            reset();//マップにキン位置を再セット

        }
        this.Invalidate();  // 再描画を促す

    }
    void Form1_KeyDown(object sender, KeyEventArgs e)//キーを押した場所への加速
    {
        if (e.KeyCode == Keys.Space)
        {
            pause = !pause;
        }

    }
    void Form1_MouseDown(object sender, MouseEventArgs e)
    {
        if (e.Button == e.Button)
        {
            Point FP = Cursor.Position;       //フォーム内座標取得
            Point CP = this.PointToClient(FP);
            fi.X = CP.X;
            fi.Y = CP.Y;

            cptimeflag = true;//押してる間を実現押す
            if (gameend) regame();
        }

    }//クリックはじめ
    void Form1_MouseUp(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            Point FP = Cursor.Position;       //フォーム内座標取得
            Point CP = this.PointToClient(FP);
            fu.X = CP.X;
            fu.Y = CP.Y;
            cptimeflag = false;//押してる間を実現離す
            Circle(cptime / 2, fi.X, fi.Y, 0);
        }
        if (e.Button == MouseButtons.Right)
        {
            cptimeflag = false;//押してる間を実現離す
            Circle(cptime / 2, fi.X, fi.Y, 1);
        }

    }//クリック終わり
    protected override void OnPaint(PaintEventArgs e)
    {

        base.OnPaint(e);

        Pen blapen = new Pen(Color.Black);
        Pen blupen = new Pen(Color.Blue);
        Pen redpen = new Pen(Color.Red);

        // e.Graphics.DrawRectangle(blapen, 50, 50, BLOCK_SIZE * 9, BLOCK_SIZE * 4);//決戦のバトルフィールド

        SolidBrush moti = new SolidBrush(Color.FromArgb(50, 50, 50));
        e.Graphics.FillEllipse(moti, fi.X - cptime / 2, fi.Y - cptime / 2, cptime, cptime); //キン補足円
        moti.Dispose();
        if (cptimeflag)
        {
            Point FP = Cursor.Position;       //フォーム内座標取得
            Point CP = this.PointToClient(FP);
            if (maintimer > 0) e.Graphics.DrawLine(Pens.Black, fi.X, fi.Y, CP.X, CP.Y);
        }

        for (int i = 1; i < 101; i++)//敵味方描画
        {
            SolidBrush kinAcolor = new SolidBrush(Color.FromArgb(mikata[i].colarr, mikata[i].colarg, mikata[i].colarb));
            SolidBrush tekicolor = new SolidBrush(Color.FromArgb(teki[i].colarr, teki[i].colarg, teki[i].colarb));

            e.Graphics.FillEllipse(kinAcolor, mikata[i].kinx, mikata[i].kiny, mikata[i].large, mikata[i].large);
            e.Graphics.DrawEllipse(blupen, mikata[i].kinx, mikata[i].kiny, mikata[i].large, mikata[i].large);

            e.Graphics.FillEllipse(tekicolor, teki[i].kinx, teki[i].kiny, teki[i].large, teki[i].large);
            e.Graphics.DrawEllipse(redpen, teki[i].kinx, teki[i].kiny, teki[i].large, teki[i].large);
            kinAcolor.Dispose();
            tekicolor.Dispose();
        }

        Font mofo = new Font("Times New Roman", 10, FontStyle.Regular);
        Font fon = new Font("Times New Roman", 50, FontStyle.Regular);
        Font bfon = new Font("Times New Roman", 100, FontStyle.Regular);
        if (enemylevel == 0)
        {
            Point SP = Cursor.Position;       //フォーム内座標取得
            Point NP = this.PointToClient(SP);
            if (fu.X / 3 * 2 / BLOCK_SIZE + fu.Y * 3 / BLOCK_SIZE / 5 * 5 <= 10)
                levelx = fu.X / 3 * 2 / BLOCK_SIZE + fu.Y * 3 / BLOCK_SIZE / 5 * 5 % 11;
            e.Graphics.DrawString("LEVEL" + levelx, fon, Brushes.Black, BLOCK_SIZE * 4, BLOCK_SIZE * 2);
            for (int counter = 1; counter <= 10; counter++)
            {
                if (NP.X / 3 * 2 / BLOCK_SIZE == ((counter - 1) % 5) + 1 && NP.Y / 2 / BLOCK_SIZE == counter / 6) e.Graphics.DrawString("" + counter, bfon, Brushes.Red, BLOCK_SIZE * 3 / 2 * ((counter - 1) % 5 + 1), BLOCK_SIZE * 2 * (counter / 6) + 50);
                else e.Graphics.DrawString("" + counter, bfon, Brushes.Black, BLOCK_SIZE * 3 / 2 * ((counter - 1) % 5 + 1), BLOCK_SIZE * 2 * (counter / 6) + 50);
            }
            e.Graphics.DrawString("START", fon, Brushes.Black, BLOCK_SIZE * 4, BLOCK_SIZE * 4);
            if (BLOCK_SIZE * 4 < fu.Y && levelx > 0) enemylevel = levelx;
        }
        else if (gameend)
        {
            if (enemypoint == allypoint)
                e.Graphics.DrawString("DRAW", bfon, SystemBrushes.WindowText, BLOCK_SIZE * 3, BLOCK_SIZE * 2 - 50);
            else if (enemypoint < allypoint)
                e.Graphics.DrawString("YOU WIN", bfon, SystemBrushes.WindowText, BLOCK_SIZE * 2, BLOCK_SIZE * 2 - 50);
            else if (enemypoint > allypoint)
                e.Graphics.DrawString("YOU LOSE", bfon, SystemBrushes.WindowText, BLOCK_SIZE * 2 - 50, BLOCK_SIZE * 2 - 50);
            e.Graphics.DrawString("Click to Restart", fon, SystemBrushes.WindowText, BLOCK_SIZE * 3, BLOCK_SIZE * 3);
        }
        else if (pause) e.Graphics.DrawString("PAUSE\n-space-", bfon, SystemBrushes.WindowText, BLOCK_SIZE * 3 - 25, BLOCK_SIZE * 2 - 50);
        else if (readytime > 0 && readytime < 150) e.Graphics.DrawString("ready?\n" + readytime * 0.0151, fon, SystemBrushes.WindowText, BLOCK_SIZE * 10 / 2 - 110, BLOCK_SIZE * 5 / 2 - 50 * 2);
        else if (readytime > -50 && readytime < 0)
            e.Graphics.DrawString("GO!!", bfon, SystemBrushes.WindowText, BLOCK_SIZE * 4 - 50, BLOCK_SIZE * 2 - 50);
        else if (readytime < 0)
        {
            e.Graphics.DrawString("敵" + enemypoint, mofo, Brushes.Red, BLOCK_SIZE * 8, BLOCK_SIZE / 4 - 10);
            e.Graphics.DrawString("LEVEL" + enemylevel, mofo, Brushes.Blue, BLOCK_SIZE * 5, BLOCK_SIZE / 4 - 10);
            e.Graphics.DrawString("味方" + allypoint, mofo, Brushes.Blue, BLOCK_SIZE * 3, BLOCK_SIZE / 4 - 10);
            e.Graphics.DrawString((int)((90 + readytime * 0.0151) / 60) + ":" + (int)((90 + readytime * 0.0151) % 60), fon, SystemBrushes.WindowText, BLOCK_SIZE * 5 - 50, BLOCK_SIZE / 4 + 10);

        }


        mofo.Dispose();
        fon.Dispose();
        bfon.Dispose();

        blupen.Dispose();
        blapen.Dispose();
        redpen.Dispose();
    }
    private int enemycheck(int xposition, int yposition, int largest)//その周辺に敵がいるか攻撃版
    {
        if (largest == 0) return 0;
        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition - 5; j <= yposition - 3; j++)                                          //上
                if (BS[i, j] > 1000) return BS[i, j];

        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition + largest + 3; j <= yposition + largest + 5; j++)                      //下
                if (BS[i, j] > 1000) return BS[i, j];

        for (int i = xposition - 5; i <= xposition - 3; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //左
                if (BS[i, j] > 1000) return BS[i, j];

        for (int i = xposition + largest + 3; i <= xposition + largest + 5; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //右
                if (BS[i, j] > 1000) return BS[i, j];
        return 0;
    }
    private int enemyserch(int xposition, int yposition, int largest)//その周辺に敵がいるか索敵版上1下2左3右4
    {
        if (largest == 0) return 0;
        for (int i = xposition - largest; i < xposition + largest; i++)
            for (int j = yposition - largest; j < yposition + largest; j++)
                if (BS[i, j] > 1000) return 0;

        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition - largest * 3; j <= yposition - largest; j++)                          //上
                if (BS[i, j] > 1000) return 1;

        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition + largest * 2; j <= yposition + largest * 4; j++)                      //下
                if (BS[i, j] > 1000) return 2;

        for (int i = xposition - largest * 3; i <= xposition - largest; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //左
                if (BS[i, j] > 1000) return 3;

        for (int i = xposition + largest * 2; i <= xposition + largest * 4; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //右
                if (BS[i, j] > 1000) return 4;
        return 0;
    }
    private int allycheck(int xposition, int yposition, int largest)//その周辺に味方がいるか攻撃版
    {
        if (largest == 0) return 0;
        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition - 5; j <= yposition - 3; j++)                                          //上
                if (BS[i, j] < 1000 && BS[i, j] > 0) return BS[i, j];

        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition + largest + 3; j <= yposition + largest + 5; j++)                      //下
                if (BS[i, j] < 1000 && BS[i, j] > 0) return BS[i, j];

        for (int i = xposition - 5; i <= xposition + largest - 3; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //左
                if (BS[i, j] < 1000 && BS[i, j] > 0) return BS[i, j];

        for (int i = xposition + largest + 3; i <= xposition + largest + 5; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //右
                if (BS[i, j] < 1000 && BS[i, j] > 0) return BS[i, j];
        return 0;
    }
    private int allyserch(int xposition, int yposition, int largest)//その周辺に味方がいるか索敵版上1下2左3右4
    {
        if (largest == 0) return 0;

        for (int i = xposition - largest; i < xposition + largest; i++)
            for (int j = yposition - largest; j < yposition + largest; j++)
                if (BS[i, j] < 1000 && BS[i, j] > 0) return 0;

        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition - largest * 3; j <= yposition - largest; j++)                          //上
                if (BS[i, j] < 1000 && BS[i, j] > 0) return 1;

        for (int i = xposition; i <= xposition + largest; i++)
            for (int j = yposition + largest * 2; j <= yposition + largest * 4; j++)                      //下
                if (BS[i, j] < 1000 && BS[i, j] > 0) return 2;

        for (int i = xposition - largest * 3; i <= xposition - largest; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //左
                if (BS[i, j] < 1000 && BS[i, j] > 0) return 3;

        for (int i = xposition + largest * 2; i <= xposition + largest * 4; i++)
            for (int j = yposition; j <= yposition + largest; j++)                                        //右
                if (BS[i, j] < 1000 && BS[i, j] > 0) return 4;
        return 0;
    }
    private void reset()    //マップの再読み込み
    {
        for (int i = 0; i < BLOCK_SIZE * 10; i++) for (int j = 0; j < BLOCK_SIZE * 5; j++) BS[i, j] = 0;//マップ更新
        int liepont = 0;
        for (int i = 1; i < 101; i++)
        {
            if (mikata[i].large == 0)
            {
                if (BS[mikata[i].kinx, mikata[i].kiny] == i) BS[mikata[i].kinx, mikata[i].kiny] = 0;
                continue;
            }
            else liepont++;
            for (int j = 0; j < mikata[i].large; j++)
                for (int o = 0; o < mikata[i].large; o++) BS[mikata[i].kinx + j, mikata[i].kiny + o] = i;
        }
        allypoint = liepont;
        liepont = 0;
        for (int i = 1; i < 101; i++)
        {
            if (teki[i].large == 0)
            {
                if (BS[teki[i].kinx, teki[i].kiny] == i + 1000) BS[teki[i].kinx, teki[i].kiny] = 0;
                continue;
            }
            else liepont++;

            for (int j = 0; j < teki[i].large; j++)
                for (int o = 0; o < teki[i].large; o++) BS[teki[i].kinx + j, teki[i].kiny + o] = i + 1000;
        }
        enemypoint = liepont;
    }
    private void Circle(int rad, int centerx, int centery, int RL)//円の中のキンにスピードを与える
    {
        for (int cix = centerx - rad; cix < centerx + rad; cix++)
        {
            if (cix < BLOCK_SIZE - 50 || cix > BLOCK_SIZE * 10 - 50) continue;
            for (int ciy = centery - rad; ciy < centery + rad; ciy++)
            {
                if (ciy < BLOCK_SIZE - 50 || ciy > BLOCK_SIZE * 5 - 50) continue;
                int ciflag = (cix - centerx) * (cix - centerx) + (ciy - centery) * (ciy - centery) - rad * rad;
                if (ciflag > 0) continue;
                int bsnunber = BS[cix, ciy];
                if (bsnunber > 0 && bsnunber < 102)
                {
                    Point FP = Cursor.Position;       //フォーム内座標取得
                    Point CP = this.PointToClient(FP);
                    if (RL == 0)
                    {
                        mikata[bsnunber].speedx = CP.X - fi.X;
                        mikata[bsnunber].speedy = CP.Y - fi.Y;
                    }
                    else if (RL == 1)
                    {
                        if (Math.Abs(CP.X - fi.X) < rad && Math.Abs(CP.Y - fi.Y) < rad)
                        {
                            mikata[bsnunber].speedx = fi.X - mikata[bsnunber].kinx;
                            mikata[bsnunber].speedy = fi.Y - mikata[bsnunber].kiny;
                        }
                        else
                        {
                            mikata[bsnunber].speedx = mikata[bsnunber].kinx - fi.X;
                            mikata[bsnunber].speedy = mikata[bsnunber].kiny - fi.Y;
                        }

                    }
                }
            }
        }
    }
    private void move(int i)//溜まってるスピードを使い移動
    {
        int speed;
        if (mikata[i].large < 5) speed = 4;
        else if (mikata[i].large < 7) speed = 3;
        else speed = 2;

        if (mikata[i].kinx > BLOCK_SIZE * 10 - 50 - mikata[i].large) { mikata[i].kinx--; }
        else if (mikata[i].kinx < BLOCK_SIZE - 50) { mikata[i].kinx++; }
        else if (mikata[i].kiny > BLOCK_SIZE * 5 - 50 - mikata[i].large) { mikata[i].kiny--; }
        else if (mikata[i].kiny < BLOCK_SIZE - 50) { mikata[i].kiny++; }
        else
        {
            if (mikata[i].kinx < BLOCK_SIZE * 10 - 50 - mikata[i].large)
                if (mikata[i].speedx >= speed)//右  
                {
                    int checkflag = check(mikata[i].kinx, mikata[i].kiny, mikata[i].large, 4);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kinx - mikata[i].large - 2 < BLOCK_SIZE * 10 - 50 - mikata[i].large)
                                mikata[i].kinx = teki[checkflag - 1000].kinx - mikata[i].large - 2;
                        }
                        else if (mikata[checkflag].kinx - mikata[i].large - 2 < BLOCK_SIZE * 10 - 50 - mikata[i].large)
                            mikata[i].kinx = mikata[checkflag].kinx - mikata[i].large - 2;
                        mikata[i].speedx -= speed;
                    }
                    else
                    {
                        if (Math.Abs(mikata[i].speedx) > Math.Abs(mikata[i].speedy))
                        {
                            mikata[i].speedx -= speed;
                            mikata[i].kinx += speed;
                        }
                        else// 斜め移動の時に移動を円滑に
                        {
                            mikata[i].speedx -= speed / 2;
                            mikata[i].kinx += speed / 2;
                        }
                    }
                }

            if (mikata[i].kinx > BLOCK_SIZE - 50)
                if (mikata[i].speedx <= -speed)//左
                {
                    int checkflag = check(mikata[i].kinx, mikata[i].kiny, mikata[i].large, 3);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kinx + mikata[i].large + 2 > 50)
                                mikata[i].kinx = teki[checkflag - 1000].kinx + mikata[i].large + 2;
                        }
                        else if (mikata[checkflag].kinx + mikata[i].large + 2 > 50)
                            mikata[i].kinx = mikata[checkflag].kinx + mikata[i].large + 2;
                        mikata[i].speedx += speed;
                    }
                    else
                    {

                        if (Math.Abs(mikata[i].speedx) > Math.Abs(mikata[i].speedy))
                        {
                            mikata[i].speedx += speed;
                            mikata[i].kinx -= speed;
                        }
                        else
                        {
                            mikata[i].speedx += speed / 2;
                            mikata[i].kinx -= speed / 2;
                        }
                    }
                }
            if (mikata[i].kiny < BLOCK_SIZE * 5 - 50 - mikata[i].large)
                if (mikata[i].speedy >= speed)//下
                {
                    int checkflag = check(mikata[i].kinx, mikata[i].kiny, mikata[i].large, 2);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kiny - mikata[i].large - 2 < BLOCK_SIZE * 5 - 50 - mikata[i].large)
                                mikata[i].kiny = teki[checkflag - 1000].kiny - mikata[i].large - 2;
                        }
                        else if (mikata[checkflag].kiny - mikata[i].large - 2 < BLOCK_SIZE * 5 - 50 - mikata[i].large)
                            mikata[i].kiny = mikata[checkflag].kiny - mikata[i].large - 2;
                        mikata[i].speedy -= speed;
                    }
                    else
                    {
                        if (Math.Abs(mikata[i].speedx) < Math.Abs(mikata[i].speedy))
                        {
                            mikata[i].speedy -= speed;
                            mikata[i].kiny += speed;
                        }
                        else
                        {
                            mikata[i].speedy -= speed / 2;
                            mikata[i].kiny += speed / 2;
                        }
                    }
                }
            if (mikata[i].kiny > BLOCK_SIZE - 50)
                if (mikata[i].speedy <= -speed)//上
                {
                    int checkflag = check(mikata[i].kinx, mikata[i].kiny, mikata[i].large, 1);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kiny + mikata[i].large + 2 > BLOCK_SIZE - 50)
                                mikata[i].kiny = teki[checkflag - 1000].kiny + mikata[i].large + 2;
                        }
                        else if (mikata[checkflag].kiny + mikata[i].large + 2 > BLOCK_SIZE - 50)
                            mikata[i].kiny = mikata[checkflag].kiny + mikata[i].large + 2;
                        mikata[i].speedy += speed;
                    }
                    else
                    {
                        if (Math.Abs(mikata[i].speedx) < Math.Abs(mikata[i].speedy))
                        {
                            mikata[i].speedy += speed;
                            mikata[i].kiny -= speed;
                        }
                        else
                        {
                            mikata[i].speedy += speed / 2;
                            mikata[i].kiny -= speed / 2;
                        }
                    }
                }
        }
    }
    private void enemymove(int i)//溜まってるスピードを使い移動敵
    {
        int speed;
        if (teki[i].large < 5) speed = 4;
        else if (teki[i].large < 7) speed = 3;
        else speed = 2;
        if (teki[i].kinx > BLOCK_SIZE * 10 - 50 - teki[i].large) { teki[i].kinx--; }
        else if (teki[i].kinx < BLOCK_SIZE - 50) { teki[i].kinx++; }
        else if (teki[i].kiny > BLOCK_SIZE * 5 - 50 - teki[i].large) { teki[i].kiny--; }
        else if (teki[i].kiny < BLOCK_SIZE - 50) { teki[i].kiny++; }
        else
        {
            if (teki[i].kinx < BLOCK_SIZE * 10 - 50 - teki[i].large)
                if (teki[i].speedx >= speed)//右  
                {
                    int checkflag = check(teki[i].kinx, teki[i].kiny, teki[i].large, 4);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kinx - teki[i].large - 2 < BLOCK_SIZE * 10 - 50 - teki[i].large)
                                teki[i].kinx = teki[checkflag - 1000].kinx - teki[i].large - 2;
                        }
                        else if (mikata[checkflag].kinx - teki[i].large - 2 < BLOCK_SIZE * 10 - 50 - teki[i].large)
                            teki[i].kinx = mikata[checkflag].kinx - teki[i].large - 2;
                        teki[i].speedx -= speed;
                    }
                    else
                    {
                        if (Math.Abs(teki[i].speedx) > Math.Abs(teki[i].speedy))
                        {
                            teki[i].speedx -= speed;
                            teki[i].kinx += speed;
                        }
                        else// 斜め移動の時に移動を円滑に
                        {
                            teki[i].speedx -= speed / 2;
                            teki[i].kinx += speed / 2;
                        }
                    }
                }

            if (teki[i].kinx > BLOCK_SIZE - 50)
                if (teki[i].speedx <= -speed)//左
                {
                    int checkflag = check(teki[i].kinx, teki[i].kiny, teki[i].large, 3);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kinx + teki[i].large + 2 > BLOCK_SIZE - 50)
                                teki[i].kinx = teki[checkflag - 1000].kinx + teki[i].large + 2;
                        }
                        else if (mikata[checkflag].kinx + teki[i].large + 2 > BLOCK_SIZE - 50)
                            teki[i].kinx = mikata[checkflag].kinx + teki[i].large + 2;
                        teki[i].speedx += speed;
                    }
                    else
                    {

                        if (Math.Abs(teki[i].speedx) > Math.Abs(teki[i].speedy))
                        {
                            teki[i].speedx += speed;
                            teki[i].kinx -= speed;
                        }
                        else
                        {
                            teki[i].speedx += speed / 2;
                            teki[i].kinx -= speed / 2;
                        }
                    }
                }
            if (teki[i].kiny < BLOCK_SIZE * 5 - 50 - teki[i].large)
                if (teki[i].speedy >= speed)//下
                {
                    int checkflag = check(teki[i].kinx, teki[i].kiny, teki[i].large, 2);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kiny - teki[i].large - 2 < BLOCK_SIZE * 5 - 50 - teki[i].large)
                                teki[i].kiny = teki[checkflag - 1000].kiny - teki[i].large - 2;
                        }
                        else if (mikata[checkflag].kiny - teki[i].large - 2 < BLOCK_SIZE * 5 - 50 - teki[i].large)
                            teki[i].kiny = mikata[checkflag].kiny - teki[i].large - 2;
                        teki[i].speedy -= speed;
                    }
                    else
                    {
                        if (Math.Abs(teki[i].speedx) < Math.Abs(teki[i].speedy))
                        {
                            teki[i].speedy -= speed;
                            teki[i].kiny += speed;
                        }
                        else
                        {
                            teki[i].speedy -= speed / 2;
                            teki[i].kiny += speed / 2;
                        }
                    }
                }
            if (teki[i].kiny > BLOCK_SIZE - 50)
                if (teki[i].speedy <= -speed)//上
                {
                    int checkflag = check(teki[i].kinx, teki[i].kiny, teki[i].large, 1);
                    if (checkflag > 0 && checkflag != i)//あたり判定
                    {
                        if (checkflag > 1000)
                        {
                            if (teki[checkflag - 1000].kiny + teki[i].large + 2 > BLOCK_SIZE - 50)
                                teki[i].kiny = teki[checkflag - 1000].kiny + teki[i].large + 2;
                        }
                        else if (mikata[checkflag].kiny + teki[i].large + 2 > BLOCK_SIZE - 50)
                            teki[i].kiny = mikata[checkflag].kiny + teki[i].large + 2;
                        teki[i].speedy += speed;
                    }
                    else
                    {
                        if (Math.Abs(teki[i].speedx) < Math.Abs(teki[i].speedy))
                        {
                            teki[i].speedy += speed;
                            teki[i].kiny -= speed;
                        }
                        else
                        {
                            teki[i].speedy += speed / 2;
                            teki[i].kiny -= speed / 2;
                        }
                    }
                }
        }
    }
    private void searchdestroy(int i, int EneAlly)//見つけて殺す
    {
        int destroy;
        if (EneAlly == 1)
        {
            destroy = enemyserch(mikata[i].kinx, mikata[i].kiny, mikata[i].large);
            if (destroy == 1)
                mikata[i].speedy -= 10;
            else if (destroy == 2)
                mikata[i].speedy += 10;
            else if (destroy == 3)
                mikata[i].speedx -= 10;
            else if (destroy == 4)
                mikata[i].speedx += 10;
        }
        if (EneAlly == 2)
        {
            destroy = allyserch(teki[i].kinx, teki[i].kiny, teki[i].large);
            if (destroy == 1)
                teki[i].speedy -= 10;
            else if (destroy == 2)
                teki[i].speedy += 10;
            else if (destroy == 3)
                teki[i].speedx -= 10;
            else if (destroy == 4)
                teki[i].speedx += 10;
        }

    }
    private int check(int xposition, int yposition, int largest, int udrl)//その周辺になにかいるかあたり判定用
    {
        if (largest == 0) return 0;
        if (udrl == 1) for (int i = xposition; i <= xposition + largest; i++)
                for (int j = 1; j < 4; j++) if (BS[i, yposition - j] > 0) return BS[i, yposition - j];                            //上
        if (udrl == 2) for (int i = xposition; i <= xposition + largest; i++)
                for (int j = 1; j < 4; j++) if (BS[i, yposition + largest + j] > 0) return BS[i, yposition + largest + j];        //下
        if (udrl == 3) for (int j = yposition; j <= yposition + largest; j++)
                for (int i = 1; i < 4; i++) if (BS[xposition - i, j] > 0) return BS[xposition - i, j];                          //左
        if (udrl == 4) for (int j = yposition; j <= yposition + largest; j++)
                for (int i = 1; i < 4; i++) if (BS[xposition + largest + i, j] > 0) return BS[xposition + largest + i, j];      //右
        return 0;
    }
    private int borncheck(int xposition, int yposition, int largest, int blue, int red, int green)//分裂判定1味方2敵
    {
        if (largest == 0) return 0;
        bool UnderRight = true;
        bool Right = true;
        bool UpRight = true;
        bool Up = true;
        bool UpLeft = true;
        bool Left = true;
        bool UnderLeft = true;
        bool Under = true;

        for (int x = xposition + largest + 2; x < xposition + largest * 2 + 2; x++)
        {
            for (int y = yposition + largest + 2; y < yposition + largest * 2 + 2; y++) if (BS[x, y] > 0) { UnderRight = false; break; }
            for (int y = yposition; y < yposition + largest; y++) if (BS[x, y] > 0) { Right = false; break; }
            for (int y = yposition - largest - 2; y < yposition - 2; y++) if (BS[x, y] > 0) { UpRight = false; break; }
        }
        for (int x = xposition - largest - 2; x < xposition - 2; x++)
        {
            for (int y = yposition + largest + 2; y < yposition + largest * 2 + 2; y++) if (BS[x, y] > 0) { UnderLeft = false; break; }
            for (int y = yposition; y < yposition + largest; y++) if (BS[x, y] > 0) { Left = false; break; }
            for (int y = yposition - largest - 2; y < yposition - 2; y++) if (BS[x, y] > 0) { UpLeft = false; break; }
        }
        for (int x = xposition; x < xposition + largest; x++)
        {
            for (int y = yposition + largest + 2; y < yposition + largest * 2 + 2; y++) if (BS[x, y] > 0) { Under = false; break; }
            for (int y = yposition - largest - 2; y < yposition - 2; y++) if (BS[x, y] > 0) { Up = false; break; }
        }
        if (Right) return 3;
        else if (Left) return 9;
        else if (Up) return 12;
        else if (Under) return 6;
        else if (UpRight) return 1;
        else if (UnderRight) return 5;
        else if (UpLeft) return 11;
        else if (UnderLeft) return 7;
        else return 0;
    }
    private void allyborn(int xposition, int yposition, int largest, int blue, int red, int green)
    {
        if (largest == 0) return;
        int ans = borncheck(xposition, yposition, largest, blue, red, green);
        if (ans == 0) return;
        int changex = 0;
        int changey = 0;
        if (ans > 0 && ans < 6) changex = largest + 2;
        else if (ans > 6 && ans < 12) changex = -largest - 2;
        if (ans > 9 || ans < 3) changey = largest + 2;
        else if (ans > 3 && ans < 9) changey = -largest - 2;

        if (BS[xposition, yposition] > 0 && BS[xposition, yposition] < BLOCK_SIZE * 10)
        {

            if (xposition + changex > BLOCK_SIZE * 10 - 50 - largest) return;
            else if (xposition + changex < BLOCK_SIZE - 50) return;
            else if (yposition + changey > BLOCK_SIZE * 5 - 50 - largest) return;
            else if (yposition + changey < BLOCK_SIZE - 50) return;

            if (bubble.ally.Count <= 0) return;
            ans = (int)bubble.ally.Pop();
            mikata[ans].kinx = xposition + changex;
            mikata[ans].kiny = yposition + changey;
            mikata[ans].large = largest;
            mikata[ans].colarb = blue;
            mikata[ans].colarr = red;
            mikata[ans].colarg = green;
        }
        return;
    }
    private void enemyborn(int xposition, int yposition, int largest, int blue, int red, int green)
    {
        if (largest == 0) return;
        int ans = borncheck(xposition, yposition, largest, blue, red, green);
        if (ans == 0) return;
        int changex = 0;
        int changey = 0;
        if (ans > 0 && ans < 6) changex = largest + 2;
        else if (ans > 6 && ans < 12) changex = -largest - 2;
        if (ans > 9 || ans < 3) changey = largest + 2;
        else if (ans > 3 && ans < 9) changey = -largest - 2;


        if (BS[xposition, yposition] > BLOCK_SIZE * 10)
        {

            if (xposition + changex > BLOCK_SIZE * 10 - 50 - largest) return;
            else if (xposition + changex < BLOCK_SIZE - 50) return;
            else if (yposition + changey > BLOCK_SIZE * 5 - 50 - largest) return;
            else if (yposition + changey < BLOCK_SIZE - 50) return;
            if (bubble.enemy.Count <= 0) return;

            ans = (int)bubble.enemy.Pop();
            teki[ans].kinx = xposition + changex;
            teki[ans].kiny = yposition + changey;
            teki[ans].large = largest;
            teki[ans].colarb = blue;
            teki[ans].colarr = red;
            teki[ans].colarg = green;
        }
        return;
    }
    private void setup()
    {
        for (int i = 1; i < 101; i++)//初期設定
        {
            mikata[i] = new kin();
            mikata[i].speedx = 0;
            mikata[i].speedy = 0;
            mikata[i].large = 1;
            mikata[i].colarr = 50;
            mikata[i].colarb = 50;
            mikata[i].colarg = 50;
            mikata[i].kinx = 60;
            mikata[i].kiny = 60;

            teki[i] = new kin();
            teki[i].speedx = 0;
            teki[i].speedy = 0;
            teki[i].large = 1;
            teki[i].colarr = 50;
            teki[i].colarb = 50;
            teki[i].colarg = 50;
            teki[i].kinx = BLOCK_SIZE * 8;
            teki[i].kiny = 60;
        }
        bubble.ally.Clear();
        bubble.enemy.Clear();
    }//最初期設定値
    private void setupdeffi(int enemypower)
    {
        for (int i = 1; i < 101; i++)//初期設定
        {
            mikata[i] = new kin();
            mikata[i].speedx = 0;
            mikata[i].speedy = 0;
            if (i < 31) mikata[i].large = 8;
            else
            {
                mikata[i].large = 0;
                bubble.ally.Push(i);
            }
            mikata[i].colarr = 85;
            mikata[i].colarb = 85;
            mikata[i].colarg = 85;
            mikata[i].kinx = BLOCK_SIZE - 20 + ((i - 1) / 5) * (mikata[i].large + 10);
            mikata[i].kiny = BLOCK_SIZE - 20 + ((i - 1) % 5) * (mikata[i].large + 10);
            BS[mikata[i].kinx, mikata[i].kiny] = i;
            BS[mikata[i].kinx + 4, mikata[i].kiny] = i;
            BS[mikata[i].kinx, mikata[i].kiny + 4] = i;
            BS[mikata[i].kinx + 4, mikata[i].kiny + 4] = i;

            teki[i] = new kin();
            teki[i].speedx = 0;
            teki[i].speedy = 0;
            if (i < 31) teki[i].large = 8;
            else
            {
                teki[i].large = 0;
                bubble.enemy.Push(i);
            }
            teki[i].colarr = 80 + enemypower * 5;
            teki[i].colarb = 80 + enemypower * 5;
            teki[i].colarg = 80 + enemypower * 5;
            teki[i].kinx = BLOCK_SIZE * 9 - ((i - 1) / 5) * (teki[i].large + 10);
            teki[i].kiny = BLOCK_SIZE * 4 - ((i - 1) % 5) * (teki[i].large + 10);
            BS[teki[i].kinx, teki[i].kiny] = i + 1000;
            BS[teki[i].kinx + 4, teki[i].kiny] = i + 1000;
            BS[teki[i].kinx, teki[i].kiny + 4] = i + 1000;
            BS[teki[i].kinx + 4, teki[i].kiny + 4] = i + 1000;
        }
    }//最初期設定値
    private void regame()
    {
        fi = new field();
        mikata = new kin[102];
        teki = new kin[102];
        bubble = new dead();
        cptimeflag = false;
        cptime = 0;
        maintimer = 0;
        allypoint = 5;
        enemypoint = 5;
        readytime = 200;
        enemylevel = 0;
        pause = false;
        gameend = false;
        setup();
    }//再ゲーム初期化
    private void enemyAI()
    {
        int[,] AIfi = new int[100, 50];
        for (int re = 0; re < 100; re++)
            for (int set = 0; set < 50; set++)
                AIfi[re, set] = 0;
        for (int ally = 1; ally <= 100; ally++)
        {
            if (mikata[ally].large != 0) AIfi[mikata[ally].kinx / 10, mikata[ally].kiny / 10]++;
        }
        int bigx, bigy, bigz;
        bigx = 0; bigy = 0; bigz = 0;
        for (int re = 0; re < 100; re++)//敵最過密地域を検索
            for (int set = 0; set < 50; set++)
                if (bigz == 0)
                {
                    bigx = re;
                    bigy = set;
                    bigz = AIfi[re, set];
                }
                else if (bigz < AIfi[re, set])
                {
                    bigx = re;
                    bigy = set;
                    bigz = AIfi[re, set];
                }
        for (int enemy = 1; enemy <= 100; enemy++)//移動
        {
            if (allyserch(teki[enemy].kinx, teki[enemy].kiny, teki[enemy].large) == 0)
            {
                teki[enemy].speedy = bigy * 10 - teki[enemy].kiny;
                teki[enemy].speedx = bigx * 10 - teki[enemy].kinx;
            }
        }
    }
}
