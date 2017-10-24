using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.DomainClasses;

namespace Assets.Scripts.ActionResult
{
    /// <summary>
    /// Data structure that holds search results for a search for items within a scene.
    /// </summary>
    public struct FindSceneItemResult
    {
        public bool Found;
        public SceneItem SceneItem;
        public string Message;

        public FindSceneItemResult(bool Found, SceneItem sceneItem, string Message)
        {
            this.Found = Found;
            this.SceneItem = sceneItem;
            this.Message = Message;
        }
    }
}
