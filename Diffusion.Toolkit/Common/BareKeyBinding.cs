using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Diffusion.Toolkit.Common
{
    /// <summary>
    /// This only exists because the InputBinding constructor is protected, but since we have to have it anyway
    /// we also use this opportunity to simplify adding a BareKeyGesture to it.
    /// </summary>
    public class BareKeyBinding : InputBinding
    {
        private BareKeyGesture _gesture = new();

        public BareKeyBinding()
        {
            Gesture = _gesture;
        }

        public Key Key
        {
            get => _gesture.Key;
            set { _gesture.Key = value; }
        }
    }
}
