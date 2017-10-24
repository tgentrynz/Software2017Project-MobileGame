using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DomainClasses
{
    /// <summary>
    /// Represents part of a scene that can be used to move to another scene.
    /// </summary>
    [Serializable]
    public class SceneExit : SceneComponent
    {
        public readonly string linkedSceneIdentifier;

        public SceneExit(string identifier, string fullName, string linkedSceneIdentifier) : base(identifier, fullName, ComponentType.Exit)
        {
            this.linkedSceneIdentifier = linkedSceneIdentifier;
        }

        public SceneExit(DTO.SceneComponent inputComponent, DTO.SceneExit inputExit) : base(inputComponent.identifier, inputComponent.fullName, ComponentType.Exit)
        {
            this.linkedSceneIdentifier = inputExit.linkedScene;
        }


    }
}
