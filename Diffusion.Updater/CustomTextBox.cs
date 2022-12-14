using System.ComponentModel;

namespace Diffusion.Updater;

public class CustomTextBox : TextBox
{
    public CustomTextBox()
    {
        SelectionHighlightEnabled = false;
    }

    const int WM_SETFOCUS = 0x0007;
    const int WM_KILLFOCUS = 0x0008;
    
    [DefaultValue(true)]
    public bool SelectionHighlightEnabled { get; set; }
    
    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_SETFOCUS && !SelectionHighlightEnabled)
            m.Msg = WM_KILLFOCUS;

        base.WndProc(ref m);
    }
}