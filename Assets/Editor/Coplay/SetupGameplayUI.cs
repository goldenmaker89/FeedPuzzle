using UnityEngine;
using Coplay.Controllers.Functions;

public class SetupGameplayUI
{
    public static string Execute()
    {
        // Create Canvas
        var canvas = CoplayTools.CreateGameObject("GameplayCanvas", "Canvas");
        CoplayTools.AddComponent(canvas, "CanvasScaler");
        CoplayTools.AddComponent(canvas, "GraphicRaycaster");
        
        // Create HUD Panel
        var hud = CoplayTools.CreateUIElement(ElementType.panel, "HUD", "GameplayCanvas");
        CoplayTools.SetRectTransform("GameplayCanvas/HUD", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        
        // Pause Button (Top Right)
        var pauseBtn = CoplayTools.CreateUIElement(ElementType.button, "PauseButton", "GameplayCanvas/HUD");
        CoplayTools.SetRectTransform("GameplayCanvas/HUD/PauseButton", "1,1", "1,1", "0.5,0.5", "100,100", "-100,-100");
        
        // Add text to button
        var pauseText = CoplayTools.CreateUIElement(ElementType.text, "Text", "GameplayCanvas/HUD/PauseButton");
        CoplayTools.SetRectTransform("GameplayCanvas/HUD/PauseButton/Text", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/HUD/PauseButton/Text", "PAUSE", 24, Color.black);

        // Queue Visualization Area (Bottom Left)
        var queueArea = CoplayTools.CreateUIElement(ElementType.panel, "QueueArea", "GameplayCanvas/HUD");
        CoplayTools.SetRectTransform("GameplayCanvas/HUD/QueueArea", "0,0", "0,0", "0.5,0.5", "300,100", "200,100");
        CoplayTools.AddComponent("GameplayCanvas/HUD/QueueArea", "HorizontalLayoutGroup");
        
        // Landing Strip Visualization Area (Bottom Right)
        var landingArea = CoplayTools.CreateUIElement(ElementType.panel, "LandingArea", "GameplayCanvas/HUD");
        CoplayTools.SetRectTransform("GameplayCanvas/HUD/LandingArea", "1,0", "1,0", "0.5,0.5", "300,100", "-200,100");
        CoplayTools.AddComponent("GameplayCanvas/HUD/LandingArea", "HorizontalLayoutGroup");

        // Pause Menu (Hidden by default)
        var pauseMenu = CoplayTools.CreateUIElement(ElementType.panel, "PauseMenu", "GameplayCanvas");
        CoplayTools.SetRectTransform("GameplayCanvas/PauseMenu", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        
        var resumeBtn = CoplayTools.CreateUIElement(ElementType.button, "ResumeButton", "GameplayCanvas/PauseMenu");
        CoplayTools.SetRectTransform("GameplayCanvas/PauseMenu/ResumeButton", "0.5,0.5", "0.5,0.5", "0.5,0.5", "200,60", "0,50");
        var resumeText = CoplayTools.CreateUIElement(ElementType.text, "Text", "GameplayCanvas/PauseMenu/ResumeButton");
        CoplayTools.SetRectTransform("GameplayCanvas/PauseMenu/ResumeButton/Text", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/PauseMenu/ResumeButton/Text", "RESUME", 24, Color.black);

        var restartBtn = CoplayTools.CreateUIElement(ElementType.button, "RestartButton", "GameplayCanvas/PauseMenu");
        CoplayTools.SetRectTransform("GameplayCanvas/PauseMenu/RestartButton", "0.5,0.5", "0.5,0.5", "0.5,0.5", "200,60", "0,-50");
        var restartText = CoplayTools.CreateUIElement(ElementType.text, "Text", "GameplayCanvas/PauseMenu/RestartButton");
        CoplayTools.SetRectTransform("GameplayCanvas/PauseMenu/RestartButton/Text", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/PauseMenu/RestartButton/Text", "RESTART", 24, Color.black);
        
        // Game Over Screen
        var gameOver = CoplayTools.CreateUIElement(ElementType.panel, "GameOverScreen", "GameplayCanvas");
        CoplayTools.SetRectTransform("GameplayCanvas/GameOverScreen", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        var goText = CoplayTools.CreateUIElement(ElementType.text, "Title", "GameplayCanvas/GameOverScreen");
        CoplayTools.SetRectTransform("GameplayCanvas/GameOverScreen/Title", "0.5,0.7", "0.5,0.7", "0.5,0.5", "400,100", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/GameOverScreen/Title", "GAME OVER", 48, Color.red);
        
        var goRestartBtn = CoplayTools.CreateUIElement(ElementType.button, "RestartButton", "GameplayCanvas/GameOverScreen");
        CoplayTools.SetRectTransform("GameplayCanvas/GameOverScreen/RestartButton", "0.5,0.4", "0.5,0.4", "0.5,0.5", "200,60", "0,0");
        var goRestartText = CoplayTools.CreateUIElement(ElementType.text, "Text", "GameplayCanvas/GameOverScreen/RestartButton");
        CoplayTools.SetRectTransform("GameplayCanvas/GameOverScreen/RestartButton/Text", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/GameOverScreen/RestartButton/Text", "TRY AGAIN", 24, Color.black);

        // Victory Screen
        var victory = CoplayTools.CreateUIElement(ElementType.panel, "VictoryScreen", "GameplayCanvas");
        CoplayTools.SetRectTransform("GameplayCanvas/VictoryScreen", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        var vicText = CoplayTools.CreateUIElement(ElementType.text, "Title", "GameplayCanvas/VictoryScreen");
        CoplayTools.SetRectTransform("GameplayCanvas/VictoryScreen/Title", "0.5,0.7", "0.5,0.7", "0.5,0.5", "400,100", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/VictoryScreen/Title", "VICTORY!", 48, Color.green);
        
        var vicRestartBtn = CoplayTools.CreateUIElement(ElementType.button, "RestartButton", "GameplayCanvas/VictoryScreen");
        CoplayTools.SetRectTransform("GameplayCanvas/VictoryScreen/RestartButton", "0.5,0.4", "0.5,0.4", "0.5,0.5", "200,60", "0,0");
        var vicRestartText = CoplayTools.CreateUIElement(ElementType.text, "Text", "GameplayCanvas/VictoryScreen/RestartButton");
        CoplayTools.SetRectTransform("GameplayCanvas/VictoryScreen/RestartButton/Text", "0,0", "1,1", "0.5,0.5", "0,0", "0,0");
        CoplayTools.SetUIText("GameplayCanvas/VictoryScreen/RestartButton/Text", "NEXT LEVEL", 24, Color.black);

        return "Created Gameplay UI structure";
    }
}
