using Managed.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Managed.UI
{
    public class Control
    {
        private Application _application;
        private ArrayList _controls = new ArrayList(); // generics seem a bit glitchy?
        private int _controlCount = 0;

        //public IEnumerable<Control> Controls { get { return _controls; } }
        public Control Parent { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Application Application { get { return _application; } }

        public int AbsoluteLeft
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.AbsoluteLeft + Left;
                }

                return Left;
            }           
        }

        public int AbsoluteTop
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.AbsoluteTop + Top;
                }

                return Top;
            }
        }

        public Control(Application application)
        {
            _application = application;
        }

        public void Remove(Control control)
        {
            control.Parent = null;
            _controls.Remove(control);
        }

        public void Add(Control control)
        {
            if (control.Parent != null)
            {
                control.Parent.Remove(control);
            }

            control.Parent = this;                        
            _controls.Add(control);
        }

        public bool WithinBounds(Point point)
        {
            return (point.X >= AbsoluteLeft && point.X < AbsoluteLeft + Width &&
                point.Y >= AbsoluteTop && point.Y < AbsoluteTop + Height);
        }

        public virtual bool SendMessage(UIMessage message, EventArgs e)
        {
            foreach (Control control in _controls)
            {
                if (control.SendMessage(message, e))
                {
                    return true;
                }
            }
            
            switch (message)
            {
                case UIMessage.TouchStart:
                    {
                        var te = e as TouchEventArgs;
                        if (WithinBounds(te.Position))
                        {
                            OnTouchStart(te);
                            return true;
                        }
                        break;
                    }

                case UIMessage.TouchMove:
                    {
                        var te = e as TouchEventArgs;
                        if (WithinBounds(te.Position))
                        {
                            OnTouchMove(te);
                            return true;
                        }
                        break;
                    }

                case UIMessage.TouchEnd:
                    {
                        var te = e as TouchEventArgs;
                        if (WithinBounds(te.Position))
                        {
                            OnTouchEnd(te);
                            return true;
                        }
                        break;
                    }
            }
            
            return false;
        }

        public virtual void Draw()
        {
            foreach (Control control in _controls)
            {
                control.Draw();
            }            
        }

        public virtual void Update(float delta)
        {
            foreach (Control control in _controls)
            {
                control.Update(delta);
            }
        }

        public event EventHandler<TouchEventArgs> TouchStart;
        public event EventHandler<TouchEventArgs> TouchMove;
        public event EventHandler<TouchEventArgs> TouchEnd;

        protected virtual void OnTouchStart(TouchEventArgs args)
        {
            if (TouchStart != null) TouchStart(this, args);
        }

        protected virtual void OnTouchMove(TouchEventArgs args)
        {
            if (TouchMove != null) TouchMove(this, args);
        }

        protected virtual void OnTouchEnd(TouchEventArgs args)
        {
            if (TouchEnd != null) TouchEnd(this, args);
        }
    }
}
