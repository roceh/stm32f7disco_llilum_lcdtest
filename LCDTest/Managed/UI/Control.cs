using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.UI
{
    public class Control
    {
        private Application _application;

        public List<Control> _controls = new List<Control>();

        public IEnumerable<Control> Controls { get { return _controls; } }
        public Control Parent { get; set; }
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Application Application { get; set; }

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
            foreach (var control in _controls)
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
                            OnTouch(te);
                            return true;
                        }
                        break;
                    }
            }
            
            return false;
        }

        public virtual void Draw()
        {
            foreach (var control in _controls)
            {
                control.Draw();
            }            
        }

        public virtual void Update(float delta)
        {
            foreach (var control in _controls)
            {
                control.Update(delta);
            }
        }

        public event EventHandler<TouchEventArgs> Touch;

        protected virtual void OnTouch(TouchEventArgs args)
        {
            if (Touch != null) Touch(this, args);
        }
    }
}
