using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DomainClasses
{
    /// <summary>
    /// Abstract representation of an item in a scene.
    /// </summary>
    [Serializable]
    public abstract class SceneItem : SceneComponent
    {
        public SceneItem(string identifier, string fullName, ComponentType type = ComponentType.Item) : base(identifier, fullName, type)
        {

        }
    }
}
