using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    /// <summary>
    /// Abstract representation of an object that makes up part of a scene.
    /// </summary>
    public abstract class SceneComponent
    {
        public readonly string identifier;
        public readonly string fullName;
        private readonly ComponentType type;

        public ComponentType Type
        {
            get { return type; }
        }

        public enum ComponentType
        {
            Exit,
            Item,
            Key,
            Door
        }

        public SceneComponent(string identifier, string fullName, ComponentType type)
        {
            this.identifier = identifier;
            this.fullName = fullName;
            this.type = type;
        }

        public override string ToString()
        {
            return identifier;
        }
    }
}
