using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Represents part of a scene that can be used to move to another scene.
    /// </summary>
    public class SceneExit : SceneComponent
    {
        public readonly string linkedSceneIdentifier;

        public SceneExit(string identifier, string fullName, string linkedSceneIdentifier) : base(identifier, fullName, ComponentType.Exit)
        {
            this.linkedSceneIdentifier = linkedSceneIdentifier;
        }

        
    }
}
