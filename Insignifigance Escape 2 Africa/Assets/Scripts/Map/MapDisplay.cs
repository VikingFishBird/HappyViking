using UnityEngine;
using System.Collections;

public class MapDisplay : MonoBehaviour {

    public Renderer textureRender;
    public Material mapMat;
    public Texture2D barracks;
    public MeshFilter meshFilter;
    public MeshRenderer meshRenderer;
    public Shader shader;

    public void DrawTexture(Texture2D texture) {
        Material newMat = new Material(shader);
        newMat.mainTexture = barracks;
        textureRender.material = newMat;

        //textureRender.sharedMaterial.SetTexture("_BaseColorMap", barracks);
        //mapMat.SetTexture("_BaseColorMap", barracks);
        //textureRender.sharedMaterial.mainTexture = texture;
        //print(mapMat == textureRender.sharedMaterial);
        textureRender.transform.localScale = new Vector3(texture.width, texture.height, 1);
    }

    public void DrawMesh(Mesh mesh, Texture2D texture) {
        mesh.RecalculateNormals();
        meshFilter.sharedMesh.Clear();
        meshFilter.sharedMesh = mesh;
        //meshRenderer.sharedMaterial.SetTexture("_BaseColorMap", texture);
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}