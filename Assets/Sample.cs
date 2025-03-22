using UnityEngine;
using UnityEngine.UI;

public class Sample : MonoBehaviour
{
    [SerializeField] private Button Draw;

    [SerializeField] private Button Clear;

    [SerializeField] private Button All;

    [SerializeField] private Slider Position;

    [SerializeField] private Slider AutoForwardSpeed;

    [SerializeField] private Toggle IsDrawAtOnce;

    [SerializeField] private RubyText RubyText;

    // Start is called before the first frame update
    private void Start()
    {
        RubyText.SetWH(900, 300);
        RubyText.SetFontSize(160);

        RubyText.SetText(
            "この<color=red>{数:かず}</color>は、{一般的:いっぱんてき}に\r\n「{ナ:na}{ノ:no}」という\r\n{単位:たんい}{接頭辞:せっとうじ}を{使用:しよう}して{表:あらわ}されます。");
        
        Position.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.ForceTextPosition((int)(RubyText.GetTextLength() * val));
            }
        );
        
        AutoForwardSpeed.value = RubyText.AutoForwardSpeed;
        AutoForwardSpeed.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.AutoForwardSpeed = val;
            }
        );
        
        IsDrawAtOnce.isOn = RubyText.IsDrawAtOnce;
        IsDrawAtOnce.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.IsDrawAtOnce = val;
            }
        );
        
        Draw.onClick.AddListener(
            () =>
            {
                RubyText.SetText("この<color=red>{数:かず}</color>は、{一般的:いっぱんてき}に\r\n「{ナ:na}{ノ:no}」という\r\n{単位:たんい}{接頭辞:せっとうじ}を{使用:しよう}して{表:あらわ}されます。");
                RubyText.StartAutoForward();
            }
        );

        Clear.onClick.AddListener(
            () =>
            {
                RubyText.SetText("");
            }
        );

        All.onClick.AddListener(
            () =>
            {
                RubyText.ForceTextDrawAll();
            }
        );
    }
}