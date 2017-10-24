using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.ActionResult;
using SQLite4Unity3d;

namespace Assets.Scripts.DomainClasses {
    /// <summary>
    /// Represents a physical location in the game environment.
    /// </summary>
    [Serializable]
    public class Scene {
        public readonly string identifier;
        public readonly string description;
        public readonly string background;

        private readonly List<SceneComponent> components;

        private List<SceneExit> exits;
        private List<SceneItem> items;

        public string Description
        {
            get
            {
                string itemText = (getItems().Count > 0 ? string.Format("\nYou can see {0}", componentListAsString(items.Cast<SceneComponent>().ToList())) : "");
                string exitText = (getExits().Count > 0 ? string.Format("\nThe exits are {0}", componentListAsString(exits.Cast<SceneComponent>().ToList())) : "");

                return string.Format("{0}\n{1}\n{2}", description, itemText, exitText);
            }
        }


        public Scene(string identifier, string description, string background)
        {
            this.identifier = identifier;
            this.description = description;
            this.background = background;
            components = new List<SceneComponent>();
        }

        public Scene(DTO.Scene inputScene, SceneComponent[] inputComponents)
        {
            // Set information for this scene
            this.identifier = inputScene.identifier;
            this.description = inputScene.description;
            this.background = inputScene.background;

            // Create list for components
            this.components = inputComponents.ToList<SceneComponent>();
        }

        public bool addExit(string identifier, string fullName, string linkedSceneIdentifier)
        {
            bool o;
            o = (from component in components where component.identifier == identifier select component).ToList().Count < 1;
            if (o)
            {
                components.Add(new SceneExit(identifier, fullName, linkedSceneIdentifier));
                forceExitListRefresh();
            }
            return o;
        }

        public bool addExit(SceneExit exit)
        {
            bool o;
            o = (from component in components where component.identifier == identifier select component).ToList().Count < 1;
            if (o)
            {
                components.Add(exit);
                forceExitListRefresh();
            }
            return o;
        }

        public bool addKeyItem(string identifier, string fullName)
        {
            bool o;
            o = (from component in components where component.identifier == identifier select component).ToList().Count < 1;
            if (o)
            {
                components.Add(new SceneKey(identifier, fullName));
                forceItemListRefresh();
            }
            return o;
        }

        public bool addDoorItem(string identifier, string fullName, string requiredItemIdentifier, string successMessage, string failMessage, string linkedSceneIdentifier, string exitID, string exitName)
        {
            bool o;
            o = (from component in components where component.identifier == identifier select component).ToList().Count < 1;
            if (o)
            {
                components.Add(new SceneDoor(identifier, fullName, requiredItemIdentifier, successMessage, failMessage, linkedSceneIdentifier, exitID, exitName));
                forceItemListRefresh();
            }
            return o;
        }

        public FindSceneExitResult findExit(string identifier)
        {
            int index = 0;
            int matches = 0;

            bool returnFound;
            SceneExit returnReference;
            string returnMessage;

            identifier = identifier.ToLower();

            for (int i = 0; i < getExits().Count; i++)
            {
                if (getExits()[i].fullName.ToLower().Contains(identifier))
                {
                    index = i;
                    matches++;
                }
            }
            if (matches == 1) // Arguement matches exactly one exit
            {
                returnFound = true;
                returnReference = exits[index];
                returnMessage = "Exit Found";
            }
            else if (matches > 1) // Argument could reference more than one exit
            {
                returnFound = false;
                returnReference = null;
                returnMessage = "You'll need to be more specific.";
            }
            else // Argument matches no exits
            {
                returnFound = false;
                returnReference = null;
                returnMessage = string.Format("Could not find {0}.", identifier);
            }
            return new FindSceneExitResult(returnFound, returnReference, returnMessage);
        }

        public FindSceneItemResult findItem(string identifier)
        {
            int index = 0;
            int matches = 0;

            bool returnFound;
            SceneItem returnReference;
            string returnMessage;

            identifier = identifier.ToLower();

            for (int i = 0; i < getItems().Count; i++)
            {
                if (getItems()[i].fullName.ToLower().Contains(identifier))
                {
                    index = i;
                    matches++;
                }
            }
            if (matches == 1) // Arguement matches exactly one exit
            {
                returnFound = true;
                returnReference = items[index];
                returnMessage = "Item Found";
            }
            else if (matches > 1) // Argument could reference more than one exit
            {
                returnFound = false;
                returnReference = null;
                returnMessage = "You'll need to be more specific.";
            }
            else // Argument matches no exits
            {
                returnFound = false;
                returnReference = null;
                returnMessage = string.Format("Could not find {0}", identifier);
            }
            return new FindSceneItemResult(returnFound, returnReference, returnMessage);
        }

        public bool removeComponent(string identifier)
        {
            bool o;
            SceneComponent component = (from c in components where c.identifier == identifier select c).ToList().FirstOrDefault();
            o = component != null;
            if (o)
            {
                components.Remove(component);
                forceAllListRefresh();
            }
            return o;
        }

        private string componentListAsString(List<SceneComponent> input)
        {
            string o = "";
            for (int i = 0; i < input.Count; i++)
            {
                if (i == input.Count - 1 && input.Count > 1)
                    o = string.Format("{0} and ", o);
                else if (i != 0)
                    o = string.Format("{0}, ", o);
                o = string.Format("{0}{1}", o, input[i].fullName);
            }
            return string.Format("{0}.", o);
        }

        private List<SceneExit> getExits()
        {
            if (exits == null)
                exits = (from component in components where component.Type == SceneComponent.ComponentType.Exit select component).Cast<SceneExit>().ToList();
            return exits;
        }
        private void forceExitListRefresh()
        {
            exits = null;
        }
        private List<SceneItem> getItems()
        {
            SceneComponent.ComponentType[] itemTypes = { SceneComponent.ComponentType.Item, SceneComponent.ComponentType.Key, SceneComponent.ComponentType.Door };
            if(items == null)
                items = (from component in components where itemTypes.Contains(component.Type) select component).Cast<SceneItem>().ToList();
            return items;
        }
        private void forceItemListRefresh()
        {
            items = null;
        }
        private void forceAllListRefresh()
        {
            forceExitListRefresh();
            forceItemListRefresh();
        }

    }

}
