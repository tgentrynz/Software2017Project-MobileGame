using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Scripts
{
    /// <summary>
    /// Representation of an item that can be picked up and used with a door item.
    /// </summary>
    class SceneKey : SceneItem
    {
        public SceneKey(string identifier, string fullName) : base(identifier, fullName, ComponentType.Key)
        {
        }
    }
}
