using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Graphics;
using SFML.Window;

namespace SimpleLogicSimulator
{
    public class InPort
    {
        public Vector2f position;
        public OutPort connexion;
        public Gate parent;
        public static InPort selected;

        public bool GetState()
        {
            if (connexion == null)
                return false;
            return connexion.state;
        }

        public InPort(Vector2f pos, Gate par)
        {
            position = pos;
            connexion = null;
            parent = par;
            Program.window.MouseButtonPressed += Window_MouseButtonPressed;
        }

        private void Window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (Program.window.HasFocus() && !Program.placingMode && !parent.GetEditingMode() && e.Button == Mouse.Button.Right)
            {
                Vector2f pos = Program.window.MapPixelToCoords(Mouse.GetPosition(Program.window));
                float distance = (float)Math.Sqrt((pos.X - GetGlobalPos().X) * (pos.X - GetGlobalPos().X) + (pos.Y - GetGlobalPos().Y) * (pos.Y - GetGlobalPos().Y));
                if (distance <= 7)
                {
                    if (selected == this)
                    {
                        selected = null;
                    }
                    else
                    {
                        selected = this;
                        if (OutPort.selected != null)
                        {
                            if (selected.connexion == OutPort.selected)
                            {
                                selected.connexion = null;
                            }
                            else
                            {
                                connexion = OutPort.selected;                               
                            }
                            selected = null;
                            OutPort.selected = null;
                        }
                    }
                }
            }
        }

        public Vector2f GetGlobalPos()
        {
            return parent.Position + position;
        }
    }

    public class OutPort
    {
        public Vector2f position;
        public bool state = false;
        public Gate parent;
        public static OutPort selected;

        public OutPort(Vector2f pos, Gate par)
        {
            position = pos;
            parent = par;
            Program.window.MouseButtonPressed += Window_MouseButtonPressed;
        }

        private void Window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (Program.window.HasFocus() && !Program.placingMode && !parent.GetEditingMode() && e.Button == Mouse.Button.Right)                
            {
                Vector2f pos = Program.window.MapPixelToCoords(Mouse.GetPosition(Program.window));
                float distance = (float)Math.Sqrt((pos.X - GetGlobalPos().X) * (pos.X - GetGlobalPos().X) + (pos.Y - GetGlobalPos().Y) * (pos.Y - GetGlobalPos().Y));
                if (distance <= 7)
                {
                    if (selected == this)
                    {
                        selected = null;
                    }
                    else
                    {
                        selected = this;
                        if (InPort.selected != null)
                        {
                            if (InPort.selected.connexion == selected)
                            {
                                InPort.selected.connexion = null;
                            }
                            else
                            {
                                InPort.selected.connexion = selected;
                            }
                            selected = null;
                            InPort.selected = null;
                        }
                    }
                }
            }
        }

        public Vector2f GetGlobalPos()
        {
            return parent.Position + position;
        }
    }

    public abstract class Gate : RectangleShape
    {
        private static ConvexShape wireDrawer;
        private static CircleShape portDrawer;
        public InPort[] inPorts;
        public OutPort[] outPorts;

        public static Gate toRemove = null;

        protected bool editing = false;

        public static Type[] gateTypes { get; private set; }
        public static int selectedType = 0;

        public static void Setup()
        {
            gateTypes = new Type[7];
            gateTypes[1] = typeof(NotGate);
            gateTypes[2] = typeof(OrGate);
            gateTypes[3] = typeof(AndGate);
            gateTypes[4] = typeof(XorGate);
            gateTypes[6] = typeof(Lamp);
            gateTypes[0] = typeof(Input);
            gateTypes[5] = typeof(MInput);         

            wireDrawer = new ConvexShape();
            wireDrawer.SetPointCount(4);
            wireDrawer.OutlineThickness = 3;
            wireDrawer.OutlineColor = Color.Black;
            portDrawer = new CircleShape();
            portDrawer.FillColor = Color.Red;
            portDrawer.Origin = new Vector2f(5, 5);
            portDrawer.Radius = 5;
        }

        public Gate(uint inPortCount, uint outPortCount)
        {
            Size = new Vector2f(50, 50);
            Origin = new Vector2f(25, 25);
            inPorts = new InPort[inPortCount];
            outPorts = new OutPort[outPortCount];
            Program.window.MouseButtonPressed += ClickEvent;
        }

        public void SetEditingMode(bool edit)
        {
            editing = edit;
            if (edit)
            {
                FillColor = new Color(FillColor.R, FillColor.G, FillColor.B, 127);
            }
            else
            {
                FillColor = new Color(FillColor.R, FillColor.G, FillColor.B, 255);
            }
        }

        public bool GetEditingMode()
        {
            return editing;
        }

        public bool IsStarter()
        {
            return (inPorts.Count() == 0 && outPorts.Count() > 0);
        }

        private void ClickEvent(object sender, SFML.Window.MouseButtonEventArgs e)
        {
            if (Program.window.HasFocus() && !editing && !Program.placingMode && e.Button == Mouse.Button.Left)
            {
                Vector2i posG = Mouse.GetPosition(Program.window);
                Vector2f pos = Program.window.MapPixelToCoords(posG);
                if (pos.X >= Position.X - Origin.X && pos.Y >= Position.Y - Origin.Y && pos.X <= Position.X + Size.X - Origin.X && pos.Y <= Position.Y + Size.Y - Origin.Y)
                {
                    if (Keyboard.IsKeyPressed(Keyboard.Key.Subtract) || Keyboard.IsKeyPressed(Keyboard.Key.Delete) || Keyboard.IsKeyPressed(Keyboard.Key.D))
                    {
                        toRemove = this;
                    }
                    else
                    {
                        OnClick();
                    }
                }
            }
        }

        public abstract void OnClick();

        public virtual void DrawMore() { }

        public void DrawWires()
        {
            foreach (InPort port in inPorts)
            {
                if (port.connexion != null)
                {
                    wireDrawer.SetPoint(0, port.GetGlobalPos());
                    wireDrawer.SetPoint(1, port.GetGlobalPos());
                    wireDrawer.SetPoint(2, port.connexion.GetGlobalPos());
                    wireDrawer.SetPoint(3, port.connexion.GetGlobalPos());
                    Program.window.Draw(wireDrawer);
                }
            }               
        }

        public void DrawDots()
        {
            foreach (InPort port in inPorts)
            {
                portDrawer.Position = port.GetGlobalPos();
                if (port == InPort.selected)
                {
                    portDrawer.Radius = 8;
                    portDrawer.Origin = new Vector2f(8, 8);
                    portDrawer.FillColor = Color.Yellow;
                }
                else
                {
                    portDrawer.Radius = 5;
                    portDrawer.Origin = new Vector2f(5, 5);
                    if (port.GetState())
                    {
                        portDrawer.FillColor = Color.Green;
                    }
                    else
                    {
                        portDrawer.FillColor = Color.Red;
                    }
                }             
                Program.window.Draw(portDrawer);
            }
            foreach (OutPort port in outPorts)
            {
                portDrawer.Position = port.GetGlobalPos();
                if (port == OutPort.selected)
                {
                    portDrawer.Radius = 8;
                    portDrawer.Origin = new Vector2f(8, 8);
                    portDrawer.FillColor = new Color(255, 127, 0);
                }
                else
                {
                    portDrawer.Radius = 5;
                    portDrawer.Origin = new Vector2f(5, 5);
                    if (port.state)
                    {
                        portDrawer.FillColor = Color.Green;
                    }
                    else
                    {
                        portDrawer.FillColor = Color.Red;
                    }
                }             
                Program.window.Draw(portDrawer);
            }
        }

        public abstract void UpdateState();
    }

    public class NotGate : Gate
    {
        public NotGate() : base(1, 1)
        {
            inPorts[0] = new InPort(new Vector2f(-25, 0), this);
            outPorts[0] = new OutPort(new Vector2f(25, 0), this);
            Texture = Textures.gatesTextures[1];
            
        }

        public override void UpdateState()
        {
            outPorts[0].state = !inPorts[0].GetState();
        }

        public override void OnClick()
        { }
    }

    public class Input : Gate
    {
        public bool state = false;

        public Input() : base(0, 1)
        {
            outPorts[0] = new OutPort(new Vector2f(25, 0), this);
            Texture = Textures.gatesTextures[0];
            FillColor = new Color(127, 127, 127);
        }

        public override void UpdateState()
        {
            outPorts[0].state = state;
        }

        public override void OnClick()
        {
            state = !state;
            if (state)
            {
                FillColor = Color.Yellow;
            }
            else
            {
                FillColor = new Color(127, 127, 127);
            }
        }
    }

    public class MInput : Gate
    {
        public bool state = false;

        public MInput() : base(0, 1)
        {
            outPorts[0] = new OutPort(new Vector2f(25, 0), this);
            Texture = Textures.gatesTextures[0];
            FillColor = new Color(127, 100, 127);
            Texture = Textures.gatesTextures[5];
            Program.window.MouseButtonReleased += Window_MouseButtonReleased;
        }

        private void Window_MouseButtonReleased(object sender, MouseButtonEventArgs e)
        {
            if (!Program.placingMode && !editing)
            {
                state = false;
                FillColor = new Color(127, 100, 127);
            }
        }

        public override void UpdateState()
        {
            outPorts[0].state = state;
        }

        public override void OnClick()
        {
            state = true;
            FillColor = Color.Yellow;
        }
    }

    public class Lamp : Gate
    {
        public bool state = false;

        public Lamp() : base(1, 0)
        {
            inPorts[0] = new InPort(new Vector2f(-25, 0), this);
            FillColor = Color.Black;
            Texture = Textures.gatesTextures[6];
        }

        public override void UpdateState()
        {
            if (inPorts[0].GetState())
            {
                FillColor = Color.Yellow;
            }
            else
            {
                FillColor = Color.Black;
            }
        }

        public override void OnClick()
        { }
    }

    public class AndGate : Gate
    {
        public AndGate() : base(2, 1)
        {
            inPorts[0] = new InPort(new Vector2f(-25, -15), this);
            inPorts[1] = new InPort(new Vector2f(-25, 15), this);
            outPorts[0] = new OutPort(new Vector2f(25, 0), this);
            Texture = Textures.gatesTextures[3];
        }

        public override void UpdateState()
        {
            outPorts[0].state = (inPorts[0].GetState() && inPorts[1].GetState());
        }

        public override void OnClick()
        { }
    }

    public class OrGate : Gate
    {
        public OrGate() : base(2, 1)
        {
            inPorts[0] = new InPort(new Vector2f(-25, -15), this);
            inPorts[1] = new InPort(new Vector2f(-25, 15), this);
            outPorts[0] = new OutPort(new Vector2f(25, 0), this);
            Texture = Textures.gatesTextures[2];
        }

        public override void UpdateState()
        {
            outPorts[0].state = (inPorts[0].GetState() || inPorts[1].GetState());
        }

        public override void OnClick()
        { }
    }

    public class XorGate : Gate
    {
        public XorGate() : base(2, 1)
        {
            inPorts[0] = new InPort(new Vector2f(-25, -15), this);
            inPorts[1] = new InPort(new Vector2f(-25, 15), this);
            outPorts[0] = new OutPort(new Vector2f(25, 0), this);
            Texture = Textures.gatesTextures[4];
        }

        public override void UpdateState()
        {
            outPorts[0].state = (inPorts[0].GetState() ^ inPorts[1].GetState());
        }

        public override void OnClick()
        { }
    }
}
