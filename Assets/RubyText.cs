using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.Text.RegularExpressions;

public class RubyText : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI     Text;

    [SerializeField]
    RectTransform       TextRect;

    [SerializeField]
    TextMeshProUGUI     Ruby;

    [SerializeField, Tooltip("�P�����A�܂��͕��͑S�̂�\�����鎞�ԁi�b�j"), Range(0.01f, 1)]
    public float        AutoForwardSpeed;

    [SerializeField, Tooltip("true/�P�x�ɑS���͂�\���Afalse/�P�������\��")]
    public bool         IsDrawAtOnce;

    class TextRuby
    {
        public int          TextPosition;
        public string       Word;
        public string       RubyWord;

        TextMeshProUGUI     parentText;
        TextMeshProUGUI     ruby;
        RectTransform       rubyRect;

        TMP_CharacterInfo[] characterInfos;
        int                 posTop;
        int                 posBtm;

        float               rubyWidth;

        /// <summary>
        /// .ctor
        /// </summary>
        public TextRuby(int no, TextMeshProUGUI rubyBase, TextMeshProUGUI _parentText)
        {
            parentText = _parentText;
            rubyWidth  = 0;

            ruby = Instantiate(rubyBase, parentText.transform);
            ruby.name = $"ruby {no}";

            rubyRect = ruby.GetComponent<RectTransform>();
            rubyRect.SetHeight(ruby.fontSize);
        }

        /// <summary>
        /// SetActive
        /// </summary>
        public void SetActive(bool active)
        {
            ruby.SetActive(active);
        }

        /// <summary>
        /// ���r���[�h�̐ݒ�A�\���͈͂̐ݒ�
        /// </summary>
        public void Refresh()
        {
            float textWidth = (characterInfos[posBtm].topRight.x - characterInfos[posTop].topLeft.x);

            ruby.SetActive(false);
            ruby.SetText(RubyWord);
            if (RubyWord.Length == 1)
            {
                // center
                ruby.alignment = TextAlignmentOptions.Bottom;
            }
            else
            {
                // ���悭�Ԃ��󂯂�
                ruby.alignment = TextAlignmentOptions.BottomFlush;
            }

            rubyWidth = ruby.preferredWidth;
            if (rubyWidth < textWidth * 0.9f)
            {
                rubyRect.SetWidth(textWidth * 0.9f);
            }
            else
            {
                rubyRect.SetWidth(rubyWidth);
            }
            rubyRect.SetHeight(ruby.preferredHeight);
        }

        /// <summary>
        /// �N���A
        /// </summary>
        public void Clear()
        {
            ruby.SetActive(false);
            ruby.SetText("");
        }

        /// <summary>
        /// ���̍X�V
        /// </summary>
        /// <param name="messagePosition">���݂̍ŏI�����\���ʒu</param>
        /// <param name="a">�ŏI�����̃�</param>
        public void UpdateAlpha(int messagePosition, float a)
        {
            if (posBtm > messagePosition)
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, 0);
            }
            else
            if (posBtm < messagePosition)
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, 1);
            }
            else
            {
                ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, a);
            }
        }

        /// <summary>
        /// ���̍X�V
        /// </summary>
        /// <param name="a">�ŏI�����̃�</param>
        public void UpdateAlpha(float a)
        {
            ruby.color = new Color(ruby.color.r, ruby.color.g, ruby.color.b, a);
        }

        /// <summary>
        /// ���r�t�H���g�T�C�Y�̐ݒ�
        /// </summary>
        /// <param name="fontSize">�{���t�H���g�̃T�C�Y</param>
        public void SetFontSize(float fontSize)
        {
            ruby.fontSize    = 
            ruby.fontSizeMax = fontSize/2;
        }

        /// <summary>
        /// �v�Z�ς݂̕�������ݒ�
        /// </summary>
        /// <param name="characterInfos">�v�Z�ς݂̕������</param>
        /// <param name="posTop">���r�擪�����ʒu</param>
        /// <param name="posBtm">���r�I�������ʒu</param>
        public void SetTmpInfo(TMP_CharacterInfo[] characterInfos, int posTop, int posBtm)
        {
            this.characterInfos = characterInfos;
            this.posTop         = posTop;
            this.posBtm         = posBtm;

            var top = this.characterInfos[this.posTop];
            var btm = this.characterInfos[posBtm];

            float y = top.ascender;
            float x = (top.topLeft.x + btm.topRight.x) / 2;

            rubyRect.SetXY(x, y);

            float r = (float)this.characterInfos[this.posBtm].color.r / 255;
            float g = (float)this.characterInfos[this.posBtm].color.g / 255;
            float b = (float)this.characterInfos[this.posBtm].color.b / 255;

            ruby.color = new Color(r, g, b, 0);
        }
    }

    /// <summary>
    /// �O��l. ���ݒl�Ɣ�r���A�Ⴂ���������炻�ꂼ����X�V
    /// </summary>
    class UpdateComparer
    {
        public string Message;
        public int    Position;
        public float  Alpha;
        public float  W, H;
        public bool   EnableWordWrapping;
    }

    List<TextRuby>              textRubys;
    int                         textRubyCount;
    UpdateComparer              updateComparer;
    TMP_CharacterInfo[]         cinfos;

    float                       fontSizeMax;
    string                      message;
    List<int>                   positionIndexes;

    int                         position;
    float                       alpha;

    Coroutine                   co_text;
    Coroutine                   co_auto;

    /// <summary>
    /// awake
    /// </summary>
    void Awake()
    {
        Ruby.SetActive(false);

        textRubys = new List<TextRuby>();
        updateComparer = new UpdateComparer();
        updateComparer.EnableWordWrapping = Text.enableWordWrapping;

        fontSizeMax = Text.fontSizeMax;
    }

    /// <summary>
    /// update
    /// </summary>
    void Update()
    {
        bool update = false;

        if (updateComparer.W != TextRect.GetWidth() || updateComparer.H != TextRect.GetHeight())
        {
            updateComparer.W = TextRect.GetWidth();
            updateComparer.H = TextRect.GetHeight();
            update = true;
        }
        if (updateComparer.EnableWordWrapping != Text.enableWordWrapping)
        {
            updateComparer.EnableWordWrapping = Text.enableWordWrapping;
            update = true;
        }

        if (update == true)
        {
            // �ĕ`��
            updateComparer.Position = 0;
            updateComparer.Alpha = 0;

            this.StartSingleCoroutine(ref co_text, textDrawing(false));
        }
    }

    /// <summary>
    /// on destroy
    /// </summary>
    void OnDestroy()
    {
        this.StopSingleCoroutine(ref co_auto);
        this.StopSingleCoroutine(ref co_text);
    }

    /// <summary>
    /// ������������̑��x��ݒ肷��
    /// </summary>
    /// <param name="_secPerWord">���b�łP�����\��</param>
    public void SetAutoForwardSpeed(float _secPerWord)
    {
        AutoForwardSpeed = _secPerWord;
    }

    /// <summary>
    /// ������������
    /// </summary>
    public void StartAutoForward()
    {
        if (AutoForwardSpeed == 0)
        {
            AutoForwardSpeed = 0.05f;
        }
        this.StartSingleCoroutine(ref co_auto, autoForward());
    }

    /// <summary>
    /// ���͂̕\���ʒu����������
    /// </summary>
    /// <param name="pos">�����F�ŏI�\�������ʒu�A�����F��</param>
    public void ForceTextPosition(float pos)
    {
        this.StopSingleCoroutine(ref co_auto);

        setTextPosition(pos);
    }

    /// <summary>
    /// ���͂̕\���ʒu����������
    /// </summary>
    /// <param name="pos">�����F�ŏI�\�������ʒu�A�����F��</param>
    public void ForceTextDrawAll()
    {
        ForceTextPosition(GetTextLength());
    }

    /// <summary>
    /// �\�����镶�͂̐ݒ�. {����:����} �̏����Ń��r��\������
    /// </summary>
    /// <param name="_message">�\�����镶��</param>
    public void SetText(string _message)
    {
        message = _message;

        Text.SetText("");
        Text.fontSizeMax = fontSizeMax;

        textRubys.ForEach( ruby => ruby.Clear() );
        textRubyCount = 0;

        position = 0;
        alpha = 0;

        // <> �` </> �R�}���h�Ȃ�
        var notagMessage = Regex.Replace(message, "<[^<|>]+>", "");

        for ( ; ; )
        {
            int top = notagMessage.IndexOf("{");
            int btm = notagMessage.IndexOf("}");

            if (top < 0 || btm < 0)
            {
                break;
            }

            // �R�}���h
            string   command = notagMessage.Substring(top, btm-top+1).Replace("{", "").Replace("}", "");
            string[] coms    = command.Split(':');

            if (textRubyCount+1 > textRubys.Count)
            {
                textRubys.Add(new TextRuby(textRubyCount, Ruby, Text));
            }

            var ruby = textRubys[textRubyCount++];
            ruby.TextPosition = top;
            ruby.Word         = coms[0];
            ruby.RubyWord     = coms[1];

            //Debug.Log($"{top} {string.Join(",", coms)}");

            notagMessage      = notagMessage.Remove(top, btm-top+1).Insert(top, coms[0]);
        }

        // {} �R�}���h�Ȃ��́ATextMeshProUGUI �ɓn���e�L�X�g
        message = Regex.Replace(message, ":[^\\}]+\\}", "").Replace("{", "");

        // �������ڂ������񂎔Ԗڂ���\������邱�Ƃ��m�F���郊�X�g
        // �i�^�O���������邽�� �������j
        positionIndexes = new List<int>();
        for (int i = 0; i < message.Length; i++)
        {
            if (message[i] == '<')
            {
                i = message.IndexOf('>', i);
                continue;
            }
            positionIndexes.Add(i);
        }

        this.StartSingleCoroutine(ref co_text, textDrawing(true));

//Debug.Log($"{positionIndexes.Count} {message}");
    }

    /// <summary>
    /// �e�L�X�g�̕��������擾�B�^�O�͏���
    /// </summary>
    public int GetTextLength()
    {
        return positionIndexes == null ? 0 : positionIndexes.Count;
    }

    /// <summary>
    /// RectXY
    /// </summary>
    public void SetXY(float x, float y)
    {
        SetX(x);
        SetY(y);
    }

    /// <summary>
    /// RectX
    /// </summary>
    public void SetX(float x)
    {
        TextRect.SetX(x);
    }

    /// <summary>
    /// RectY
    /// </summary>
    public void SetY(float y)
    {
        TextRect.SetY(y);
    }

    /// <summary>
    /// RectWH
    /// </summary>
    public void SetWH(float width, float height)
    {
        SetWidth(width);
        SetHeight(height);
    }

    /// <summary>
    /// RectW
    /// </summary>
    public void SetWidth(float width)
    {
        TextRect.SetWidth(width);
        updateComparer.W = width;
    }

    /// <summary>
    /// RectH
    /// </summary>
    public void SetHeight(float height)
    {
        TextRect.SetHeight(height);
        updateComparer.H = height;
    }

    /// <summary>
    /// �t�H���g�� AutoSize ��ݒ肷��
    /// </summary>
    public void SetFontAutoSize(float min, float max)
    {
        Text.enableAutoSizing = true;
        Text.fontSizeMin = min;
        Text.fontSizeMax = max;
        Text.fontSize    = max;
        fontSizeMax      = max;
    }

    /// <summary>
    /// �t�H���g�T�C�Y��ݒ肷��
    /// </summary>
    public void SetFontSize(float size)
    {
        Text.fontSizeMax = size;
        Text.fontSize    = size;
        fontSizeMax      = size;
    }

    /// <summary>
    /// ������������
    /// </summary>
    /// <returns></returns>
    IEnumerator autoForward()
    {
        int   max = positionIndexes.Count;
        float time = 0;

        yield return null;

        for ( ; time < max; )
        {
            time += Time.deltaTime * (1.0f / AutoForwardSpeed);
            
            setTextPosition(time);

            yield return null;
        }

        co_auto = null;
    }

    /// <summary>
    /// �e�L�X�g�`��
    /// </summary>
    IEnumerator textDrawing(bool calculate)
    {
        textRubys.ForEach( ruby => ruby.Clear() );

        if (calculate == true)
        {
            Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 0);

            // ��U�e�L�X�g�������Ȃ���ԂőS�e�L�X�g��`�悷��i���������擾���邽�߁j
            Text.SetText(message);

        }

        // �P�t���[���o�߂��Ȃ��ƕ�����񂪍X�V����Ȃ�
        yield return null;

        // �S�������\�����ꂽ��Ԃ̃t�H���g�T�C�Y���t�H���g�ő�T�C�Y�Ƃ���
        // �i��������Ă����Ȃ��ƁA���������Ȃ����o�J�ł��������ɂȂ�ȂǁA�s����ȃe�L�X�g�`��ɂȂ�j
        Text.fontSizeMax = Text.fontSize;

        // �o������������������Ƀ��r��ݒ�
        refreshRuby();

        Text.SetText("");

        // �J���[��߂�
        Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 1);

        while (true)
        {
            var msg = Text.text;

//DDisp.Log($"{position} {alpha}");
            // ���͂��ς������A�\���ʒu���ω�
            if (updateComparer.Message != message || updateComparer.Position != position)
            {
                if (position >= 0 && position < positionIndexes.Count)
                {
                    int    ia   = (int)(255 * alpha);
                    string taga = $"<alpha=#{ia.ToString("x2")}>";

                    if (IsDrawAtOnce == true)
                    {
                        msg = taga + message;
                        msg = Regex.Replace(msg, $"(?<tag><color=[^>]+>)", "${tag}" + taga);
                        msg = msg.Replace("</color>", "</color>" + taga);
                        refreshRubyAlpha(alpha);
                    }
                    else
                    {
                        msg = message.Substring(0, positionIndexes[position]+1);

                        if (msg.Length >= 1)
                        {
                            msg = msg.Insert(positionIndexes[position], taga);

                            refreshRubyAlpha(alpha);
                        }
                    }
                }

                updateComparer.Message = message;
                updateComparer.Position = position;
                updateComparer.Alpha = alpha;
            }
            else
            // �ŏI�����̃����ω�
            if (updateComparer.Alpha != alpha)
            {
                if (msg.Length > 1)
                {
                    int ia = (int)(255 * alpha);
                    msg = Regex.Replace(msg, $"<alpha=[^>]+>", $"<alpha=#{ia.ToString("x2")}>");

                    refreshRubyAlpha(alpha);
                }

                updateComparer.Alpha = alpha;
            }

            if (Text.text != msg)
            {
//Debug.Log($"{msg}");
                Text.SetText(msg);
            }

            yield return null;
        }
    }

    /// <summary>
    /// ���r�̍Đݒ�
    /// </summary>
    void refreshRuby()
    {
        for (int i = 0; i < textRubys.Count; i++)
        {
            textRubys[i].SetActive(false);
        }

        // �����`���̕������i�\���ʒu��J���[�Ȃǁj
        cinfos = Text.GetTextInfo(Text.text).characterInfo;

        for (int i = 0; i < textRubys.Count; i++)
        {
            var ruby    = textRubys[i];

            int posTop  = ruby.TextPosition;
            int posBtm;

            var infoTop = cinfos[posTop];

            // ���r��U�镶����̂����A���������̏I�[��������������
            // �i�����񂪎������s�ȂǂłQ�s�ɂ܂������Ă��܂����̑΍�j
            for (posBtm = ruby.TextPosition + ruby.Word.Length - 1; ; posBtm--)
            {
                var info = cinfos[posBtm];
                if (infoTop.ascender == info.ascender)
                {
                    break;
                }
            }

            ruby.SetTmpInfo(cinfos, posTop, posBtm);
            ruby.SetFontSize(Text.fontSize);
            ruby.Refresh();
            ruby.SetActive(true);
        }

        refreshRubyAlpha(0);
    }

    /// <summary>
    /// ���r�̕\���i���X�V�j
    /// </summary>
    void refreshRubyAlpha(float alpha)
    {
        if (IsDrawAtOnce == true)
        {
            textRubys.ForEach( ruby => ruby.UpdateAlpha(alpha) );
        }
        else
        {
            textRubys.ForEach( ruby => ruby.UpdateAlpha(position, alpha) );
        }
    }

    /// <summary>
    /// �����̕\���ʒu�ݒ�
    /// </summary>
    /// <param name="pos">�����F�ŏI�\�������ʒu�A�����F��</param>
    void setTextPosition(float pos)
    {
        int count = GetTextLength();

        if (IsDrawAtOnce == true)
        {
            this.position = count - 1;
            alpha = pos;
            if (alpha > 1)
            {
                alpha = 1;
            }
        }
        else
        {
            if (pos < 0)
            {
                this.position = 0;
                alpha         = 0;
            }
            else
            if (pos >= count)
            {
                this.position = count - 1;
                alpha         = 1;
            }
            else
            {
                this.position = (int)pos;
                alpha         = pos - this.position;
            }
        }
    }

}
