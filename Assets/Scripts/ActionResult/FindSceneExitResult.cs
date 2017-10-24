using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.DomainClasses;

namespace Assets.Scripts.ActionResult
{
    /// <summary>
    /// Data structure that holds search results for a search for exits within a scene.
    /// </summary>
    public struct FindSceneExitResult
    {
        public bool Found;
        public SceneExit SceneExit;
        public string Message;

        public FindSceneExitResult(bool Found, SceneExit SceneExit, string Message)
        {
            this.Found = Found;
            this.SceneExit = SceneExit;
            this.Message = Message;
        }
    }
}
