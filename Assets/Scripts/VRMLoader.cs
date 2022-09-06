using KiliWare.SampleVRMApp;
using System.Collections;
using TMPro;
using UniGLTF;
using UnityEngine;
using UnityEngine.Networking;
using VRM;

public class VRMLoader : MonoBehaviour
{
    [SerializeField]
    private string VRM_Name;
    [SerializeField]
    private TextMeshProUGUI Stopwatch_TMP;
    [SerializeField] 
    RuntimeAnimatorController controller;
    [SerializeField]
    public PoseManager poseManager;

    //スタート時に呼ばれる
    void Start()
    {
        StartCoroutine("LoadVRM", VRM_Name);
    }

    IEnumerator LoadVRM(string name)
    {
        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        string path = Application.streamingAssetsPath + "/" + name + ".vrm";

#if UNITY_EDITOR

        var parser = new GlbFileParser(path);
        var glbData = parser.Parse();
        var vrmData = new VRMData(glbData);

        using (var context = new VRMImporterContext(vrmData))
        {
            RuntimeGltfInstance instance = context.Load();

            instance.ShowMeshes();

            sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + "ms");
            Stopwatch_TMP.text = sw.ElapsedMilliseconds + "ms";

            GameObject vrm = GameObject.Find("VRM");
            Animator anim =vrm.GetComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            poseManager._animators.Add(anim);

            yield return instance.Root;
        }

#else
        UnityWebRequest webRequest = UnityWebRequest.Get(path);
        yield return webRequest.SendWebRequest();
        if (webRequest.isNetworkError)
        {
            print(webRequest.error);
            yield break;
        }

        using var gltfData = new GlbBinaryParser(webRequest.downloadHandler.data, "").Parse();
        var vrmData = new VRMData(gltfData);

        using (var context = new VRMImporterContext(vrmData))
        {
            RuntimeGltfInstance instance = context.Load();

            instance.ShowMeshes();

            sw.Stop();
            Debug.Log(sw.ElapsedMilliseconds + "ms");
            Stopwatch_TMP.text = sw.ElapsedMilliseconds + "ms";

            GameObject vrm = GameObject.Find("VRM");
            Animator anim =vrm.GetComponent<Animator>();
            anim.runtimeAnimatorController = controller;
            poseManager._animators.Add(anim);

            yield return instance.Root;
        }

#endif

    }

}