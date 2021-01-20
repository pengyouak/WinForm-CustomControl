using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CustomControl.UI
{
    public class ButtonBase : Button
    {
        private bool _mouseDown;
        private bool _mouseEnter;

        private Color _disableBackground = Color.FromArgb(240, 240, 240);
        private Color _disableForeColor = Color.FromArgb(90, 90, 90);

        private int _radius = 0;
        private Color _fillColor = Color.FromArgb(33, 155, 238);
        private Color _textColor = Color.White;
        private StringAlignment _textHAlign = StringAlignment.Center;
        private StringAlignment _textVAlign = StringAlignment.Center;
        private int _borderWidth = 0;
        private Color _borderColor = Color.LightGray;
        private bool _fuzzyEnabled = false;
        private int _fuzzyMaxOpacity = 255;
        private int _fuzzyPenWidth = 1;

        [Category("ButtonBase")]
        [Description("圆角的弧度")]
        public int Radius
        {
            get => _radius;
            set
            {
                if (_radius < 0) return;
                _radius = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("按钮被禁用后的背景填充色")]
        public Color DisableBackground
        {
            get => _disableBackground;
            set
            {
                _disableBackground = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("按钮被禁用后的文字颜色")]
        public Color DisableForeColor
        {
            get => _disableForeColor;
            set
            {
                _disableForeColor = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("按钮的背景填充色")]
        public Color FillColor
        {
            get => _fillColor;
            set
            {
                _fillColor = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("按钮上的文字的颜色")]
        public Color TextColor
        {
            get => _textColor;
            set
            {
                _textColor = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("按钮上的文字水平对齐方式")]
        public StringAlignment TextHAlign
        {
            get => _textHAlign;
            set
            {
                _textHAlign = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("按钮上的文字垂直对齐方式")]
        public StringAlignment TextVAlign
        {
            get => _textVAlign;
            set
            {
                _textVAlign = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("边框宽度")]
        public int BorderWidth
        {
            get => _borderWidth;
            set
            {
                _borderWidth = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("边框颜色")]
        public Color BorderColor
        {
            get => _borderColor;
            set
            {
                _borderColor = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("是否启用模糊笔")]
        public bool FuzzyEnabled
        {
            get => _fuzzyEnabled;
            set
            {
                _fuzzyEnabled = value;
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("模糊笔的最大透明度")]
        public int FuzzyMaxOpacity
        {
            get => _fuzzyMaxOpacity;
            set
            {
                if (value >= 0 && value <= 255)
                    _fuzzyMaxOpacity = value;
                else throw new ArgumentOutOfRangeException("模糊比的透明度只能再0到255之间");
                Invalidate();
            }
        }

        [Category("ButtonBase")]
        [Description("模糊笔的宽度")]
        public int FuzzyPenWidth
        {
            get => _fuzzyPenWidth;
            set
            {
                if (value - _borderWidth > 1)
                    throw new ArgumentOutOfRangeException("模糊笔的大小不能超过边框的宽度");
                _fuzzyPenWidth = value;
                Invalidate();
            }
        }

        public ButtonBase()
        {
            InitializeComponent();
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        }

        public ButtonBase(IContainer container)
        {
            container.Add(this);
            InitializeComponent();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            _mouseDown = true;
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            _mouseDown = false;
            base.OnMouseUp(e);
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            _mouseEnter = true;
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            _mouseEnter = false;
            base.OnMouseLeave(e);
        }

        protected Color ChangeColor(Color color)
        {
            float red = (float)color.R * 0.73f;
            float green = (float)color.G * 0.7f;
            float blue = (float)color.B * 0.7f;

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);
            base.OnPaintBackground(pe);

            var fillColor = _fillColor;
            if (!Enabled)
                fillColor = _disableBackground;
            else if (_mouseDown)
                fillColor = ChangeColor(fillColor);
            else if (_mouseEnter)
                fillColor = ControlPaint.Light(fillColor);

            var g = pe.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.CompositingMode = System.Drawing.Drawing2D.CompositingMode.SourceOver;

            var fillRect = this.ClientRectangle;
            ShrinkShape(ref fillRect, _borderWidth * -1);

            // 填充背景
            var path = RoundRect(fillRect, _radius * 2);
            var brush = new SolidBrush(fillColor);
            g.FillPath(brush, path);

            // 画边框
            if (_borderWidth > 0)
            {
                path = RoundRect(fillRect, _radius * 2);
                if (!_fuzzyEnabled)
                    g.DrawPath(new Pen(_borderColor, _borderWidth), path);
                else
                    DrawPathWithFuzzyLine(path, g, _borderColor, _fuzzyMaxOpacity, _borderWidth, _fuzzyPenWidth);
                g.ResetTransform();
            }

            // 写文字
            var rec = fillRect;
            ShrinkShape(ref rec, -_radius / 2);

            WriteString(g, rec);
        }

        private void DrawPathWithFuzzyLine(GraphicsPath path,
            Graphics gr, Color base_color, int max_opacity,
            int width, int opaque_width)
        {
            int num_steps = width - opaque_width + 1;

            float delta = (float)max_opacity / num_steps / num_steps;

            float alpha = delta;

            for (int thickness = width; thickness >= opaque_width;
                thickness--)
            {
                Color color = Color.FromArgb(
                    (int)alpha,
                    base_color.R,
                    base_color.G,
                    base_color.B);
                using (Pen pen = new Pen(color, thickness))
                {
                    pen.EndCap = LineCap.Round;
                    pen.StartCap = LineCap.Round;
                    gr.DrawPath(pen, path);
                }
                alpha += delta;
            }
        }

        protected void ShrinkShape(ref Rectangle rect, int amount)
        {
            rect.Inflate(amount, amount);
        }

        protected GraphicsPath RoundRect(Rectangle Rect, float radius)
        {
            Rect.Width -= 1;
            GraphicsPath graphicsPath = new GraphicsPath();
            if (radius <= 0)
                graphicsPath.AddRectangle(Rect);
            else
            {
                graphicsPath.AddArc((float)Rect.X, (float)Rect.Y, radius, radius, 180f, 90f);
                graphicsPath.AddArc((float)checked(Rect.X + Rect.Width) - radius, (float)Rect.Y, radius, radius, 270f, 90f);
                graphicsPath.AddArc((float)checked(Rect.X + Rect.Width) - radius, (float)checked(Rect.Y + Rect.Height) - radius - 1, radius, radius, 0f, 90f);
                graphicsPath.AddArc((float)Rect.X, (float)checked(Rect.Y + Rect.Height) - radius - 1, radius, radius, 90f, 90f);
                graphicsPath.CloseAllFigures();
            }
            return graphicsPath;
        }

        protected void WriteString(Graphics g, Rectangle textRect)
        {
            var textColor = _textColor;
            if (!Enabled)
                textColor = _disableForeColor;

            g.DrawString(
                Text,
                Font,
                new SolidBrush(textColor),
                textRect,
                new StringFormat()
                {
                    Alignment = _textHAlign,
                    LineAlignment = _textVAlign
                }
            );
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            return;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // ButtonBase
            this.Font = new Font("微软雅黑", 16);
            this.ResumeLayout(false);
        }
    }
}
