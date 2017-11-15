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
                if (ViewManager.Instance.currentView == ViewManager.ViewType.Help)
                {
                    ViewManager.Instance.closeHelp("");
                    ViewManager.Instance.outputBoxText.text = "Closed help.";
                }
            }
        }
    }
}
