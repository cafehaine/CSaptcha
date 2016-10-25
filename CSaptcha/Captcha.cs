using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace CSaptcha
{
    public class Captcha : object
    {
        string value;
        Random rnd;
        int length;
        string possibleChars;

        private void init()
        {
            char[] tempArray = new char[length];

            for (int i = 0; i < length; i++)
                tempArray[i] = possibleChars[rnd.Next(0, possibleChars.Length - 1)];

            value = new string(tempArray);
        }

        public void ReGenerate()
        {
            init();
        }

        public Captcha(int length = 10, string possibleChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890")
        {
            this.length = length;
            this.possibleChars = possibleChars;

            if (length < 5)
                throw new Exception("The length entered is too short or negative.");

            // welp, guess this seed is random enough for the kind of security we want.
            rnd = new Random(DateTime.Now.Millisecond + length - possibleChars.Length + 42);

            init();
        }

        public Bitmap GenerateBitmap(Color background, Color foreground, int captchaWidth = 200, int captchaHeight = 60, string fontName = "Times", int fontSize = 20)
        {
            if (captchaWidth < 100 || captchaHeight < 30)
                throw new Exception("The size entered is too small or negative.");

            Bitmap output = new Bitmap(captchaWidth, captchaHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            RectangleF rectf = new RectangleF(0, 0, captchaWidth, captchaHeight);

            Graphics g = Graphics.FromImage(output);
            g.Clear(background);
            Font font = new Font(fontName, fontSize);
            SizeF size = g.MeasureString(value, font);

            g.RotateTransform(rnd.Next(-5, 5));

            g.CompositingQuality = CompositingQuality.HighSpeed;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            g.DrawString(value, font, new SolidBrush(foreground), captchaWidth / 2 - size.Width / 2, captchaHeight / 2 - size.Height / 2);

            g.ResetTransform();

            g.Flush();

            applySineWave(ref output, captchaWidth,captchaHeight);

            g = Graphics.FromImage(output);

            Pen forePen = new Pen(foreground);

            // Random circles
            #region
            for (int i = 0; i < rnd.Next(1,3); i++)
            {
                int width = rnd.Next(20, captchaWidth - 1);
                int height = rnd.Next(6, captchaHeight - 1);
                int x = rnd.Next(0, captchaWidth - width);
                int y = rnd.Next(0, captchaHeight - height);

                Rectangle outer = new Rectangle(x, y, width, height);
                g.DrawEllipse(forePen, outer);
            }
            #endregion

            // Random lines
            #region
            for (int i = 0; i < rnd.Next(2, 5); i++)
            {
                int r = rnd.Next(0,captchaWidth - 1);
                int s = rnd.Next(0, captchaHeight - 1);
                int t = rnd.Next(0, captchaWidth - 1);
                int u = rnd.Next(0, captchaHeight - 1);

                g.DrawLine(forePen, r, s, t, u);
            }
            #endregion

            g.Flush();

            return output;
        }

        public string GenerateBase64Png(Color background, Color foreground, int captchaWidth = 200, int captchaHeight = 60, string fontName = "Times", int fontSize = 20)
        {
            Bitmap picture = GenerateBitmap(background, foreground, captchaWidth, captchaHeight, fontName, fontSize);

            MemoryStream pngPicture = new MemoryStream();

            picture.Save(pngPicture, ImageFormat.Png);
            byte[] data = pngPicture.ToArray();
            picture.Dispose();
            pngPicture.Dispose();

            return "data:image/png;base64," + Convert.ToBase64String(data);
        }

        private unsafe void applySineWave(ref Bitmap bitmap, int width, int height)
        {
            //! First, we fill an array with the vertical offset per column
            // about 2 * pi * 8
            int offset = rnd.Next(0, 50);
            int[] verticalOffsets = new int[width];
            for (int x = 0; x < width; x++)
            {
                verticalOffsets[x] = (int)(Math.Cos((x + offset) / 8) * 3);
            }

            Bitmap inputClone = new Bitmap(bitmap);
            BitmapData inputData = inputClone.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            BitmapData outputData = bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            for (int y = 0; y < outputData.Height; y++)

            {
                byte* row = (byte*)outputData.Scan0 + (y * outputData.Stride);

                for (int x = 0; x < outputData.Width; x++)
                {
                    if (verticalOffsets[x] != 0)
                    {
                        int rowToRead = y + verticalOffsets[x];
                        if (rowToRead >= height)
                            rowToRead = height - 1;
                        else if (rowToRead < 0)
                            rowToRead = 0;

                        IntPtr pointerToPixel = inputData.Scan0 + (rowToRead * inputData.Stride) + (x * 3);

                        row[x * 3] = *(byte*)(pointerToPixel);
                        row[x * 3 + 1] = *(byte*)(pointerToPixel + 1);
                        row[x * 3 + 2] = *(byte*)(pointerToPixel + 2);
                    }
                }

            }
            inputClone.UnlockBits(inputData);
            bitmap.UnlockBits(outputData);
        }

        public bool IsAnswer(string input)
        {
            return input == value;
        }

        public override string ToString()
        {
            return "Captcha";
        }
    }
}
