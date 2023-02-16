using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Text))]
public class NetDebugAttach : MonoBehaviour
{
    private Text myText;

    // Use this for initialization
    private void Awake()
    {
        myText = GetComponent<Text>();
    }

    private void Update()
    {
        myText.text = NetDebug.getText();
    }
}