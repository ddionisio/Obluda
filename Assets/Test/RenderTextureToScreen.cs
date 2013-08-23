using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class RenderTextureToScreen : MonoBehaviour {
    public Texture texture;
    public Material material;

    public float gamma = 2.4f;
    public float shine = 0.05f;
    public float blend = 0.65f;
    public float resolution = 0.3f;

    void OnGUI() {
        if(texture) {
            if(Event.current.type == EventType.Repaint) {
                material.SetFloat("gamma", gamma);
                material.SetFloat("shine", shine);
                material.SetFloat("blend", blend);

                material.SetFloat("srcW", Screen.width / resolution);
                material.SetFloat("srcH", Screen.height / resolution);

                Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, material);
                Event.current.Use();
            }
        }
    }
}
