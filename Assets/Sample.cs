using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sample : MonoBehaviour
{
    [SerializeField]
    Button       Draw;

    [SerializeField]
    Button       Clear;

    [SerializeField]
    Button       All;

    [SerializeField]
    Slider       Position;

    [SerializeField]
    Slider       AutoForwardSpeed;

    [SerializeField]
    Toggle       IsDrawAtOnce;

    [SerializeField]
    RubyText     RubyText;

    // Start is called before the first frame update
    void Start()
    {
        RubyText.SetWH(900, 300);
        RubyText.SetFontSize(160);

        RubyText.SetText("����<color=red>{��:����}</color>�́A{��ʓI:�����ς�Ă�}��\r\n�u{�i:na}{�m:no}�v�Ƃ���\r\n{�P��:����}{�ړ���:�����Ƃ���}��{�g�p:���悤}����{�\:�����}����܂��B");

        // �\���ʒu�̐ݒ�
        Position.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.ForceTextPosition(RubyText.GetTextLength() * val);
            }
        );

        // �\�����x
        AutoForwardSpeed.value = RubyText.AutoForwardSpeed;
        AutoForwardSpeed.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.AutoForwardSpeed = val;
            }
        );

        // true... ���͂��P�x�ɕ\��
        // false...�P�������\��
        IsDrawAtOnce.isOn = RubyText.IsDrawAtOnce;
        IsDrawAtOnce.onValueChanged.AddListener(
            (val) =>
            {
                RubyText.IsDrawAtOnce = val;
            }
        );

        // �\���J�n
        Draw.onClick.AddListener(
            () =>
            {
                RubyText.SetText("����<color=red>{��:����}</color>�́A{��ʓI:�����ς�Ă�}��\r\n�u{�i:na}{�m:no}�v�Ƃ���\r\n{�P��:����}{�ړ���:�����Ƃ���}��{�g�p:���悤}����{�\:�����}����܂��B");
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
