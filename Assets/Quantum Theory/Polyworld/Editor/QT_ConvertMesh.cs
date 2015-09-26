using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
public class QT_ConvertMesh : EditorWindow
{
    [MenuItem("Window/Quantum Theory/PolyWorld Mesh Converter")]

    static void Init()
    {
        QT_ConvertMesh window = (QT_ConvertMesh)EditorWindow.GetWindow(typeof(QT_ConvertMesh));
        window.title = "PolyWorld Mesher";
        window.maxSize = new Vector2(300, 285);
        window.minSize = window.maxSize;
        window.Show();
    }


    private GameObject[] SourceGOs;
    private Color brightenColor = Color.black;
    private string HelpMessage;
    //private string extension = "-Faceted";
    Texture WorldIcon = (Texture)AssetDatabase.LoadAssetAtPath("Assets/Quantum Theory/Polyworld/Editor/QT_PolyWorld-icon.png", typeof(Texture));
    private bool filterBilinear = true;
    enum BlurAmount { None, Some, Full }
    BlurAmount blurAmount = BlurAmount.None;
    int mipLevel = 0;
  //  bool combineMats = true;
    //progress bar stuff


    void UpdateProgress(int totalCount,int currentGOIndex,string currentGO)
    {
        EditorUtility.DisplayProgressBar("Processing...", "Converting "+currentGO, Mathf.InverseLerp(0, totalCount, currentGOIndex));
    }


    public void OnGUI()
    {
        EditorGUI.DrawPreviewTexture(new Rect(10, 10, 280, 60), WorldIcon);
        GUILayout.BeginArea(new Rect(10, 70, 280, 120));
        if (Selection.gameObjects.Length > 0)
        {
            SourceGOs = Selection.gameObjects;

            if (isRootSelected()==false)            
                HelpMessage = "Mesh conversion will only work properly if the root of all selected prefabs are selected. Do not select children.";                      
            else
            {
                HelpMessage = "Brighten by Color adds the color to the final vertex colors of all selected GameObjects. Use the Color swatch in the material change the color as well. Combine Materials will apply one new material to the each new prefab. Alternate Coloring applies Bilinear Filtering. Blur Amount selects a mipmap within the diffuse texture chain.";
               
                if (SourceGOs.Length == 1)
                    GUILayout.Label("GameObject Selected: " + SourceGOs[0].name);
                else
                    GUILayout.Label("GameObject Selected: Multiple");
                brightenColor = EditorGUILayout.ColorField("Brighten by Color: ", brightenColor, null);
                //combineMats = EditorGUILayout.Toggle("Combine Materials", combineMats);
                filterBilinear = EditorGUILayout.Toggle("Alternate Coloring", filterBilinear);
                blurAmount = (BlurAmount)EditorGUILayout.EnumPopup("Blur Amount: ", blurAmount);
                
                if (GUILayout.Button("Convert to PolyWorld Mesh"))
                {

                    List<GameObject> badGOs = new List<GameObject>(); //stores a list of selected gameobjects that have no mesh. Just for error checking.
                    List<string> badMeshNames = CheckTriLimit(SourceGOs);
                    List<string> badMats = CheckMaterials(SourceGOs);
                    List<string> badTerrains = CheckforTerrain(SourceGOs);
                    List<string> notPrefab = CheckforPrefab(SourceGOs);

                    if (badMats.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Material Warning", "Some shaders assigned to the selected meshes don't use the _MainTex and/or _Color variable name.To fix, switch these materials to use the standard Diffuse shader.\n\nCheck the Console for the material names.", "OK");
                        foreach (string s in badMats)
                            Debug.LogError("The shader assigned to the material named " + s + " does not use the _MainTex and/or _Color variable. To fix, switch it to the standard Diffuse shader.");
                    }
                    else if (badTerrains.Count > 0)
                        EditorUtility.DisplayDialog("Terrains in Selection", "There is a terrain in the selection. This script will not convert terrains. Use the PolyWorld Terrain script located in Window->Quantum Theory->PolyWorld Terrain.", "OK");
                    else if (notPrefab.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Not a Prefab", "Some GameObjects in the selection are not a prefab. Create a prefab first, then run the conversion script.\n\nCheck the Console for the Gameobjects that are not prefabs.", "OK");
                        foreach (string s in notPrefab)
                            Debug.LogError("GameObject named " + s + " is not a prefab.");
                    }
                    else
                    {
                        if (badMeshNames.Count == 0)
                        {

                            foreach (GameObject g in SourceGOs) //for every GO you have selected
                            {
                                GameObject WorkingGO = (GameObject)(PrefabUtility.InstantiatePrefab((GameObject)PrefabUtility.GetPrefabParent(g)));//instantiate it and work on it.                                
                                WorkingGO.name = WorkingGO.name.Replace("(Clone)", "");
                                MeshRenderer[] MRs = WorkingGO.GetComponentsInChildren<MeshRenderer>(true); //get all the MR components in the children.
                                MeshFilter[] MFs = WorkingGO.GetComponentsInChildren<MeshFilter>(true);
                                SkinnedMeshRenderer[] SMRs = WorkingGO.GetComponentsInChildren<SkinnedMeshRenderer>(true);//get all the SMR components in the children.
                                //now get all the gameobjects with meshes in it                          
                                GameObject[] MeshGOs = GetMeshGOs(MRs, SMRs);
                                if (MeshGOs.Length > 0)
                                    ConvertMesh(WorkingGO, MeshGOs, MFs, MRs, SMRs);
                                else
                                    badGOs.Add(WorkingGO);
                                GameObject.DestroyImmediate(WorkingGO);
                            }
                            EditorUtility.ClearProgressBar();
                            EditorUtility.DisplayDialog("Conversion Complete", "Success! All new content is located in:\n\nAssets/-Faceted Meshes", "OK");

                        }
                        else
                        {
                            EditorUtility.DisplayDialog("Triangle Count Exceeded", "Meshes that exceed 21845 triangles have been found in this selection. Please either reduce the triangle count on the model, or break it apart into seperate meshes.\n\nThe debug console will now output which meshes are too high poly.\n\nConversion aborted.", "OK");
                            foreach (string s in badMeshNames)
                                Debug.LogWarning(s + " has too many triangles. Please reduce the triangle count or break it apart into seperate meshes.");

                        }
                    }
                    if (badGOs.Count > 0)
                    {
                        EditorUtility.DisplayDialog("Meshes Not Found", "Some GameObjects in the selection had no meshes. Check the debug log. ", "OK");
                        foreach (GameObject g in badGOs)
                            Debug.LogWarning(g.name + " does not have a Mesh Filter or Skinned Mesh Filter, nor does it have children with these components, and did not get converted.");
                    }
                }
            }
        }
        else               
           HelpMessage="In the Scene View, select the character or prop prefab that has a diffuse texture assigned to it. This script converts the prefab parent of the selected objects. Multiple objects are supported.";

        GUILayout.EndArea();        
        EditorGUI.HelpBox(new Rect(5, 185, 290,95), HelpMessage,MessageType.Info);
       
    }
    private List<string> CheckforPrefab(GameObject[] SourceGOs)
    {
        List<string> badGOs = new List<string>();

        foreach (GameObject g in SourceGOs)
        {
            GameObject p = (GameObject)PrefabUtility.GetPrefabParent(g);
            if (p == null)
                badGOs.Add(g.name);
        }
        return badGOs;
    }
    //checks to see if any of the materials are using _Maintex and _Color.
    private List<string> CheckMaterials(GameObject[] SourceGOs)
    {
        List<string> badMats = new List<string>();
        //these properties exist in the material behind the scenes. Maybe check the shader if it's using them.

        foreach (GameObject g in SourceGOs) //for every GO you have selected
        {
            MeshRenderer[] mrs = g.GetComponentsInChildren<MeshRenderer>(true);
            foreach (MeshRenderer mr in mrs)
            {               
                    Material[] m = mr.sharedMaterials;
                    foreach (Material mat in m)
                    {
                        if(!mat.HasProperty("_Color") || !mat.HasProperty("_MainTex"))
                        badMats.Add(mat.name);
                    }
                
            }

            SkinnedMeshRenderer[] smrs = g.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            foreach (SkinnedMeshRenderer smr in smrs)
            {
                Material[] m = smr.sharedMaterials;
                foreach (Material mat in m)
                {
                    if(!mat.HasProperty("_Color") || !mat.HasProperty("_MainTex"))
                        badMats.Add(mat.name);
                }

            }
        }
        return badMats;
    }

    //checks to see if you actually selected the root of all prefabs
    private bool isRootSelected()
    {
        bool isRoot = true;
        foreach (GameObject g in SourceGOs)
        {           
            if (g.transform.parent != null)            
                isRoot = false;            
        }
        return isRoot;
    }
    //gets all the gameobjects with meshes in it
    private GameObject[] GetMeshGOs(MeshRenderer[] MRs, SkinnedMeshRenderer[] SMRs)
    {
        List<GameObject> GOs = new List<GameObject>();
        foreach (MeshRenderer m in MRs)
            GOs.Add(m.gameObject);
        foreach (SkinnedMeshRenderer s in SMRs)
            GOs.Add(s.gameObject);
        return GOs.ToArray();
    }

    //checks all the meshes to make sure the result of conversion will be within the vertex limit in unity, currently 65536 verts.
    private List<string> CheckTriLimit(GameObject[] SourceGOs)
    {
        List<string> meshNames = new List<string>();

        foreach (GameObject g in SourceGOs)
        {
            MeshFilter[] m = g.GetComponentsInChildren<MeshFilter>();
            if (m.Length > 0)
            {
                foreach (MeshFilter mf in m)
                {
                    if (mf.sharedMesh.triangles.Length > 21845)
                    {
                        meshNames.Add(mf.sharedMesh.name);
                        break;
                    }
                }
            }
            SkinnedMeshRenderer[] s = g.GetComponentsInChildren<SkinnedMeshRenderer>();
            if (s.Length > 0)
            {
                foreach (SkinnedMeshRenderer smr in s)
                {
                    if (smr.sharedMesh.triangles.Length > 21845)
                    {
                        meshNames.Add(smr.sharedMesh.name);
                        break;
                    }
                }
            }            
        }
        return meshNames;
    }

    private List<string> CheckforTerrain(GameObject[] SourceGOs)
    {
        List<string> GONames = new List<string>();
        foreach (GameObject g in SourceGOs)
        {
            Terrain[] terrains = g.GetComponentsInChildren<Terrain>(true);
            if (terrains.Length > 0)
                GONames.Add(g.name);
        }
        return GONames;
    }

    //global is just easier.
    private List<Vector3> FinalVerts = new List<Vector3>();
    private List<Vector2> FinalUVS = new List<Vector2>();
    private List<Color32> FinalVCs = new List<Color32>();
    private List<int> FinalTris = new List<int>();
    //the next two are for skinned meshes. Bone weights and bind poses.
    private List<BoneWeight> FinalBWs = new List<BoneWeight>();
    private List<Matrix4x4> FinalBPs = new List<Matrix4x4>();
     
    private Mesh newVCMesh;

    //here is where the work is done.    
    private void ConvertMesh(GameObject WorkingGO, GameObject[] MeshGOs, MeshFilter[] MFs, MeshRenderer[] MRs, SkinnedMeshRenderer[] SMRs)
    {
       
        //just a set of flags for which go's are skinnedmeshes. it may be possible to have a mix i think
        bool[] isSkinnedMesh = new bool[MeshGOs.Length];

        string folderPath = GetFolderPath();
        Material finalMat = CreateMasterMaterial(folderPath, WorkingGO.name + "_Mat"); //create one material for the entire GO. Really, anything vertex colored can use one material..
        
        int totalCount = MeshGOs.Length;

        int SMRIndex = 0; //stores the current index to the SMR GOs
        //for every gameobject child with a mesh in it
        for (int x = 0; x < MeshGOs.Length; x++)       
        {
            AssetDatabase.Refresh();
            FinalVerts.Clear();
            FinalUVS.Clear();
            FinalVCs.Clear();
            FinalTris.Clear();
            FinalBWs.Clear();
            FinalBPs.Clear();

            isSkinnedMesh[x] = isGOSkinnedMesh(MeshGOs[x]);

            newVCMesh = new Mesh();

            if (isSkinnedMesh[x])
            {
                RenderSkinnedMesh(SMRs[SMRIndex], MeshGOs[x]);
                SMRIndex++;
            }
            else
                RenderSimpleProp(MFs[x], MRs[x], MeshGOs[x]);

            newVCMesh.vertices = FinalVerts.ToArray();
            newVCMesh.uv = FinalUVS.ToArray();
            newVCMesh.colors32 = FinalVCs.ToArray();
            newVCMesh.triangles = FinalTris.ToArray();
            if (isSkinnedMesh[x])
            {
                newVCMesh.boneWeights = FinalBWs.ToArray();
                newVCMesh.bindposes = FinalBPs.ToArray();
            }
            newVCMesh.Optimize();
            newVCMesh.RecalculateNormals(); //recalc normals breaks normals along uv seams.. 
            newVCMesh.name = MeshGOs[x].name + "-Faceted";
            newVCMesh.RecalculateBounds();
            //write meshes to disk.
            CreateMesh(newVCMesh, folderPath); 
            //plug in the new mesh to the current GO in the prefab hierarchy.
            MeshGOs[x] = UpdateMeshGO(MeshGOs[x], newVCMesh, finalMat, isSkinnedMesh[x]);
            UpdateProgress(totalCount,x,MeshGOs[x].name);
        }
        //All done making meshes. Update the prefab or create a new one.
        UpdatePrefab(WorkingGO, MeshGOs, isSkinnedMesh, folderPath);
    }

    private GameObject UpdateMeshGO(GameObject MeshGO, Mesh newVCMesh, Material finalMat, bool isSkinnedMesh)
    {
        SkinnedMeshRenderer SMR;
        MeshRenderer MR;
        MeshFilter MF;

        Material[] fMat = new Material[1];
        fMat[0] = finalMat;

        if (isSkinnedMesh)
        {
            SMR = MeshGO.GetComponent<SkinnedMeshRenderer>();
            SMR.sharedMesh = newVCMesh;
            SMR.sharedMaterials = fMat;
        }
        else
        {
            MR = MeshGO.GetComponent<MeshRenderer>();
            MF = MeshGO.GetComponent<MeshFilter>();
            MR.sharedMaterials = fMat;
            MF.sharedMesh = newVCMesh;
        }
        return MeshGO;

    }
    //checks to see if the GO is a skinned mesh.
    private bool isGOSkinnedMesh(GameObject MeshGO)
    {
        bool isSkinnedMesh = false;

        SkinnedMeshRenderer SMR = MeshGO.GetComponent<SkinnedMeshRenderer>();
        if (SMR != null)
            isSkinnedMesh = true;
        return isSkinnedMesh;
    }

    private void OnInspectorUpdate()
    {
        Repaint();
    }
    private string GetFolderPath()
    {
        string guid, newFolderPath;
        // List<GameObject> newGOs = new List<GameObject>(); //holds refs to the new gameobjects we're making

        //setup the folder. check if it exists. if it doesn't, make it.
        string windowsAssetsPath = Application.dataPath;
        string windowsFacetedPath = windowsAssetsPath + "/Faceted Meshes";

        if (!Directory.Exists(windowsFacetedPath))
        {
            guid = AssetDatabase.CreateFolder("Assets", "Faceted Meshes");
            newFolderPath = AssetDatabase.GUIDToAssetPath(guid);
        }
        else
            newFolderPath = "Assets/Faceted Meshes";

        return newFolderPath;
    }
    private void RenderSimpleProp(MeshFilter MF, MeshRenderer MR, GameObject currentGO)
    {
        //make a copy of the source mesh
        Mesh sourceMesh = MF.sharedMesh;
        
        //for every submesh, do all the work.
        for (int x = 0; x < sourceMesh.subMeshCount; x++)
        {

            List<Vector3> SMVerts = new List<Vector3>();
            List<Vector2> SMUVs = new List<Vector2>();
            List<Color32> SMVCs = new List<Color32>();
          
            //triangle arrays point to the array index of the vertices in the vertex array
            int[] triList = sourceMesh.GetTriangles(x); //submesh's index number corresponds to the material index of the meshrendere component
            //get the diffuse texture. Hopefully the shader obeys the standard naming convention..

            //doesn't work with substances or probably square textures..
            string path = AssetDatabase.GetAssetPath(MR.sharedMaterials[x].GetTexture("_MainTex"));
            TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(path);
            A.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Texture2D tex = (Texture2D)MR.sharedMaterials[x].GetTexture("_MainTex");
            Color matColor = MR.sharedMaterials[x].GetColor("_Color");

            //Coloring will not work with nonsquare textures.

            //we only want to go as far as 32x32
            int mipAmount = tex.mipmapCount - 4;//(int)Mathf.Log((int)tex.width)+1;            
            mipLevel = PickMipLevel(tex, mipAmount);


            if (mipLevel > 0) //if we chose to do blur amount..
            {
                int mipSize = (int)Mathf.Pow(2f, (mipAmount - (mipLevel - 1)) + 4); //+4 since we are raising 2 to the x power.
                Color[] mcs = tex.GetPixels(0, 0, mipSize, mipSize, mipLevel - 2);      
                tex = new Texture2D(mipSize, mipSize, TextureFormat.ARGB32, false);
                tex.SetPixels(mcs);
            }
        

            //to facet the submesh, go through each triangle. get each vertex, add it to an new vertex array. do same with uvs and boneweights

            for (int t = 0; t < triList.Length; t++)
            {
                //add the vertex to the new array.
                SMVerts.Add(sourceMesh.vertices[triList[t]]);
                SMUVs.Add(sourceMesh.uv[triList[t]]);
            }

            

            //make vertex colors from the diffuse but using the uvs and verts of the mesh we're making   
            //second time around, vert length is already b
            for (int z = 0; z < SMVerts.Count; z++)
            {
                
                if (filterBilinear)
                {
                    float UVx = SMUVs[z].x;
                    float UVy = SMUVs[z].y;
                    SMVCs.Add(((tex.GetPixelBilinear(UVx, UVy) + (Color32)brightenColor)) * (Color32)matColor);
                }
                else
                {
                    int UVx = (int)(SMUVs[z].x * (tex.width));
                    int UVy = (int)(SMUVs[z].y * (tex.width));
                    SMVCs.Add(((tex.GetPixel(UVx, UVy) + (Color32)brightenColor)) * (Color32)matColor);
                }
              
            }

            //faceted submesh uv, verts, vert colors ready for the submesh. Add them to the master lists.
            foreach (Vector3 v in SMVerts)
                FinalVerts.Add(v);
            foreach (Vector2 v in SMUVs)
                FinalUVS.Add(v);
            foreach (Color32 c in SMVCs)
                FinalVCs.Add(c);
        }
       

        //all done, now recreate the triangle index for the new mesh, then average the vertex colors.
        for (int l = 0; l < FinalVerts.Count; l++)
            FinalTris.Add(l);

        //average the color to get faceted color
        for (int v = 0; v < FinalTris.Count; v += 3)
        {
            Color32 avg;
            int v1 = FinalTris[v];
            int v2 = FinalTris[v + 1];
            int v3 = FinalTris[v + 2];

            Vector3 c1 = new Vector3(FinalVCs[v1].r, FinalVCs[v1].g, FinalVCs[v1].b);
            Vector3 c2 = new Vector3(FinalVCs[v2].r, FinalVCs[v2].g, FinalVCs[v2].b);
            Vector3 c3 = new Vector3(FinalVCs[v3].r, FinalVCs[v3].g, FinalVCs[v3].b);
            Vector3 avgC = (c1 + c2 + c3) / 3;
            avg = new Color32((byte)avgC.x, (byte)avgC.y, (byte)avgC.z, 1);

          
            FinalVCs[v1] = avg;
            FinalVCs[v2] = avg;
            FinalVCs[v3] = avg;
        }


    }
    private void RenderSkinnedMesh(SkinnedMeshRenderer SMR, GameObject currentGO)
    {


        //make a copy of the source mesh
        Mesh sourceMesh = SMR.sharedMesh;

        foreach (Matrix4x4 matrix in sourceMesh.bindposes)
            FinalBPs.Add(matrix);

        //for every submesh, do all the work.
        for (int x = 0; x < sourceMesh.subMeshCount; x++)
        {

            List<Vector3> SMVerts = new List<Vector3>();
            List<Vector2> SMUVs = new List<Vector2>();
            List<Color32> SMVCs = new List<Color32>();
          //  List<int> SMTris = new List<int>();
            List<BoneWeight> SMBWs = new List<BoneWeight>();

            //triangle arrays point to the array index of the vertices in the vertex array
            int[] triList = sourceMesh.GetTriangles(x); //submesh's index number corresponds to the material index of the meshrendere component
            //get the diffuse texture. Hopefully the shader obeys the standard naming convention..

            string path = AssetDatabase.GetAssetPath(SMR.sharedMaterials[x].GetTexture("_MainTex"));
            TextureImporter A = (TextureImporter)AssetImporter.GetAtPath(path);
            A.isReadable = true;
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            Texture2D tex = (Texture2D)SMR.sharedMaterials[x].GetTexture("_MainTex");
            Color matColor = SMR.sharedMaterials[x].GetColor("_Color");


            //Coloring will not work with nonsquare textures.

            //we only want to go as far as 32x32
            int mipAmount = tex.mipmapCount - 4;//(int)Mathf.Log((int)tex.width)+1;            
            mipLevel = PickMipLevel(tex, mipAmount);


            if (mipLevel > 0) //if we chose to do blur amount..
            {
                int mipSize = (int)Mathf.Pow(2f, (mipAmount - (mipLevel - 1)) + 4); //+4 since we are raising 2 to the x power.
                Color[] mcs = tex.GetPixels(0, 0, mipSize, mipSize, mipLevel - 2);                
                tex = new Texture2D(mipSize, mipSize, TextureFormat.ARGB32, false);
                tex.SetPixels(mcs);
            }
        

            //to facet the submesh, go through each triangle. get each vertex, add it to an new vertex array. do same with uvs and boneweights

            for (int t = 0; t < triList.Length; t++)
            {
                //add the vertex to the new array.
                SMVerts.Add(sourceMesh.vertices[triList[t]]);
                SMUVs.Add(sourceMesh.uv[triList[t]]);
                //Add the bone weights
                SMBWs.Add(sourceMesh.boneWeights[triList[t]]);
            }
            //make vertex colors from the diffuse but using the uvs and verts of the mesh we're making   
            //second time around, vert length is already b
            for (int z = 0; z < SMVerts.Count; z++)
            {
                if (filterBilinear)
                {
                    float UVx = SMUVs[z].x;
                    float UVy = SMUVs[z].y;
                    SMVCs.Add(((tex.GetPixelBilinear(UVx, UVy) + (Color32)brightenColor)) * (Color32)matColor);
                }
                else
                {
                    int UVx = (int)(SMUVs[z].x * (tex.width));
                    int UVy = (int)(SMUVs[z].y * (tex.width));
                    SMVCs.Add(((tex.GetPixel(UVx, UVy) + (Color32)brightenColor)) * (Color32)matColor);
                }
            }

            //faceted submesh uv, verts, vert colors ready for the submesh. Add them to the master lists.
            foreach (Vector3 v in SMVerts)
                FinalVerts.Add(v);
            foreach (Vector2 v in SMUVs)
                FinalUVS.Add(v);
            foreach (Color32 c in SMVCs)
                FinalVCs.Add(c);
            foreach (BoneWeight b in SMBWs)
                FinalBWs.Add(b);

        }
        //all done, now recreate the triangle index for the new mesh, then average the vertex colors.

        for (int l = 0; l < FinalVerts.Count; l++)
            FinalTris.Add(l);

        //average the color to get faceted color

        for (int v = 0; v < FinalTris.Count; v += 3)
        {
            Color32 avg;
            int v1 = FinalTris[v];
            int v2 = FinalTris[v + 1];
            int v3 = FinalTris[v + 2];

            Vector3 c1 = new Vector3(FinalVCs[v1].r, FinalVCs[v1].g, FinalVCs[v1].b);
            Vector3 c2 = new Vector3(FinalVCs[v2].r, FinalVCs[v2].g, FinalVCs[v2].b);
            Vector3 c3 = new Vector3(FinalVCs[v3].r, FinalVCs[v3].g, FinalVCs[v3].b);
            Vector3 avgC = (c1 + c2 + c3) / 3;
            avg = new Color32((byte)avgC.x, (byte)avgC.y, (byte)avgC.z, 1);

            FinalVCs[v1] = avg;
            FinalVCs[v2] = avg;
            FinalVCs[v3] = avg;
        }


    }

    private int PickMipLevel(Texture2D tex, int mipAmount)
    {
        int ml = 0;

        if (blurAmount == BlurAmount.None)
            return ml;
        else
        {
            if (blurAmount == BlurAmount.Some)
                ml = mipAmount / 2;
            else
                ml = mipAmount;
        }
        return ml;
    }
    private Material CreateMasterMaterial(string folderPath, string matName)
    {
        //write the material. There will be only one since all the GO's will use it.
        Material newMat = new Material(Shader.Find("QuantumTheory/VertexColors/IBL/Diffuse"));
        newMat.name = matName;
        AssetDatabase.CreateAsset(newMat, folderPath + "/" + newMat.name + ".asset");
        return newMat;
    }
    private void CreateMesh(Mesh vcmesh, string folderPath)
    {
        //write the new meshes to the project. Will overwrite if one is found.
        vcmesh.name = vcmesh.name.Replace("(Clone)", "");
        AssetDatabase.CreateAsset(vcmesh, folderPath + "/" + vcmesh.name + ".asset");
        AssetDatabase.Refresh();
    }
    private void UpdatePrefab(GameObject WorkingGO, GameObject[] MeshGOs, bool[] isSkinnedMesh, string folderPath)
    {
        string pfPath = folderPath + "/" + WorkingGO.name + "-Faceted.prefab";
       // string pfPath = AssetDatabase.GetAssetPath(PrefabUtility.GetPrefabParent(WorkingGO));
        GameObject sourcePF = (GameObject)AssetDatabase.LoadAssetAtPath(pfPath, typeof(GameObject));

        if (sourcePF == null)
            PrefabUtility.CreatePrefab(pfPath, WorkingGO);
        else
            PrefabUtility.ReplacePrefab(WorkingGO, sourcePF, ReplacePrefabOptions.ReplaceNameBased);        
        AssetDatabase.Refresh();
    }

}
