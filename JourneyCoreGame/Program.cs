using JourneyCoreLib;
using JourneyCoreLib.Drawing;
using JourneyCoreLib.Rendering.Environment.Tiling;
using JourneyCoreLib.Graphics;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using JourneyCoreLib.Core.Context.Entities;
using JourneyCoreLib.Game.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using JourneyCoreLib.Game;

namespace JourneyCoreGame
{
    public class Program
    {

        private static ConsoleManager CManager { get; set; }
        private static GameLoop GLoop { get; set; }

        static void Main(string[] args)
        {
            CManager = new ConsoleManager();
            CManager.Hide(false);

            GLoop = new GameLoop();
            GLoop.Runtime();
        }
    }
}
