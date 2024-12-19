using UnityEngine;
using UnityEngine.Rendering;

namespace HighlightPlus {

    [DefaultExecutionOrder(100)]
    [ExecuteInEditMode]
    public class HighlightEffectBlocker : MonoBehaviour {

        Renderer thisRenderer;
        public bool blockOutlineAndGlow;
        public bool blockOverlay;
    
        void OnEnable () {
            thisRenderer = GetComponentInChildren<Renderer>();
            HighlightPlusRenderPassFeature.RegisterBlocker(this);
        }

        void OnDisable () {
            HighlightPlusRenderPassFeature.UnregisterBlocker(this);
        }

        public void BuildCommandBuffer(CommandBuffer cmd, Material mat) {
            if (thisRenderer == null) return;
            cmd.DrawRenderer(thisRenderer, mat);
        }

    }
}
