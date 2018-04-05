using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.System;
using SFML.Window;
using SFML.Graphics;

namespace SimpleLogicSimulator
{
    public static class Program
    {
        public static RenderWindow window;
        public static View view;
        public static View gui;

        static List<Gate> gates = new List<Gate>();

        static Vector2i lastMouse = new Vector2i();
        static bool dragging = false;

        static Gate placing;
        public static bool placingMode = false;
        public static bool gridLock = false;

        static void Main(string[] args)
        {
            Gate.Setup();
            Textures.LoadTextures();

            Color clearColor = new Color(230, 230, 150);

            window = new RenderWindow(new VideoMode(800, 600), "Simple Logic Simulator");
            window.SetActive(false);
            window.SetFramerateLimit(60);
            window.Resized += Window_Resized;
            window.MouseButtonPressed += Window_MouseButtonPressed;
            window.MouseWheelMoved += Window_MouseWheelMoved;
            window.Closed += Window_Closed;
            window.KeyPressed += Window_KeyPressed;
            view = window.GetView();
            gui = window.GetView();
            Console.Title = "Simple Logic Simulator Console";

            placing = new NotGate();
            placing.SetEditingMode(true);

            AddGates();

            while (window.IsOpen)
            {
                window.DispatchEvents();
                CheckControls();
                Update();
                window.Clear(clearColor);
                Draw();
                window.Display();
            }
        }

        private static void CheckControls()
        {
            if (window.HasFocus())
            {
                if (Mouse.IsButtonPressed(Mouse.Button.Middle))
                {
                    if (dragging == true)
                    {
                        Vector2i delta = new Vector2i((int)window.Size.X, (int)window.Size.Y) / 2 - Mouse.GetPosition(window);
                        view.Center += new Vector2f(delta.X / -10, delta.Y / -10);
                        window.SetView(view);
                    }
                    dragging = true;
                    lastMouse = Mouse.GetPosition(window);
                }
                else
                {
                    dragging = false;
                    lastMouse = new Vector2i();
                }
                if (placingMode)
                {
                    Vector2f pos = window.MapPixelToCoords(Mouse.GetPosition(window));
                    if (gridLock)
                    {
                        placing.Position = new Vector2f((float)Math.Round(pos.X / 25, 0) * 25f, (float)Math.Round(pos.Y / 25, 0) * 25f);
                    }
                    else
                    {
                        placing.Position = pos;
                    }                   
                }
            }
        }

        private static void Update()
        {
            if (Gate.toRemove != null)
            {
                gates.Remove(Gate.toRemove);
                Gate.toRemove = null;
            }
            foreach (Gate g in gates)
            {
                g.UpdateState();
            }
        }

        private static void Draw()
        {
            foreach (Gate g in gates)
            {
                window.Draw(g);
            }
            foreach (Gate g in gates)
            {
                g.DrawWires();
            }
            foreach (Gate g in gates)
            {
                g.DrawDots();
            }
            foreach (Gate g in gates)
            {
                g.DrawMore();
            }
            if (placingMode)
            {
                window.Draw(placing);
            }
        }

        private static void AddGates()
        {
            Input inp1 = new Input();
            inp1.Position = new Vector2f(100, 100);

            MInput inp2 = new MInput();
            inp2.Position = new Vector2f(100, 200);

            Input inp3 = new Input();
            inp3.Position = new Vector2f(100, 300);

            MInput inp4 = new MInput();
            inp4.Position = new Vector2f(100, 400);

            XorGate xor1 = new XorGate();
            xor1.Position = new Vector2f(300, 100);

            AndGate and1 = new AndGate();
            and1.Position = new Vector2f(300, 200);

            OrGate or1 = new OrGate();
            or1.Position = new Vector2f(300, 300);

            NotGate not1 = new NotGate();
            not1.Position = new Vector2f(300, 400);

            Lamp lamp1 = new Lamp();
            lamp1.Position = new Vector2f(700, 100);

            Lamp lamp2 = new Lamp();
            lamp2.Position = new Vector2f(700, 200);

            Lamp lamp3 = new Lamp();
            lamp3.Position = new Vector2f(700, 300);

            Lamp lamp4 = new Lamp();
            lamp4.Position = new Vector2f(700, 400);

            gates.Add(inp1);
            gates.Add(inp2);
            gates.Add(inp3);
            gates.Add(inp4);
            gates.Add(xor1);
            gates.Add(or1);
            gates.Add(not1);
            gates.Add(and1);
            gates.Add(lamp1);
            gates.Add(lamp2);
            gates.Add(lamp3);
            gates.Add(lamp4);
        }

        private static void Window_KeyPressed(object sender, KeyEventArgs e)
        {
            if (window.HasFocus())
            { 
                switch (e.Code)
                {
                    case (Keyboard.Key.A):
                        placingMode = !placingMode;
                        break;
                    case (Keyboard.Key.G):
                        gridLock = !gridLock;
                        break;
                    case (Keyboard.Key.X):
                        OutPort.selected = null;
                        InPort.selected = null;
                        break;
                }
            }
        }
        private static void Window_Closed(object sender, EventArgs e)
        {
            window.Close();
        }
        private static void Window_MouseWheelMoved(object sender, MouseWheelEventArgs e)
        {
            view.Size *= (((float)e.Delta / -10.0f + 1.0f));
            window.SetView(view);
        }
        private static void Window_MouseButtonPressed(object sender, MouseButtonEventArgs e)
        {
            if (window.HasFocus())
            {
                switch (e.Button)
                {
                    case (Mouse.Button.Right):
                        if (placingMode)
                        {
                            Type T = placing.GetType();
                            Gate g = (Gate)Activator.CreateInstance(T);
                            g.Position = placing.Position;
                            gates.Add(g);
                        }
                        break;
                    case (Mouse.Button.Left):
                        if (placingMode)
                        {
                            Gate.selectedType += 1;
                            Gate.selectedType %= Gate.gateTypes.Length;
                            placing = (Gate)Activator.CreateInstance(Gate.gateTypes[Gate.selectedType]);
                            placing.SetEditingMode(true);
                        }
                        break;
                }
            }
        }
        private static void Window_Resized(object sender, SizeEventArgs e)
        {

        }
    }
}
