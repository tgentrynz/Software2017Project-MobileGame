using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts {
    public class AccelerometerManager : MonoBehaviour {
        private const float threshold = 2.0f;
        // Update is called once per frame
        void Update() {
            float i = Mathf.Abs(Input.acceleration.magnitude);
            Debug.Log(i);
            if (i > threshold)
            {
                switch (ViewManager.Instance.currentView) {
                    case ViewManager.ViewType.Help:
                        ViewManager.Instance.closeHelp("");
                        ViewManager.Instance.outputBoxText.text = "Closed help.";
                        break;
                    case ViewManager.ViewType.Menu:
                        ViewManager.Instance.exitGame("");
                        break;
                }
            }
        }
    }
}
