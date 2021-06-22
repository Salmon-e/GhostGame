using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace GhostGame
{
    public class PlayerController
    {
        public Dictionary<string, PlayerAction> actions;
        Player player;
        KeyboardState lastState = Keyboard.GetState();
        public bool focused;
        public PlayerController(Player player)
        {
            actions = new Dictionary<string, PlayerAction>();
            actions.Add(PlayerAction.Jump, new PlayerAction(Keys.Space, PressStyle.Press));
            actions.Add(PlayerAction.EndJump, new PlayerAction(Keys.Space, PressStyle.Release));
            actions.Add(PlayerAction.Left, new PlayerAction(Keys.A, PressStyle.During));
            actions.Add(PlayerAction.Right, new PlayerAction(Keys.D, PressStyle.During));
            actions.Add(PlayerAction.StartRoll, new PlayerAction(Keys.S, PressStyle.Press));
            actions.Add(PlayerAction.EndRoll, new PlayerAction(Keys.S, PressStyle.Release));
            this.player = player;
        }
        public void Update()
        {
            if (!focused) return;
            KeyboardState keyboardState = Keyboard.GetState();
            foreach(PlayerAction action in actions.Values)
            {
               
                if(action.style == PressStyle.During)
                {
                    if(keyboardState.IsKeyDown(action.key))
                        action.action();
                }
                else if(action.style == PressStyle.Press)
                {
                    if (keyboardState.IsKeyDown(action.key) && !lastState.IsKeyDown(action.key))
                        action.action();
                }
                else if(action.style == PressStyle.Release)
                {
                    if (!keyboardState.IsKeyDown(action.key) && lastState.IsKeyDown(action.key))
                        action.action();
                }
            }
            lastState = keyboardState;
        }
    }
    public enum PressStyle
    {
        Press, Release, During
    }
    public class PlayerAction
    {
        public Keys key;
        public PressStyle style;
        public Action action;       

        public static string Jump = "jump";
        public static string EndJump = "end jump";
        public static string Left = "left";
        public static string Right = "right";
        public static string StartRoll = "start roll";
        public static string EndRoll = "end roll";
        public PlayerAction(Keys key, PressStyle style)
        {
            this.key = key;
            this.style = style;
        }
             
    }
}
