﻿using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;

namespace Coordinate_Systems.Game;

internal static class Program
{
    private const int FPS = 60;
    
    
    public static void Main(string[] args)
    {
        #region settings
        
        var gameSettings = GameWindowSettings.Default;
        gameSettings.RenderFrequency = FPS;
        gameSettings.UpdateFrequency = FPS;
            
        var uiSettings = NativeWindowSettings.Default;
        uiSettings.APIVersion = Version.Parse("4.1.0");
        uiSettings.Size = new Vector2i(800,600);
        uiSettings.Title = "LearnOpenGL";
        uiSettings.NumberOfSamples = 4;

        uiSettings.WindowState = WindowState.Normal;
        uiSettings.WindowBorder = WindowBorder.Resizable;
        uiSettings.IsEventDriven = false;
        uiSettings.StartFocused = true;
        
        #endregion
        
        
        using var game = new Game1();
        game.InitWindow(gameSettings,uiSettings);
        game.Run();
    }
}