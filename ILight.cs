using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
namespace GhostGame
{
    public interface ILight
    {
        Vector2 lightPosition { get; set; }
        float intensity { get; set; }
    }
}
