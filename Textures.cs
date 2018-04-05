using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFML.Graphics;

namespace SimpleLogicSimulator
{
    public static class Textures
    {
        public static Texture[] gatesTextures;
        public static void LoadTextures()
        {
            gatesTextures = new Texture[7];
            gatesTextures[0] = new Texture("textures\\input.png");
            gatesTextures[1] = new Texture("textures\\notgate.png");
            gatesTextures[2] = new Texture("textures\\orgate.png");
            gatesTextures[3] = new Texture("textures\\andgate.png");
            gatesTextures[4] = new Texture("textures\\xorgate.png");
            gatesTextures[5] = new Texture("textures\\minput.png");
            gatesTextures[6] = new Texture("textures\\lamp.png");
        }
    }


}
