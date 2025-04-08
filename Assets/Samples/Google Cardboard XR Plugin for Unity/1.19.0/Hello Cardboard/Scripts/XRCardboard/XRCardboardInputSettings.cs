using UnityEngine;

[CreateAssetMenu(fileName = "XRCardboardInputSettings", menuName = "Google Cardboard/Cardboard Input Settings")]
public class XRCardboardInputSettings : ScriptableObject
{
    [field: SerializeField]
    public string ClickInput { get; } = "Submit";
    [field: SerializeField]
    public bool ClickOnHover { get; } = false;
    [field: SerializeField, Range(.5f, 5)]
    public float GazeTime { get; } = 2f;
}