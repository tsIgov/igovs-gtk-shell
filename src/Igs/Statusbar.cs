using Gtk;

public class Statusbar : Window
{
    public Statusbar(Gdk.Monitor monitor) : base("statusbar")
    {



        LayerShell.InitWindow(this);
        //LayerShell.SetNamespace(this, Name);
        LayerShell.SetLayer(this, LayerShell.Layer.Top);
        LayerShell.SetAnchor(this, LayerShell.Edge.Top, true);
        LayerShell.SetAnchor(this, LayerShell.Edge.Left, true);
        LayerShell.SetAnchor(this, LayerShell.Edge.Right, true);
        LayerShell.AutoExclusiveZoneEnable(this);
        LayerShell.SetMonitor(this, monitor);
        //LayerShell.SetKeyboardMode(this, LayerShell.KeyboardMode.Exclusive);

        Box box = new(Orientation.Horizontal, 10);
        Add(box);

        Button button = new Button ("Hit me left");
        button.Clicked += (_,_) => { Console.WriteLine("Button pressed"); };

        Button button2 = new Button ("Hit me right");
        button2.Clicked += (_,_) => { Console.WriteLine("Button pressed"); };

        Button button3 = new Button ("Hit me center");
        button2.Clicked += (_,_) => { Console.WriteLine("Button pressed"); };

        box.PackStart(button, true, true, 0);
        box.PackEnd(button2, false, false, 20);
        box.CenterWidget = button3;


    }

    protected override void OnShown()
    {
        base.OnShown();
        //Window.TypeHint = Gdk.WindowTypeHint.Dock;
        //Window.AcceptFocus = false;
        //Window.KeepBelow = true;
    }

    protected override void OnSizeAllocated(Gdk.Rectangle allocation)
    {
        base.OnSizeAllocated(allocation);
        //Move(1200, 300);

    }
}