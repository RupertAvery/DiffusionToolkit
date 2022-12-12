using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace Diffusion.Toolkit.Common;

/// <summary>
/// This gesture doesn't handle keys originating in a text control. This allows key bindings without modifier keys
/// that don't break normal typing. A standard KeyGesture doesn't have such logic; this allows the parent of a
/// text box to handle such bare keypresses before the textbox gets to see it as normal text input, thus breaking
/// normal typing.
/// </summary>
public class BareKeyGesture : InputGesture
{
    public Key Key { get; set; }

    public override bool Matches(object targetElement, InputEventArgs inputEventArgs)
    {
        var keyEventArgs = inputEventArgs as KeyEventArgs;
        if (keyEventArgs == null)
            return false;

        if (inputEventArgs.OriginalSource is TextBoxBase { IsReadOnly: false })
            return false;

        return (int)Key == (int)keyEventArgs.Key && Keyboard.Modifiers == ModifierKeys.None;
    }
}