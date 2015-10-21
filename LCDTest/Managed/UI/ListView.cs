using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Managed.UI
{
    public class ListView : Control
    {
        private List<string> _items = new List<string>();
        private float _velocity = 0.0f;
        private float _touchOffset;
        private float _velocityTrackOffset;
        private float _velocityTrackTime;
        private float _flickTime;
        private float _amplitude;
        private float _target;
        private int _touchId = -1;
        private Point _touchStartPosition;

        public List<string> Items { get { return _items; } }
        public int RowHeight { get; set; }
        public float YOffset { get; set; }

        public ListView(Application application) : base(application)
        { }

        public override void Draw()
        {
            base.Draw();

            var left = AbsoluteLeft;
            var top = AbsoluteTop;

            for (int i = 0; i < _items.Count; i++)
            {
                if ((i * RowHeight) - (int) YOffset > 0 && (i * RowHeight) - (int) YOffset + RowHeight < Height)
                {
                    Application.Display.DrawRectangle(left, top + (i * RowHeight) - (int) YOffset, Width, RowHeight);
                    Application.Display.DrawCenteredString(_items[i], left, top + (i * RowHeight) - (int) YOffset, Width, RowHeight);
                }
            }
        }

        protected override void OnTouchStart(TouchEventArgs args)
        {
            _touchId = args.Id;
            _touchStartPosition = args.Position;
            _touchOffset = YOffset;
            _velocityTrackOffset = YOffset;
            _velocityTrackTime = Application.FrameTime;

            base.OnTouchStart(args);
        }

        protected override void OnTouchMove(TouchEventArgs args)
        {
            if (args.Id == _touchId)
            {
                YOffset = _touchOffset + (_touchStartPosition.Y - args.Position.Y);
            }

            base.OnTouchMove(args);
        }

        protected override void OnTouchEnd(TouchEventArgs args)
        {
            if (_touchId != -1)
            {
                if (_velocity > 10 || _velocity < -10)
                {
                    _amplitude = 0.8f * _velocity;
                    _target = YOffset + _amplitude;
                    _flickTime = Application.FrameTime;
                }

                _touchId = -1;
            }

            base.OnTouchEnd(args);
        }

        /// <summary>
        /// Math.Exp does not seem to compile currently?!?
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public float Exp(float x)
        {
            const float Exp = (float)Math.E;
            const float ExpHalfFactor = 1.648721f;
            const float ExpStep1 = 0.00138888f;
            const float ExpStep2 = 0.00833333f;
            const float ExpStep3 = 0.04166666f;
            const float ExpStep4 = 0.16666666f;
            const float ExpStep5 = 0.5f;
            const float ExpStep6 = 1.0f;

            int sign;

            // Reduce range to [0.0,1.0] 
            if (x < 0)
            {
                x = -x;
                sign = -1;
            }
            else
            {
                sign = 1;
            }

            float result = 1.0f;

            while (x > 1.0f)
            {
                x -= 1.0f;
                result *= Exp;
            }

            // Reduce range to [0.0,0.5] 
            if (x > 0.5f)
            {
                x -= 0.5f;
                result *= ExpHalfFactor;
            }

            float temp;

            temp = ExpStep1 * x;
            temp = (temp + ExpStep2) * x;
            temp = (temp + ExpStep3) * x;
            temp = (temp + ExpStep4) * x;
            temp = (temp + ExpStep5) * x;
            temp = (temp + ExpStep6) * x;

            result *= (temp + 1.0f);

            if (sign == -1)
            {
                result = 1.0f / result;
            }

            return result;
        }

        public override void Update(float delta)
        {
            base.Update(delta);

            // touch ended - so process velocity
            if (_touchId == -1 && _velocity != 0.0f)
            {
                // get point in exp curve for flick based on time since flick started
                float d = (-_amplitude * Exp(-(Application.FrameTime - _flickTime) / 0.325f));

                if (d > 0.1f || d < -0.1f)
                {
                    // still moving to target
                    YOffset = _target + d;
                }
                else
                {
                    // at target - set and stop
                    YOffset = _target;
                    _velocity = 0.0f;
                }
            }
            else if (_touchId != -1 && Application.FrameTime - _velocityTrackTime > 0.1)
            {
                // track velocity periodically (100ms)
                float v = (YOffset - _velocityTrackOffset) / (Application.FrameTime - _velocityTrackTime);

                // smooth out rapid velocity changes
                _velocity = 0.8f * v + 0.2f * _velocity;
                
                // record position for next velocity check
                _velocityTrackOffset = YOffset;
                _velocityTrackTime = Application.FrameTime;
            }            
        }
    }
}
