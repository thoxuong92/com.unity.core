using game;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAttributes : MonoBehaviour
{
    public ReorderableDictionary<string, string> dictionary = new ReorderableDictionary<string, string>();
    [URL("http://google.com/")]
    [HorizontalLine("Title")]
    [ReadOnly]
    public string textRead = "ReadOnly";
    [Layer]
    public int layerEnemy;

    [Tag]
    public string TagPlayer;
    [Preview(Size.medium)] public Sprite sprite;

    public bool isShowText;
    [ShowIf(nameof(isShowText))]
    public string Text = "Hello world";

    [ForceFill(errorMessage = "not null")]
    public GameObject Target;

    [Button(nameof(TestButton))]
    void TestButton()
    {
        Debug.Log("TestButton");
        List<int> list = null;
        Vector2[] array = null;
        if (list.IsNullOrEmpty())
        {
            Debug.Log("List is null");
        }
        if (array.IsNullOrEmpty())
        {
            Debug.Log("Array is null");
        }
    }
    [Scene]
    public int sceneName;
    [SceneButton]
    public void OnClickButton()
    {
        Debug.Log("OnClickButton");
    }
    [DrawCube(1, BaseColor.Red, true, 0.2f)]
    public Vector3 posCube;
    [DrawSphere(1, BaseColor.Green, true, 0.2f)]
    public Vector3 posCircle;

    [DrawLine(IsLocal =true)]
    public Vector3 Next;
}
